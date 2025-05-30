﻿using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FluentAssertionsToShouldlyAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FluentAssertionsToShouldlyAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FATSH001";

        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle),
            Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager,
                typeof(Resources));

        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager,
                typeof(Resources));

        private const string Category = "Usage";

        private const string HelpLinkUri = "https://github.com/BrainSolutionsTeam/FluentAssertionsToShouldlyAnalyzer";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: Title,
            messageFormat: MessageFormat,
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description,
            helpLinkUri: HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private const string FaShouldPrefix = "Should";
        private const string FaNamespace = "FluentAssertions";

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpression = (InvocationExpressionSyntax)context.Node;
            if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess
                && FaShouldPrefix.Equals(memberAccess.Name.Identifier.Text, StringComparison.InvariantCulture))
            {
                // Found a call to a method with the name "Should"
                var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess);

                if (symbolInfo.Symbol is IMethodSymbol methodSymbol
                    && methodSymbol.ContainingNamespace.ToString().StartsWith(FaNamespace))
                {
                    // The method is in the FluentAssertions namespace
                    var diagnostic = Diagnostic.Create(Rule, memberAccess.Name.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
