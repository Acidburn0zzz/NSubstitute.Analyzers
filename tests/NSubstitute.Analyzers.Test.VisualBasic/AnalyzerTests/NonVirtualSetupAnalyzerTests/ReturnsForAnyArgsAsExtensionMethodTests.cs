﻿using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace NSubstitute.Analyzers.Test.VisualBasic.AnalyzerTests.NonVirtualSetupAnalyzerTests
{
    public class ReturnsForAnyArgsAsExtensionMethodTests : NonVirtualSetupAnalyzerTest
    {
        public override async Task ReportsDiagnostics_WhenSettingValueForNonVirtualMethod()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace

    Public Class Foo

        Public Function Bar() As Integer
            Return 2
        End Function
    End Class

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of Foo)()
            substitute.Bar().ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace
";
            var expectedDiagnostic = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.NonVirtualSetupSpecification,
                Severity = DiagnosticSeverity.Warning,
                Message = "Member Bar can not be intercepted. Only interface members and overrideable, overriding, and must override members can be intercepted.",
                Locations = new[]
                {
                    new DiagnosticResultLocation(16, 13)
                }
            };

            await VerifyVisualBasicDiagnostic(source, expectedDiagnostic);
        }

        public override async Task ReportsDiagnostics_WhenSettingValueForLiteral(string literal, string type)
        {
            var source = $@"Imports NSubstitute

Namespace MyNamespace

    Public Class FooTests

        Public Sub Test()
            Call {literal}.ReturnsForAnyArgs({literal})
        End Sub
    End Class
End Namespace
";
            var expectedDiagnostic = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.NonVirtualSetupSpecification,
                Severity = DiagnosticSeverity.Warning,
                Message = $"Member {literal} can not be intercepted. Only interface members and overrideable, overriding, and must override members can be intercepted.",
                Locations = new[]
                {
                    new DiagnosticResultLocation(8, 18)
                }
            };

            await VerifyVisualBasicDiagnostic(source, expectedDiagnostic);
        }

        public override async Task ReportsDiagnostics_WhenSettingValueForStaticMethod()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace

    Public Class Foo

        Public Shared Function Bar() As Integer
            Return 2
        End Function
    End Class

    Public Class FooTests

        Public Sub Test()
            Foo.Bar().ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace
";
            var expectedDiagnostic = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.NonVirtualSetupSpecification,
                Severity = DiagnosticSeverity.Warning,
                Message = "Member Bar can not be intercepted. Only interface members and overrideable, overriding, and must override members can be intercepted.",
                Locations = new[]
                {
                    new DiagnosticResultLocation(15, 13)
                }
            };

            await VerifyVisualBasicDiagnostic(source, expectedDiagnostic);
        }


        public override async Task ReportsNoDiagnostics_WhenSettingValueForVirtualMethod()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace

    Public Class Foo

        Public Overridable Function Bar() As Integer
            Return 2
        End Function
    End Class

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of Foo)()
            substitute.Bar().ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace
";
            await VerifyVisualBasicDiagnostic(source);
        }

        public override async Task ReportsNoDiagnostics_WhenSettingValueForNonSealedOverrideMethod()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace

    Public Class Foo

        Public Overridable Function Bar() As Integer
            Return 2
        End Function
    End Class

    Public Class Foo2
        Inherits Foo

        Public Overrides Function Bar() As Integer
            Return 1
        End Function
    End Class

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of Foo2)()
            substitute.Bar().ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace
";
            await VerifyVisualBasicDiagnostic(source);
        }

        public override async Task ReportsNoDiagnostics_WhenDataFlowAnalysisIsRequired()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace

    Public Class Foo

        Public Overridable Function Bar() As Integer
            Return 2
        End Function
    End Class

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of Foo)()
            Dim returnValue = substitute.Bar()
            returnValue.ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace
";
            await VerifyVisualBasicDiagnostic(source);
        }

        public override async Task ReportsNoDiagnostics_WhenSettingValueForDelegate()
        {
            var source = @"Imports NSubstitute
Imports System

Namespace MyNamespace

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of Func(Of Integer))()
            substitute().ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace
";
            await VerifyVisualBasicDiagnostic(source);
        }

        public override async Task AnalyzerReturnsDiagnostics_WhenSettingValueForSealedOverrideMethod()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace

    Public Class Foo

        Public Overridable Function Bar() As Integer
            Return 2
        End Function
    End Class

    Public Class Foo2
        Inherits Foo

        Public NotOverridable Overrides Function Bar() As Integer
            Return 1
        End Function
    End Class

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of Foo2)()
            substitute.Bar().ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace
