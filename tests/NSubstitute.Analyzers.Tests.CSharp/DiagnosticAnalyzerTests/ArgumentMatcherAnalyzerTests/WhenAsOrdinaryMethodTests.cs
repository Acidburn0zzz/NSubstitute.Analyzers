using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute.Analyzers.CSharp.DiagnosticAnalyzers;
using NSubstitute.Analyzers.Shared;
using NSubstitute.Analyzers.Tests.Shared.DiagnosticAnalyzers;
using Xunit;

namespace NSubstitute.Analyzers.Tests.CSharp.DiagnosticAnalyzerTests.ArgumentMatcherAnalyzerTests
{
    public class WhenAsOrdinaryMethodTests : ArgumentMatcherMisuseDiagnosticVerifier
    {
        [Theory]
        [InlineData("Arg.Any<int>()")]
        [InlineData("Arg.Is(1)")]
        public async Task ReportsNoDiagnostics_WhenUsedWithSetupMethod(string arg)
        {
            var source = $@"using System;
using System.Threading.Tasks;
using NSubstitute;

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
            SubstituteExtensions.When(substitute, delegate(Foo x) {{ x.Bar({arg}); }}).Do(x => throw new NullReferenceException());
            SubstituteExtensions.When(substitute, x => x.Bar({arg})).Do(x => throw new NullReferenceException());
            SubstituteExtensions.When(substitute, x => x.Bar({arg})).Do(x => {{ throw new NullReferenceException(); }});
            SubstituteExtensions.When(substitute, x => x.Bar({arg})).Do(x => {{ throw new NullReferenceException(); }});
            SubstituteExtensions.When(substitute, SubstituteCall).Do(x => {{ throw new NullReferenceException(); }});
        }}

        private Task SubstituteCall(Foo obj)
        {{
            obj.Bar(Arg.Any<int>());
            return Task.CompletedTask;
        }}
    }}
}}";

            await VerifyNoDiagnostic(source);
        }

        [Theory]
        [InlineData("[|Arg.Any<int>()|]")]
        [InlineData("[|Arg.Is(1)|]")]
        public async Task ReportsDiagnostics_WhenUsedWithoutSetupMethod(string arg)
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
            substitute.Bar({arg});
        }}
    }}
}}";
            await VerifyDiagnostic(source, ArgumentMatcherUsedOutsideOfCallDescriptor);
        }

        [Theory]
        [InlineData("Arg.Any<int>()")]
        [InlineData("Arg.Is(1)")]
        public async Task ReportsNoDiagnostics_WhenUsedWitSetupMethod_Indexer(string arg)
        {
            var source = $@"using NSubstitute;
using System;

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
            SubstituteExtensions.When(substitute, delegate(Foo x) {{ var y = x[{arg}]; }}).Do(x => throw new NullReferenceException());
            SubstituteExtensions.When(substitute, x => {{ var y = x[{arg}]; }}).Do(x => throw new NullReferenceException());
        }}
    }}
}}";

            await VerifyNoDiagnostic(source);
        }

        [Theory]
        [InlineData("[|Arg.Any<int>()|]")]
        [InlineData("[|Arg.Is(1)|]")]
        public async Task ReportsDiagnostics_WhenUsedWithoutSetupMethod_Indexer(string arg)
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
            var x = substitute[{arg}];
        }}
    }}
}}";

            await VerifyDiagnostic(source, ArgumentMatcherUsedOutsideOfCallDescriptor);
        }
    }
}