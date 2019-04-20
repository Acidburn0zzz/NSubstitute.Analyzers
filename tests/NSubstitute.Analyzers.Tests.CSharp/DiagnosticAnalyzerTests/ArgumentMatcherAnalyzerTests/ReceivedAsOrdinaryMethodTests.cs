using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute.Analyzers.CSharp.DiagnosticAnalyzers;
using Xunit;

namespace NSubstitute.Analyzers.Tests.CSharp.DiagnosticAnalyzerTests.ArgumentMatcherAnalyzerTests
{
    public class ReceivedAsOrdinaryMethodTests : ArgumentMatcherMisuseDiagnosticVerifier
    {
        [Theory]
        [InlineData("Arg.Any<int>()")]
        [InlineData("Arg.Is(1)")]
        public async Task ReportsNoDiagnostics_WhenUsedWithReceivedCallsAssertionMethod(string arg)
        {
            var source = $@"using NSubstitute;

namespace MyNamespace
{{
    public abstract class Foo
    {{
        public abstract int Bar(int x);
    }}

    public class FooTests
    {{
        public void Test()
        {{
            var substitute = NSubstitute.Substitute.For<Foo>();
            SubstituteExtensions.Received(substitute, 1).Bar({arg});
        }}
    }}
}}";

            await VerifyNoDiagnostic(source);
        }

        [Theory]
        [InlineData("Arg.Any<int>()")]
        [InlineData("Arg.Is(1)")]
        public async Task ReportsNoDiagnostics_WhenUsedWithReceivedCallsAssertionMethod_Indexer(string arg)
        {
            var source = $@"using NSubstitute;

namespace MyNamespace
{{
    public abstract class Foo
    {{
        public abstract int this[int x] {{ get; }}
    }}

     public class FooTests
    {{
        public void Test()
        {{
            var substitute = NSubstitute.Substitute.For<Foo>();
            _ = SubstituteExtensions.Received(substitute, 1)[{arg}];
        }}
    }}
}}";

            await VerifyNoDiagnostic(source);
        }
    }
}