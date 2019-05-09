using System.Threading.Tasks;
using NSubstitute.Analyzers.Tests.Shared.Extensibility;

namespace NSubstitute.Analyzers.Tests.CSharp.DiagnosticAnalyzerTests.AwaitedWhenCallAnalyzerTests
{
    [CombinatoryData("When", "When<Foo>", "WhenForAnyArgs", "WhenForAnyArgs<Foo>")]
    public class WhenAsExtensionMethodTests : AwaitedWhenCallAnalyzerVerifier
    {
        public override async Task ReportsDiagnostics_WhenAsyncMethod_IsAwaited(string method)
        {
            var source = $@"using System.Threading.Tasks;
using NSubstitute;

namespace MyNamespace
{{
    public class Foo
    {{
        public virtual async Task Bar()
        {{
            await Task.CompletedTask;
        }}
    }}

    public class FooTests
    {{
        public void Test()
        {{
            var substitute = NSubstitute.Substitute.For<Foo>();
            substitute.{method}([|async callInfo => await callInfo.Bar()|]);
        }}
    }}
}};";
            await VerifyDiagnostic(source, AwaitedWhenCallDescriptor);
        }

        public override async Task ReportsNoDiagnostics_WhenAsyncMethod_IsNotAwaited(string method)
        {
            var source = $@"using System.Threading.Tasks;
using NSubstitute;

namespace MyNamespace
{{
    public class Foo
    {{
        public virtual async Task Bar()
        {{
            await Task.CompletedTask;
        }}
    }}

    public class FooTests
    {{
        public void Test()
        {{
            var substitute = NSubstitute.Substitute.For<Foo>();
            substitute.{method}(callInfo => callInfo.Bar());
        }}
    }}
}};";
            await VerifyNoDiagnostic(source);
        }

        public override async Task ReportsNoDiagnostics_WhenUnfortunatelyNamedAsyncMethod_IsAwaited(string method)
        {
            var source = $@"using System.Threading.Tasks;
using NSubstitute;

namespace MyNamespace
{{
    public class Foo
    {{
        public virtual async Task Bar()
        {{
            await Task.CompletedTask;
        }}
    }}

    public class FooTests
    {{
        public void Test()
        {{
            var substitute = NSubstitute.Substitute.For<Foo>();
            substitute.{method}(async callInfo => await callInfo.Bar(), 1);
        }}
    }}

    public static class SubstituteExtensions
    {{
        public static T When<T>(this T substitute, System.Action<T> substituteCall, int x)
        {{
            return default(T);
        }}

        public static T WhenForAnyArgs<T>(this T substitute, System.Action<T> substituteCall, int x)
        {{
            return default(T);
        }}
    }}
}};";
            await VerifyNoDiagnostic(source);
        }
    }
}