﻿using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NSubstitute.Analyzers.DiagnosticAnalyzers;
using Xunit;

namespace NSubstitute.Analyzers.Test.VisualBasic.DiagnosticAnalyzerTests.UnusedReceivedAnalyzerTests
{
    public class DidNotReceiveWithAnyArgsAsExtensionMethodTests : DiagnosticAnalyzerTests.UnusedReceivedAnalyzerTests.UnusedReceivedAnalyzerTests
    {
         [Fact]
        public override async Task ReportDiagnostics_WhenUsedWithoutMemberCall()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace
    Interface IFoo
    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of IFoo)()
            substitute.DidNotReceiveWithAnyArgs()
        End Sub
    End Class
End Namespace
";
            var expectedDiagnostic = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.UnusedReceived,
                Severity = DiagnosticSeverity.Warning,
                Message = "Unused received check.",
                Locations = new[]
                {
                    new DiagnosticResultLocation(10, 13)
                }
            };

            await VerifyVisualBasicDiagnostic(source, expectedDiagnostic);
        }

        [Fact]
        public override async Task ReportNoDiagnostics_WhenUsedWithMethodMemberAccess()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace
    Public Class FooBar
    End Class

    Interface IFoo
        Function Bar() As FooBar
    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of IFoo)()
            substitute.DidNotReceiveWithAnyArgs().Bar()
        End Sub
    End Class
End Namespace
";

            await VerifyVisualBasicDiagnostic(source);
        }

        [Fact]
        public override async Task ReportNoDiagnostics_WhenUsedWithPropertyMemberAccess()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace
    Interface IFoo
        Property Bar As Integer
    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of IFoo)()
            Dim bar = substitute.DidNotReceiveWithAnyArgs().Bar
        End Sub
    End Class
End Namespace
";

            await VerifyVisualBasicDiagnostic(source);
        }

        [Fact]
        public override async Task ReportNoDiagnostics_WhenUsedWithIndexerMemberAccess()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace
    Interface IFoo
        Default ReadOnly Property Item(ByVal x As Integer) As Integer
    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of IFoo)()
            Dim bar = substitute.DidNotReceiveWithAnyArgs()(0)
        End Sub
    End Class
End Namespace
";

            await VerifyVisualBasicDiagnostic(source);
        }

        public override async Task ReportNoDiagnostics_WhenUsedWithInvokingDelegate()
        {
            var source = @"Imports NSubstitute
Imports System

Namespace MyNamespace

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of Func(Of Integer))()
            substitute.DidNotReceiveWithAnyArgs()()
        End Sub
    End Class
End Namespace
";
            await VerifyVisualBasicDiagnostic(source);
        }

        public override async Task ReportsNoDiagnostics_WhenUsedWithUnfortunatelyNamedMethod()
        {
            var source = @"Imports System
Imports System.Runtime.CompilerServices

Namespace NSubstitute
    Module SubstituteExtensions
        <Extension()>
        Function DidNotReceiveWithAnyArgs(Of T As Class)(ByVal substitute As T, ByVal params As Integer) As T
            Return Nothing
        End Function
    End Module

    Public Class FooTests
        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of Func(Of Integer))()
            substitute.DidNotReceiveWithAnyArgs(1)
        End Sub
    End Class
End Namespace
";
            await VerifyVisualBasicDiagnostic(source);
        }
    }
}