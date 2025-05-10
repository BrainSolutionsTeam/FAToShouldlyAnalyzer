using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertionsToShouldlyAnalyzer.Test.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluentAssertionsToShouldlyAnalyzer.Test
{
    [TestClass]
    public class FluentAssertionsToShouldlyAnalyzerUnitTests
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestNoIssue()
        {
            var test = @"";

            await VerifyCs.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        [DataRow(001, "UnitTests.txt", new[] { 0, 1, 2 })]
        public async Task TestShouldCodeFix(int index, string sourceCodePath, int[] diagnosticLocations)
        {
            // Arrange
            var sourceCode = await File.ReadAllTextAsync(Path.Combine(GetImportPath(), sourceCodePath));

            // Act
            var expectedDiagnostics = diagnosticLocations
                .Select(loc => VerifyCs
                    .Diagnostic(FluentAssertionsToShouldlyAnalyzer.Rule)
                    .WithLocation(loc))
                .ToArray();

            // Assert
            await VerifyCs.VerifyAnalyzerAsync(sourceCode, expectedDiagnostics);
        }

        #region Helpers

        private static string GetImportPath()
        {
            string[] importPaths =
            {
                @".\TestCases", @"TestCases", @"..\TestCases", @"..\..\TestCases", @"..\..\..\TestCases", @"..\..\..\..\TestCases"
            };
            var importPath = importPaths.FirstOrDefault(Directory.Exists);
            if (importPath is null)
            {
                throw new ArgumentNullException(nameof(importPath));
            }
            return importPath;
        }

        private static class VerifyCs
        {
            public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
                => CSharpCodeFixVerifier<
                    FluentAssertionsToShouldlyAnalyzer,
                    FluentAssertionsToShouldlyCodeFixProvider>.Diagnostic(descriptor);

            private static CSharpCodeFixVerifier<
                FluentAssertionsToShouldlyAnalyzer,
                FluentAssertionsToShouldlyCodeFixProvider>.Test CreateTest()
            {
                var test = new CSharpCodeFixVerifier<
                        FluentAssertionsToShouldlyAnalyzer,
                        FluentAssertionsToShouldlyCodeFixProvider>.Test
                {
                    ReferenceAssemblies = ReferenceAssemblies.Default
                            .AddPackages(ImmutableArray.Create(
                                    new PackageIdentity("FluentAssertions", "8.2.0"),
                                    new PackageIdentity("xunit", "2.5.3"),
                                    new PackageIdentity("xunit.runner.visualstudio", "2.5.3"),
                                    new PackageIdentity("Shouldly", "4.3.0")
                                )
                            )
                };

                return test;
            }

            public static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
            {
                var test = CreateTest();
                test.TestCode = source;
                test.ExpectedDiagnostics.AddRange(expected);
                return test.RunAsync(CancellationToken.None);
            }
        }

        #endregion
    }
}
