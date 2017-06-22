Option Strict On
Option Explicit On

Imports Microsoft.Win32
Imports System.Deployment.Application
Imports System.Drawing
Imports System.Environment
Imports System.IO
Imports System.Reflection
Imports System.Windows.Forms
Imports ShapeExtract.Scripts

Namespace Scripts

    Module Main

#Region "  Error Trapping "

        ''' <summary> 
        ''' Global error message for all procedures
        ''' </summary>
        ''' <param name="ex">the handled exception</param>
        ''' <param name="silent">show or hide the message</param>
        Public Sub DisplayErrorMessage(ByRef ex As Exception, Optional ByVal silent As Boolean = False)
            Dim sf As New System.Diagnostics.StackFrame(1)
            Dim caller As System.Reflection.MethodBase = sf.GetMethod()
            Dim procedure As String = (caller.Name).Trim
            Dim msg As String = "Contact your system administrator."
            msg += NewLine & "Procedure: " & procedure
            msg += NewLine & "Description: " & ex.ToString
            Console.WriteLine(msg)
            If silent = False Then
                MessageBox.Show(msg, "Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

        End Sub

#End Region

#Region "  Form Subroutines "

        ''' <summary> 
        ''' Set form icon
        ''' </summary>
        ''' <param name="currentForm">the current form object</param>
        ''' <param name="bmp">the icon referenced for the form</param>
        Public Sub SetFormIcon(ByRef currentForm As Form, ByRef bmp As Bitmap)
            Try
                currentForm.Icon = Icon.FromHandle(bmp.GetHicon)

            Catch ex As Exception
                Call Main.DisplayErrorMessage(ex)
                Exit Try

            End Try

        End Sub

        ''' <summary>
        ''' set the icon in add/remove programs
        ''' </summary>
        ''' <param name="iconName">The referenced icon name for the application.</param>
        ''' <remarks>
        ''' only run if deployed 
        ''' </remarks>
        Public Sub SetAddRemoveProgramsIcon(iconName As String)
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
                    Call DisplayErrorMessage(ex)
                End Try
            End If
        End Sub

#End Region

    End Module

End Namespace
