using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace FluentAssertionsToShouldlyAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FluentAssertionsToShouldlyCodeFixProvider)), Shared]
    public class FluentAssertionsToShouldlyCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(global::FluentAssertionsToShouldlyAnalyzer.FluentAssertionsToShouldlyAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            return Task.CompletedTask;
            //var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            //var diagnostic = context.Diagnostics.First();
            //var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            //var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            //context.RegisterCodeFix(
            //    CodeAction.Create(
            //        title: CodeFixResources.CodeFixTitle,
            //        createChangedSolution: c => MakeUppercaseAsync(context.Document, declaration, c),
            //        equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
            //    diagnostic);
        }

        private async Task<Solution> MakeUppercaseAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            // Compute new uppercase name.
            var identifierToken = typeDecl.Identifier;
            var newName = identifierToken.Text.ToUpperInvariant();

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }

        private async Task<Solution> ChangeShouldBeCall(Document document)
        {
            var root = await document.GetSyntaxRootAsync().ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);

            var invocationExpressions = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocationExpressions)
            {
                if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
                {
                    continue;
                }

                var symbolInfo = semanticModel.GetSymbolInfo(memberAccess);

                if (FluentAssertionsToShouldlyAnalyzer.IsCallingShouldFromFa(memberAccess, symbolInfo) is false)
                {
                    continue;
                }

                var methodSymbol = symbolInfo.Symbol as IMethodSymbol;

                // Check the next method in the chain
                if ((memberAccess.Parent is InvocationExpressionSyntax nextInvocation
                     && nextInvocation.Expression is MemberAccessExpressionSyntax nextMemberAccess
                     && semanticModel.GetSymbolInfo(nextMemberAccess).Symbol is IMethodSymbol nextMethodSymbol) is false)
                {
                    continue;
                }
                switch (nextMethodSymbol.Name)
                {
                    case "Be":
                        // Suggest replacement for "Should().Be()"
                        // Example: Replace with "ShouldBe()"
                        break;

                    case "BeGreaterThan":
                        // Suggest replacement for "Should().BeGreaterThan()"
                        // Example: Replace with "ShouldBeGreaterThan()"
                        break;

                    // Add more cases as needed for other methods
                    default:
                        // Handle other cases or do nothing
                        break;
                }
            }

            // Return the updated solution
            var updatedDocument = document.WithSyntaxRoot(root);
            return updatedDocument.Project.Solution;
        }
    }
}
