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
                    createChangedSolution: c => ChangeShouldMethodCall(
                        root,
                        semanticModel,
                        context.Document,
                        diagnostic),
                    equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                diagnostic);
        }

        private static Task<Solution> ChangeShouldMethodCall(
            SyntaxNode root,
            SemanticModel semanticModel,
            Document document,
            Diagnostic diagnostic)
        {
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var invocationNode = root.FindNode(diagnosticSpan);

            if ((invocationNode is IdentifierNameSyntax invocation                                              // invocation = Should
                  && invocation.Parent is MemberAccessExpressionSyntax memberAccess                             // memberAccess = person.Name.Should
                  && memberAccess.Expression is MemberAccessExpressionSyntax memberAccessExpression             // memberAccessExpression = person.Name
                  && memberAccessExpression.Expression is IdentifierNameSyntax identifierName                   // identifierName = person
                ) is false)
            {
                return Task.FromResult(document.Project.Solution);
            }

            var symbolInfo = semanticModel.GetSymbolInfo(memberAccess);

            if (FluentAssertionsToShouldlyAnalyzer.IsCallingShouldFromFa(memberAccess, symbolInfo) is false)
            {
                return Task.FromResult(document.Project.Solution);
            }

            // Check the next method in the chain
            if ((memberAccess.Parent is InvocationExpressionSyntax shouldMethodInvocation                       // shouldMethodInvocation = person.Name.Should()
                 && shouldMethodInvocation.Parent is MemberAccessExpressionSyntax nextMemberAccess              // nextMemberAccess = person.Name.Should().Be, .NotBe, ...
                 && nextMemberAccess.Parent is InvocationExpressionSyntax nextInvocation                        // nextInvocation = person.Name.Should().Be("xxx")
                 && semanticModel.GetSymbolInfo(nextMemberAccess).Symbol is IMethodSymbol nextMethodSymbol      // nextMethodSymbol = MethodSymbol of `Be`, `NotBe`, etc.
                 ) is false)
            {
                return Task.FromResult(document.Project.Solution);
            }

            // TODO: Handle specific case before basic cases, like Should Throw ??

            var faReplacement = GetFaReplacement();

            if (faReplacement.TryGetValue(nextMethodSymbol.Name, out var shouldlyMethod) is false)
            {
                return Task.FromResult(document.Project.Solution);
            }

            // TODO: OKAY here

            // Go for replacement:
            var newMemberAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                memberAccessExpression,
                SyntaxFactory.IdentifierName(shouldlyMethod));

            var updatedInvocation = SyntaxFactory.InvocationExpression(newMemberAccess, nextInvocation.ArgumentList);

            root = root.ReplaceNode(nextInvocation, updatedInvocation);

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
