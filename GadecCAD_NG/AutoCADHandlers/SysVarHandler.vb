'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices

''' <summary>
''' <para><see cref="SysVarHandler"/> can modify variables and reset them to their original values when disposed.</para>
''' <para>Provides also static methods to get and set AutoCAD system variables.</para>
''' </summary>
Public Class SysVarHandler
    Implements System.IDisposable
    Private _disposed As Boolean

    ''' <summary>
    ''' The present document.
    ''' </summary>
    Private ReadOnly _document As Document
    ''' <summary>
    ''' List of previous values of system variables.
    ''' </summary>
    Private _variables As Dictionary(Of String, Object)

    ''' <summary>
    ''' Initializes a new instance of <see cref="SysVarHandler"/>.
    ''' <para><see cref="SysVarHandler"/> can modify variables and reset them to their original values when disposed.</para>
    ''' <para>Provides also static methods to get and set AutoCAD system variables.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Sub New(document As Document)
        _document = document
        _variables = New Dictionary(Of String, Object)
    End Sub

    ''' <summary>
    ''' Reset modified variables to their original values when disposed.
    ''' </summary>
    ''' <param name="disposing">True for resetting.</param>
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not _disposed Then
            If disposing Then Reset()
            _variables = Nothing
            _disposed = True
        End If
    End Sub

    ''' <summary>
    ''' Implements <see cref="IDisposable.Dispose"/>.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

    'functions

    ''' <summary>
    ''' Gets an AutoCAD system variable.
    ''' </summary>
    ''' <param name="varName">The name of the system variable.</param>
    ''' <returns></returns>
    Public Function [Get](varName As String) As Object
        Return GetVar(varName)
    End Function

    ''' <summary>
    ''' Sets an AutoCAD system variable.
    ''' </summary>
    ''' <param name="varName">The name of the system variable.</param>
    ''' <param name="value">The value to set.</param>
    Public Sub [Set](varName As String, value As Object)
        Dim previousValue = GetVar(varName)
        If previousValue = value Then Exit Sub

        Dim setResult = SetVar(_document, varName, value)
        If setResult And Not _variables.ContainsKey(varName) Then _variables.Add(varName, previousValue)
    End Sub

    ''' <summary>
    ''' Reset modified variables to their original values.
    ''' </summary>
    Public Sub Reset()
        Using _document.LockDocument
            _variables.ToList.ForEach(Sub(v) SetVar(v.Key, v.Value))
            _variables = New Dictionary(Of String, Object)
        End Using
    End Sub

    '///shared part of class\\\

    ''' <summary>
    ''' Gets an AutoCAD system variable.
    ''' </summary>
    ''' <param name="varName">The name of the system variable.</param>
    ''' <returns></returns>
    Public Shared Function GetVar(varName As String) As Object
        Try
            Return Application.GetSystemVariable(varName)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Sets an AutoCAD system variable.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="varName">The name of the system variable.</param>
    ''' <param name="value">The value to set.</param>
    ''' <returns></returns>
    Public Shared Function SetVar(document As Document, varName As String, value As Object) As Boolean
        Using document.LockDocument()
            Return SetVar(varName, value)
        End Using
    End Function

    'private shared functions

    ''' <summary>
    ''' Sets an AutoCAD system variable.
    ''' </summary>
    ''' <param name="varName">The name of the system variable.</param>
    ''' <param name="value">The value to set.</param>
    ''' <returns></returns>
    Private Shared Function SetVar(varName As String, value As Object) As Boolean
        Try
            Application.SetSystemVariable(varName, value)
            Return True
        Catch ex As Exception
            ex.AddData($"Var: {varName}")
            ex.AddData($"Value: {value.ToString}")
            ex.ReThrow
            Return False
        End Try
    End Function

End Class
