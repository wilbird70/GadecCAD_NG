'Gadec Engineerings Software (c) 2022

Imports Autodesk.AutoCAD.DatabaseServices

''' <summary>
''' <para><see cref="ModifiedObjects"/> contains collections of objectids of added, deleted or changed objects.</para>
''' </summary>
Public Class ModifiedObjects
    ''' <summary>
    ''' Collection of <see cref="ObjectId"/> of recently added frames.
    ''' </summary>
    Public ReadOnly Property AddedFrameIds As New ObjectIdCollection
    ''' <summary>
    ''' Collection of <see cref="ObjectId"/> of recently deleted frames.
    ''' </summary>
    Public ReadOnly Property DeletedFrameIds As New ObjectIdCollection
    ''' <summary>
    ''' Collection of <see cref="ObjectId"/> of recently changed frames (position, scale or attribute changes).
    ''' </summary>
    Public ReadOnly Property ChangedFrameIds As New ObjectIdCollection
    ''' <summary>
    ''' Collection of <see cref="ObjectId"/> of recently changed symbols (position, scale or attribute changes).
    ''' </summary>
    Public ReadOnly Property ChangedSymbolIds As New ObjectIdCollection
    ''' <summary>
    ''' Collection of <see cref="ObjectId"/> of recently changed blockreferences.
    ''' <para>Note: This one is to prevent to check and process blockreferences multiple times.</para>
    ''' </summary>
    Public ReadOnly Property ChangedBlockReferenceIds As New ObjectIdCollection
    ''' <summary>
    ''' Collection of <see cref="ObjectId"/> of recently changed blockreferences.
    ''' <para>Note: This one is to prevent to check and process blockreferences multiple times.</para>
    ''' </summary>
    Public ReadOnly Property ChangedAttributeOwnerIds As New ObjectIdCollection

    ''' <summary>
    ''' Initializes a new instance of <see cref="ModifiedObjects"/>.
    ''' <para><see cref="ModifiedObjects"/> contains collections of objectids of added, deleted or changed objects.</para>
    ''' </summary>
    Public Sub New()
    End Sub

End Class
