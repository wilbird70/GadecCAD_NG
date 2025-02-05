'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides the required events for the application.
''' </summary>
Public Class ApplicationEvents

    'subs

    ''' <summary>
    ''' Initializes the required events for the application.
    ''' </summary>
    Public Shared Sub Initialize()
        AddHandler Application.QuitWillStart, AddressOf QuitWillStartEventHandler
        AddHandler Application.QuitAborted, AddressOf QuitAbortedEventHandler
        AddHandler Application.SystemVariableChanged, AddressOf SystemVariableChangedEventHandler
        With Application.DocumentManager
            AddHandler .DocumentCreated, AddressOf DocumentCreatedEventHandler
            AddHandler .DocumentActivated, AddressOf DocumentActivatedEventHandler
            AddHandler .DocumentDestroyed, AddressOf DocumentDestroyedEventHandler
            AddHandler .DocumentToBeDestroyed, AddressOf DocumentToBeDestroyedEventHandler
        End With
        If NotNothing(ActiveDocument) Then DocumentCreatedEventHandler(Nothing, Nothing)
    End Sub

    'private eventhandlers

    ''' <summary>
    ''' EventHandler for the event that occurs when user wants to quit AutoCAD.
    ''' <para>This handler controls the locking of the documentevents.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Shared Sub QuitWillStartEventHandler(sender As Object, e As EventArgs)
        DocumentEvents.DocumentEventsEnabled = False
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when user wants to quit, but changes his mind in time.
    ''' <para>This handler controls the release of the documentevents.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Shared Sub QuitAbortedEventHandler(sender As Object, e As EventArgs)
        DocumentEvents.DocumentEventsEnabled = True
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when a system variable (SYSVAR) is changed.
    ''' <para>This handler controls:</para>
    ''' <para>- Showing/highlighting the active layer on the GADEC palette.</para>
    ''' <para>- Tranfer the value of DIMSCALE into the LTSCALE and HPSCALE system variables.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Shared Sub SystemVariableChangedEventHandler(sender As Object, e As SystemVariableChangedEventArgs)
        Try
            Select Case e.Name
                Case "CLAYER"
                    PaletteHelper.ShowSelectedLayer()
                Case "DIMSCALE"
                    Dim doc = ActiveDocument()
                    Dim scale = Val(SysVarHandler.GetVar("DIMSCALE"))
                    If scale > 0 Then
                        SysVarHandler.SetVar(doc, "LTSCALE", scale)
                        SysVarHandler.SetVar(doc, "HPSCALE", scale)
                    End If
            End Select
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when a document is opened or created (new document).
    ''' <para>This handler controls:</para>
    ''' <para>- Adding of documentevents to the document.</para>
    ''' <para>- Finds all frames in the drawing to renew the framedata.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Shared Sub DocumentCreatedEventHandler(sender As Object, e As DocumentCollectionEventArgs)
        Try
            Dim doc = If(IsNothing(e), ActiveDocument(), e.Document)
            doc.DocumentEvents
            Dim frameIdCollections = FrameHelper.GetFrameIdCollections(doc.FrameData)
            XRecordObjectIdsHelper.Update(doc, "{Company}".Compose, "FrameWorkIDs", frameIdCollections)
            PrefixLengths.TryAdd(doc.GetPath, -1)

        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when a document gets the focus.
    ''' <para>This handler controls:</para>
    ''' <para>- Reloading the framelist on the GADEC palette.</para>
    ''' <para>- Reloading the filelist on the GADEC palette.</para>
    ''' <para>- Showing/highlighting the active layer on the GADEC palette.</para>
    ''' <para>- Regenerate the drawing in the editor if required.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Shared Sub DocumentActivatedEventHandler(sender As Object, e As DocumentCollectionEventArgs)
        Try
            If Not DocumentEvents.DocumentEventsEnabled Then Exit Sub

            PaletteHelper.ReloadFrameList()
            PaletteHelper.ReloadFileList()
            PaletteHelper.ShowSelectedLayer()
            If e.Document?.EditorNeedsRegen Then
                e.Document.Editor.Regen()
                e.Document.EditorNeedsRegen(False)
            End If
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when a document is closed.
    ''' <para>When other documents are open, one of them will get the focus.</para>
    ''' <para>But when it was the last document, this handler controls:</para>
    ''' <para>- Clearing the frame- and filelists.</para>
    ''' <para>- Hiding/unhighlighting the active layer on the GADEC palette.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Shared Sub DocumentDestroyedEventHandler(sender As Object, e As DocumentDestroyedEventArgs)
        Try
            If Not DocumentEvents.DocumentEventsEnabled Then Exit Sub

            If Application.DocumentManager.Count = 0 Then
                PaletteHelper.ReloadFrameList()
                PaletteHelper.ReloadFileList()
                PaletteHelper.ShowSelectedLayer()
            End If
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when a document is to be closed.
    ''' <para>It disposes/removes the document events and the frame database from the documents userdata property.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Shared Sub DocumentToBeDestroyedEventHandler(sender As Object, e As DocumentCollectionEventArgs)
        Try
            If Not DocumentEvents.DocumentEventsEnabled Then Exit Sub

            e.Document.DocumentEvents.Dispose()
            e.Document.FrameData.Dispose()
        Catch ex As System.Exception
            GadecException(ex)
        End Try
    End Sub

End Class
