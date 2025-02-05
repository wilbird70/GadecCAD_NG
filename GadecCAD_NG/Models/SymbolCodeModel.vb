'Gadec Engineerings Software (c) 2022

''' <summary>
''' <para><see cref="SymbolCodeModel"/> contains detailed information about encoding symbols.</para>
''' </summary>
Public Class SymbolCodeModel
    ''' <summary>
    ''' Contains the texts for each atribute.
    ''' </summary>
    ''' <returns>A list of ini-file-like stringexpressions (eg 'ADR=001').</returns>
    Public Property Texts As String()
    ''' <summary>
    ''' Determine which attribute-tag has the location text.
    ''' </summary>
    ''' <returns>The tag.</returns>
    Public Property LocationTag As String
    ''' <summary>
    ''' Determine which attribute-tag has the counter code.
    ''' </summary>
    ''' <returns>The tag.</returns>
    Public Property CounterTag As String
    ''' <summary>
    ''' Contains the step value for the counter code.
    ''' </summary>
    ''' <returns>The stepvalue (1 or -1).</returns>
    Public Property CounterStep As Integer
    ''' <summary>
    ''' The objectid of the layer for the loopline.
    ''' </summary>
    ''' <returns>The objectid.</returns>
    Public Property drawLoopLine As Boolean
    Public Property LoopLineLayerId As Autodesk.AutoCAD.DatabaseServices.ObjectId

    ''' <summary>
    ''' Initializes a new instance of <see cref="SymbolCodeModel"/>.
    ''' <para><see cref="SymbolCodeModel"/> contains detailed information about encoding symbols.</para>
    ''' </summary>
    Public Sub New()
    End Sub

End Class
