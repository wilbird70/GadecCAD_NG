'Gadec Engineerings Software (c) 2022

''' <summary>
''' <para><see cref="FolderWatchEvents"/> adds several events to the folder to track files to be created, changed, deteled or renamed.</para>
''' <para>Note: This class is currently not ready or in use.</para>
''' </summary>
Public Class FolderWatchEvents
    ''' <summary>
    ''' The <see cref="IO.FileSystemWatcher"/>.
    ''' </summary>
    Private ReadOnly _fileSystemWatcher As IO.FileSystemWatcher
    ''' <summary>
    ''' List to contain all fullnames of documents that are currently open.
    ''' </summary>
    Private ReadOnly _documents As String()

    ''' <summary>
    ''' Initializes a new instance of <see cref="FolderWatchEvents"/>.
    ''' <para><see cref="FolderWatchEvents"/> adds several events to the folder to track files to be created, changed, deteled or renamed.</para>
    ''' <para>Note: This class is currently not ready or in use.</para>
    ''' </summary>
    ''' <param name="folder"></param>
    Public Sub New(folder As String)
        _documents = DocumentsHelper.GetDocumentNames()
        If _fileSystemWatcher.Path = folder Then Exit Sub

        If NotNothing(_fileSystemWatcher) Then
            RemoveHandler _fileSystemWatcher.Created, AddressOf FileSystemWatcherChangedEventHandler
            RemoveHandler _fileSystemWatcher.Changed, AddressOf FileSystemWatcherChangedEventHandler
            RemoveHandler _fileSystemWatcher.Deleted, AddressOf FileSystemWatcherChangedEventHandler
            RemoveHandler _fileSystemWatcher.Renamed, AddressOf FileSystemWatcherRenamedEventHandler
            _fileSystemWatcher.Dispose()
        End If

        _fileSystemWatcher = New IO.FileSystemWatcher(folder)
        AddHandler _fileSystemWatcher.Created, AddressOf FileSystemWatcherChangedEventHandler
        AddHandler _fileSystemWatcher.Changed, AddressOf FileSystemWatcherChangedEventHandler
        AddHandler _fileSystemWatcher.Deleted, AddressOf FileSystemWatcherChangedEventHandler
        AddHandler _fileSystemWatcher.Renamed, AddressOf FileSystemWatcherRenamedEventHandler
        _fileSystemWatcher.EnableRaisingEvents = True
        _fileSystemWatcher.IncludeSubdirectories = True
        _fileSystemWatcher.Filter = "*.*"
        _fileSystemWatcher.NotifyFilter = (IO.NotifyFilters.LastAccess Or IO.NotifyFilters.LastWrite Or IO.NotifyFilters.FileName Or IO.NotifyFilters.DirectoryName)
    End Sub

    'private eventhandlers

    ''' <summary>
    ''' EventHandler for an event that occurs when a file is created, changed or deteled.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FileSystemWatcherChangedEventHandler(sender As Object, e As IO.FileSystemEventArgs)
        Select Case True
            Case Not IO.Path.GetExtension(e.Name) = ".dwg"
            Case _documents.Contains(e.Name)
            Case Else
                Select Case e.ChangeType
                    Case IO.WatcherChangeTypes.Deleted : MsgBox("File Deleted:{L}name{T}{0}{L}type{T}{1}".Compose(e.Name, e.ChangeType))
                    Case IO.WatcherChangeTypes.Created : MsgBox("File Created:{L}name{T}{0}{L}type{T}{1}".Compose(e.Name, e.ChangeType))
                    Case IO.WatcherChangeTypes.Changed : MsgBox("File Changed:{L}name{T}{0}{L}type{T}{1}".Compose(e.Name, e.ChangeType))
                End Select
        End Select
    End Sub

    ''' <summary>
    ''' EventHandler for an event that occurs when a file is renamed.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FileSystemWatcherRenamedEventHandler(sender As Object, e As IO.RenamedEventArgs)
        Select Case True
            Case Not IO.Path.GetExtension(e.Name) = ".dwg"
            Case _documents.Contains(e.Name)
            Case Else
                MsgBox("File Renamed:{L}from{T}{0}{L}to{T}{1}".Compose(e.OldName, e.Name))
        End Select
    End Sub

End Class
