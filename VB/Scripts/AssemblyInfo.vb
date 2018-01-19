Option Strict On
Option Explicit On

Imports Microsoft.Win32
Imports System.Deployment.Application
Imports System.Drawing
Imports System.IO
Imports System.Reflection
Imports System.Windows.Forms

Namespace Scripts

    Public Class AssemblyInfo

        Public Shared Sub SetFormIcon(ByRef currentForm As Form, ByRef bmp As Bitmap)
            Try
                currentForm.Icon = Icon.FromHandle(bmp.GetHicon)

            Catch ex As Exception
                ErrorHandler.DisplayMessage(ex)
                Exit Try

            End Try

        End Sub

        Public Shared Sub SetAddRemoveProgramsIcon(iconName As String)
            If System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed AndAlso ApplicationDeployment.CurrentDeployment.IsFirstRun Then
                Try
                    Dim code As Assembly = Assembly.GetExecutingAssembly()
                    Dim asdescription As AssemblyDescriptionAttribute = DirectCast(Attribute.GetCustomAttribute(code, GetType(AssemblyDescriptionAttribute)), AssemblyDescriptionAttribute)
                    Dim assemblyDescription As String = asdescription.Description

                    'Get the assembly information
                    Dim assemblyInfo As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly()

                    'CodeBase is the location of the ClickOnce deployment files
                    Dim uriCodeBase As New Uri(assemblyInfo.CodeBase)
                    Dim clickOnceLocation As String = Path.GetDirectoryName(uriCodeBase.LocalPath.ToString())

                    'the icon is included in this program
                    Dim iconSourcePath As String = Path.Combine(clickOnceLocation, Convert.ToString("Resources\") & iconName)
                    If Not File.Exists(iconSourcePath) Then
                        Return
                    End If

                    Dim myUninstallKey As RegistryKey = Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Uninstall")
                    Dim mySubKeyNames As String() = myUninstallKey.GetSubKeyNames()
                    For i As Integer = 0 To mySubKeyNames.Length - 1
                        Dim myKey As RegistryKey = myUninstallKey.OpenSubKey(mySubKeyNames(i), True)
                        Dim myValue As Object = myKey.GetValue("DisplayName")
                        If myValue IsNot Nothing AndAlso myValue.ToString() = assemblyDescription Then
                            myKey.SetValue("DisplayIcon", iconSourcePath)
                            Exit For
                        End If
                    Next
                Catch ex As Exception
                    ErrorHandler.DisplayMessage(ex)
                End Try
            End If
        End Sub

        Public Shared Function GetCurrentLocation(locationType As String) As String
            Try
                Dim assemblyInfo As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly()
                Dim uriCodeBase As New Uri(assemblyInfo.CodeBase)

                Select Case locationType
                    Case "AssemblyLocation"
                        Return assemblyInfo.Location
                    Case "ClickOnceLocation"
                        Return Path.GetDirectoryName(uriCodeBase.LocalPath.ToString())
                    Case Else
                        Return String.Empty

                End Select
            Catch generatedExceptionName As Exception

                Return String.Empty
            End Try

        End Function

    End Class

End Namespace