";
            var expectedDiagnostic = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.NonVirtualSetupSpecification,
                Severity = DiagnosticSeverity.Warning,
                Message = "Member Bar can not be intercepted. Only interface members and overrideable, overriding, and must override members can be intercepted.",
                Locations = new[]
                {
                    new DiagnosticResultLocation(24, 13)
                }
            };

            await VerifyVisualBasicDiagnostic(source, expectedDiagnostic);
        }

        public override async Task ReportsNoDiagnostics_WhenSettingValueForAbstractMethod()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace

    Public MustInherit Class Foo

        Public MustOverride Function Bar() As Integer
    End Class

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of Foo)()
            substitute.Bar().ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace
";

            await VerifyVisualBasicDiagnostic(source);
        }


        public override async Task ReportsNoDiagnostics_WhenSettingValueForInterfaceMethod()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace

    Interface IFoo

        Function Bar() As Integer

    End Interface

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of IFoo)()
            substitute.Bar().ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace
";
            await VerifyVisualBasicDiagnostic(source);
        }


        public override async Task ReportsNoDiagnostics_WhenSettingValueForInterfaceProperty()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace

    Interface IFoo

       Property Bar As Integer

    End Interface

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of IFoo)()
            substitute.Bar.ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace
";
            await VerifyVisualBasicDiagnostic(source);
        }


        public override async Task ReportsNoDiagnostics_WhenSettingValueForGenericInterfaceMethod()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace

    Public Interface IFoo(Of T)

        Function Bar(Of T)() As Integer
    End Interface

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of IFoo(Of Integer))()
            substitute.Bar(Of Integer).ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace";

            await VerifyVisualBasicDiagnostic(source);
        }

        public override async Task AnalyzerReturnsNoDiagnostic_WhenSettingValueForAbstractProperty()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace

    Public MustInherit Class Foo

        Public MustOverride ReadOnly Property Bar As Integer
    End Class

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.For(Of Foo)
            substitute.Bar.ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace";

            await VerifyVisualBasicDiagnostic(source);
        }

        public override async Task ReportsNoDiagnostics_WhenSettingValueForInterfaceIndexer()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace

    Public Interface IFoo

        Default Property Item(ByVal i As Integer) As Integer
    End Interface

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.For(Of IFoo)
            substitute(1).ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace";
            await VerifyVisualBasicDiagnostic(source);

        }



        public override async Task AnalyzerReturnsNoDiagnostic_WhenSettingValueForVirtualProperty()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace

    Public Class Foo

        Public Overridable ReadOnly Property Bar As Integer
            Get
            End Get
        End Property
    End Class

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.For(Of Foo)
            substitute.Bar.ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace";

            await VerifyVisualBasicDiagnostic(source);
        }


        public override async Task ReportsDiagnostics_WhenSettingValueForNonVirtualProperty()
        {
            var source = @"Imports NSubstitute

Namespace MyNamespace

    Public Class Foo

        Public ReadOnly Property Bar As Integer
            Get
            End Get
        End Property
    End Class

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.For(Of Foo)
            substitute.Bar.ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace";

            var expectedDiagnostic = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.NonVirtualSetupSpecification,
                Severity = DiagnosticSeverity.Warning,
                Message = "Member Bar can not be intercepted. Only interface members and overrideable, overriding, and must override members can be intercepted.",
                Locations = new[]
                {
                    new DiagnosticResultLocation(17, 13)
                }
            };

            await VerifyVisualBasicDiagnostic(source, expectedDiagnostic);
        }


        public override async Task ReportsNoDiagnostics_WhenSettingValueForVirtualIndexer()
        {
            var source = @"Imports System
Imports NSubstitute

Namespace MyNamespace

    Public Class Foo

        Public Overridable Default Property Item(ByVal x As Integer) As Integer
            Set
                Throw New NotImplementedException
            End Set
            Get
                Throw New NotImplementedException
            End Get

        End Property
    End Class

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.For(Of Foo)
            substitute(1).ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace";

            await VerifyVisualBasicDiagnostic(source);
        }


        public override async Task AnalyzerReturnsDiagnostics_WhenSettingValueForNonVirtualIndexer()
        {
            var source = @"Imports System
Imports NSubstitute

Namespace MyNamespace

    Public Class Foo

        Public Default ReadOnly Property Item(ByVal x As Integer) As Integer
            Get
                Throw New NotImplementedException
            End Get
        End Property
    End Class

    Public Class FooTests

        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.For(Of Foo)
            substitute(1).ReturnsForAnyArgs(1)
        End Sub
    End Class
End Namespace";

            var expectedDiagnostic = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.NonVirtualSetupSpecification,
                Severity = DiagnosticSeverity.Warning,
                Message = "Member Item can not be intercepted. Only interface members and overrideable, overriding, and must override members can be intercepted.",
                Locations = new[]
                {
                    new DiagnosticResultLocation(19, 13)
                }
            };

            await VerifyVisualBasicDiagnostic(source, expectedDiagnostic);
        }
    }
}