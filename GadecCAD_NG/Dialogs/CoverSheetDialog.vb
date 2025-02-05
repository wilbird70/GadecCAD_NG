'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Windows.Forms

''' <summary>
''' <para><see cref="CoverSheetDialog"/> allows the user for setting the parameters for the coversheet of a drawinglist.</para>
''' </summary>
Public Class CoverSheetDialog
    ''' <summary>
    ''' Contains the fullnames of pdf-files to add as attachments to a designpackage.
    ''' </summary>
    ''' <returns>A list of filenames.</returns>
    Public Property Attachments As String() = {}
    ''' <summary>
    ''' Determines if the user wants no frames, but only pdf-attachments in the designpackage.
    ''' </summary>
    ''' <returns>True if user wants only attachments in the designpackage.</returns>
    Public Property OnlyAttachments As Boolean = False

    ''' <summary>
    ''' Contains the folder to initially search for attachments.
    ''' </summary>
    Private ReadOnly _initialFolder As String
    ''' <summary>
    ''' Contains the frame record for the coversheet- and drawinglist frames.
    ''' </summary>
    Private ReadOnly _frameListRow As DataRow

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="CoverSheetDialog"/>.
    ''' <para><see cref="CoverSheetDialog"/> allows the user for setting the parameters for the coversheet of a drawinglist.</para>
    ''' </summary>
    ''' <param name="frameListRow">The frame record for the coversheet- and drawinglist frames.</param>
    ''' <param name="addDesignStatus">If true, adds a designstatus stamp to the coversheet.</param>
    ''' <param name="initialFolder">The folder to initially search for attachments.</param>
    Sub New(frameListRow As DataRow, Optional addDesignStatus As Boolean = False, Optional initialFolder As String = "")
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        Me.Text = Registerizer.GetApplicationVersion()
        Translator.TranslateControls(Me)

        _initialFolder = initialFolder
        _frameListRow = frameListRow
        Dim revisionTexts = "REVTEXTS".Translate.Cut
        SystemTextBox.Text = frameListRow.GetString("System")
        Description1TextBox.Text = frameListRow.GetString("Description1")
        Description2TextBox.Text = frameListRow.GetString("Description2")
        ltOK.Select()

        StatusComboBox.Items.AddRange(revisionTexts)
        Select Case addDesignStatus
            Case True
                Dim menuData = DataSetHelper.LoadFromXml("{Support}\SetContextMenus.xml".Compose)
                Dim contextMenuStrip = MenuStripHelper.Create(menuData.GetTable("SplitButton"), AddressOf ContextMenuStripClickEventHandler, My.Resources.ResourceManager)
                ltOK.ContextMenuStrip = contextMenuStrip
                StatusComboBox.Select()
            Case Else
                StatusComboBox.Visible = False
                ltStatus.Visible = False
                ltAttachments.Visible = False
                InformationButton.Visible = False
                ltOK.Image = Nothing
                ltOK.TextAlign = Drawing.ContentAlignment.MiddleCenter
        End Select

        Me.ShowDialog()
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when user clicks on an empty part of this dialogbox.
    ''' <para>If screenshots are enabled (by doubleclicking on the languageflag, see <see cref="SettingsPalette"/>) a screenshot of this dialogbox will be taken.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_Click(sender As Object, e As EventArgs) Handles Me.Click
        Try
            If ScreenShot Then ImageHelper.GetScreenShot(Me.Location, Me.Size)
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'buttons

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the OK button.
    ''' <para>When the user clicks on the arrow image, a context menu is displayed.</para>
    ''' <para>Otherwise it calls a method that sets the buttonvalue, saves the settings in the frame record and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AcceptButton_Click(sender As Object, e As EventArgs) Handles ltOK.Click
        Try
            Dim clickPosition = ltOK.PointToClient(New System.Drawing.Point(MousePosition.X, MousePosition.Y))
            If clickPosition.X >= (ltOK.Size.Width - ltOK.Image?.Width) Then
                Dim stripLocation = ltOK.PointToScreen(New System.Drawing.Point(1, ltOK.Height))
                ltOK.ContextMenuStrip.Show(stripLocation)
            Else
                Accept()
            End If
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the Cancel button.
    ''' <para>It sets the buttonvalue and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub CancelButton_Click(sender As Object, e As EventArgs) Handles ltCancel.Click
        Try
            Me.Hide()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the Attachments button.
    ''' <para>It allows the user to select pdf-files to add to the designpackage.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ltAttachments_Click(sender As Object, e As EventArgs) Handles ltAttachments.Click
        Try
            Dim selectedFiles = FileSystemHelper.SelectFiles(_initialFolder, "pdf")
            Dim attachments = _Attachments.ToList
            attachments.AddRange(selectedFiles)
            Select Case attachments.Count
                Case 0 : Exit Sub
                Case 1 : ltAttachments.Text = "1 pdf"
                Case Else : ltAttachments.Text = "{0} pdf's".Compose(attachments.Count)
            End Select
            _Attachments = attachments.ToArray
            ltAttachments.Font = FontHelper.ArialBold
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'eventHandlers

    ''' <summary>
    ''' EventHandler for the event that occurs when the user selects an option in the contextmenu.
    ''' <para>It selects the OnlyAttachments option and calls a method that sets the buttonvalue, saves the settings in the frame record and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ContextMenuStripClickEventHandler(sender As Object, e As EventArgs)
        Try
            If _Attachments.Count = 0 OrElse Not DirectCast(sender, ToolStripItem).Tag.ToString = "OnlyPdf" Then Exit Sub

            _OnlyAttachments = True
            Accept()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'private subs

    ''' <summary>
    ''' Executes when the user clicks the acceptbutton or choose the option for OnlyAttachments.
    ''' <para>It sets the buttonvalue, saves the settings in the frame record and closes the dialogbox.</para>
    ''' </summary>
    Private Sub Accept()
        Me.DialogResult = DialogResult.OK
        Dim description = If(Description2TextBox.Text = "", "{0}", "{0}, {1}").Compose(SystemTextBox.Text, Description2TextBox.Text)
        Dim projectName = StrConv("{0} ({1}, {2})".Compose(_frameListRow.GetString("Client1"), StatusComboBox.Text, description), VbStrConv.ProperCase)
        _frameListRow.SetString("ProjectName", projectName)
        _frameListRow.SetString("System", SystemTextBox.Text)
        _frameListRow.SetString("Description1", Description1TextBox.Text)
        _frameListRow.SetString("Description2", Description2TextBox.Text)
        _frameListRow.SetString("Status", StatusComboBox.Text)
        Me.Hide()
    End Sub

End Class