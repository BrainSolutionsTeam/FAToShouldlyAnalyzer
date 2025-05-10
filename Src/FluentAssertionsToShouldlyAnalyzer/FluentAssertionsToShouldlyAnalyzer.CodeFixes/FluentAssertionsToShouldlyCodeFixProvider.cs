using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace FluentAssertionsToShouldlyAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FluentAssertionsToShouldlyCodeFixProvider)), Shared]
    public class FluentAssertionsToShouldlyCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(FluentAssertionsToShouldlyAnalyzer.DiagnosticId);

        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        public sealed override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context) => Task.CompletedTask;
    }
}
