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
using VerifyCS = FAToShouldlyAnalyzer.Test.CSharpCodeFixVerifier<
    FAToShouldlyAnalyzer.FAToShouldlyAnalyzer,
    FAToShouldlyAnalyzer.FAToShouldlyCodeFixProvider>;

namespace FAToShouldlyAnalyzer.Test
{
    [TestClass]
    public class FAToShouldlyAnalyzerUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestNoIssue()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestShouldBe()
        {
            // Arrange
            var shouldBeCode = await File.ReadAllTextAsync(Path.Combine(GetImportPath(), "ShouldBe.txt"));
            var shouldBeCodeFix = await File.ReadAllTextAsync(Path.Combine(GetImportPath(), "ShouldBeResult.txt"));

            // Act
            var expected = VerifyCS.Diagnostic(nameof(FAToShouldlyAnalyzer)).WithLocation(0);

            // Assert
            var codeFixTester = new CodeFixTester(shouldBeCode, shouldBeCodeFix, expected);
            await codeFixTester.RunAsync(CancellationToken.None);
            var compilerDiagnostics = codeFixTester.CompilerDiagnostics;
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
                throw new ArgumentNullException("Invalid import path");
            }
            return importPath;
        }

        private class
            CodeFixTester : CSharpCodeFixTest<FAToShouldlyAnalyzer, FAToShouldlyCodeFixProvider, XUnitVerifier>
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
                            new PackageIdentity("xunit.runner.visualstudio", "2.5.3")
                        )
                    );
            }
        }

    }
}
