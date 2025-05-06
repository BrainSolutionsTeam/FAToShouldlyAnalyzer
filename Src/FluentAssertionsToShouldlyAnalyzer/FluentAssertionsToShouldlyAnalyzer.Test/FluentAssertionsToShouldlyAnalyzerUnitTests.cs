using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Xunit.Assert;
using VerifyCS = FluentAssertionsToShouldlyAnalyzer.Test.Verifiers.CSharpCodeFixVerifier<
    FluentAssertionsToShouldlyAnalyzer.FluentAssertionsToShouldlyAnalyzer,
    FluentAssertionsToShouldlyAnalyzer.FluentAssertionsToShouldlyCodeFixProvider>;

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

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        [DataRow(001, "ShouldBe.txt", "ShouldBeResult.txt", 0)]
        [DataRow(002, "ShouldNotBe.txt", "ShouldNotBeResult.txt", 0)]
        [DataRow(003, "ShouldBeNull.txt", "ShouldBeNullResult.txt", 0)]
        [DataRow(004, "ShouldNotBeNull.txt", "ShouldNotBeNullResult.txt", 0)]
        public async Task TestShouldCodeFix(int index, string sourceCodePath, string resultCodePath, int diagnosticLocation)
        {
            // Arrange
            var sourceCode = await File.ReadAllTextAsync(Path.Combine(GetImportPath(), sourceCodePath));
            var sourceCodeFix = await File.ReadAllTextAsync(Path.Combine(GetImportPath(), resultCodePath));

            // Act
            var expected = VerifyCS
                .Diagnostic(FluentAssertionsToShouldlyAnalyzer.Rule)
                .WithLocation(diagnosticLocation);

            // Assert
            var codeFixTester = new CodeFixTester(sourceCode, sourceCodeFix, expected);
            Assert.True(codeFixTester.ExpectedDiagnostics.Any());
            await codeFixTester.RunAsync(CancellationToken.None);
        }

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

        private class
            CodeFixTester : CSharpCodeFixTest<FluentAssertionsToShouldlyAnalyzer, FluentAssertionsToShouldlyCodeFixProvider, XUnitVerifier>
        {
            public CodeFixTester(
                string source,
                string fixedSource,
                params DiagnosticResult[] expected)
            {
                TestCode = source;
                FixedCode = fixedSource;
                ExpectedDiagnostics.AddRange(expected);

                ReferenceAssemblies = ReferenceAssemblies.Default
                    .AddPackages(ImmutableArray.Create(
                            new PackageIdentity("FluentAssertions", "8.2.0"),
                            new PackageIdentity("xunit", "2.5.3"),
                            new PackageIdentity("xunit.runner.visualstudio", "2.5.3"),
                            new PackageIdentity("Shouldly", "4.3.0")
                        )
                    );
            }
        }

    }
}
