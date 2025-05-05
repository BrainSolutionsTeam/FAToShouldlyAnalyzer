using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentAssertionsToShouldlyAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FluentAssertionsToShouldlyCodeFixProvider)), Shared]
    public class FluentAssertionsToShouldlyCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(FluentAssertionsToShouldlyAnalyzer.DiagnosticId);

        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        public sealed override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);


            var diagnostic = context.Diagnostics.First();


            //Register code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.CodeFixTitle,
                    createChangedSolution: c => ChangeShouldMethodCall(root, semanticModel, context.Document),
                    equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                diagnostic);
        }

        private static Task<Solution> ChangeShouldMethodCall(SyntaxNode root, SemanticModel semanticModel, Document document)
        {
            var invocationExpressions = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            var faReplacement = GetFaReplacement();

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

                // Check the next method in the chain
                if ((memberAccess.Parent.Parent.Parent is InvocationExpressionSyntax nextInvocation
                     && nextInvocation.Expression is MemberAccessExpressionSyntax nextMemberAccess
                     && semanticModel.GetSymbolInfo(nextMemberAccess).Symbol is IMethodSymbol nextMethodSymbol) is false)
                {
                    continue;
                }

                // Handle specific case before this, like Should Throw ? 

                if (faReplacement.TryGetValue(nextMethodSymbol.Name, out var value) is false)
                {
                    continue;
                }

                var updatedInvocation = nextInvocation
                    .WithExpression(SyntaxFactory.IdentifierName(value))
                    .WithArgumentList(invocation.ArgumentList);

                root = root.ReplaceNode(nextInvocation, updatedInvocation);
            }

            // Return the updated solution
            var updatedDocument = document.WithSyntaxRoot(root);
            return Task.FromResult(updatedDocument.Project.Solution);
        }

        private static Dictionary<string, string> GetFaReplacement() =>
            new Dictionary<string, string>()
            {
                ["Be"] = "ShouldBe",
                ["NotBe"] = "ShouldNotBe",
                ["BeNull"] = "ShouldBeNull",
                ["NotBeNull"] = "ShouldNotBeNull",
            };
    }
}
