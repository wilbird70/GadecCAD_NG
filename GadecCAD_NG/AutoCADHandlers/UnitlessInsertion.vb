'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.DatabaseServices

''' <summary>
''' <para><see cref="UnitlessInsertion"/> sets the insertion units temporary to unitless (undefined).</para>
''' </summary>
Public Class UnitlessInsertion
    Implements System.IDisposable
    Private _disposed As Boolean

    ''' <summary>
    ''' Contains the present drawing.
    ''' </summary>
    Private ReadOnly _database As Database
    ''' <summary>
    ''' Contains the previous value of the insertion units variable.
    ''' </summary>
    Private ReadOnly _previousValue As UnitsValue

    ''' <summary>
    ''' Initializes a new instance of <see cref="UnitlessInsertion"/> with the specified drawing.
    ''' </summary>
    ''' <param name="database"></param>
    Public Sub New(database As Database)
        _database = database
        _previousValue = _database.Insunits
        _database.Insunits = UnitsValue.Millimeters
    End Sub

    ''' <summary>
    ''' Sets the insertion units to the previous value.
    ''' </summary>
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not _disposed Then
            If disposing Then
                _database.Insunits = _previousValue
            End If
            _disposed = True
        End If
    End Sub

    ''' <summary>
    ''' Implements the dispose method.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
End Class
