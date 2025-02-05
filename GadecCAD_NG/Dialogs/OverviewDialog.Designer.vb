Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OverviewDialog
    Inherits Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim DataGridViewCellStyle3 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Me.ltCancel = New Button()
        Me.GroupsListBox = New ListBox()
        Me.ltOK = New Button()
        Me.GroupingLabel = New Label()
        Me.FramesDataGridView = New DataGridView()
        Me.ltClose = New Button()
        Me.AssociateFoldersListBox = New ListBox()
        Me.ViewDesignButton = New Button()
        Me.ViewProjectButton = New Button()
        Me.ViewDescriptionButton = New Button()
        Me.ltSelectAll = New Button()
        Me.ViewClientButton = New Button()
        Me.ViewRevisionButton = New Button()
        Me.DownButton = New Button()
        Me.UpButton = New Button()
        CType(Me.FramesDataGridView, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ltCancel
        '
        Me.ltCancel.DialogResult = DialogResult.Cancel
        Me.ltCancel.Location = New System.Drawing.Point(557, 439)
        Me.ltCancel.Name = "ltCancel"
        Me.ltCancel.Size = New System.Drawing.Size(85, 23)
        Me.ltCancel.TabIndex = 43
        Me.ltCancel.Text = "XXX"
        Me.ltCancel.UseVisualStyleBackColor = True
        '
        'GroupsListBox
        '
        Me.GroupsListBox.FormattingEnabled = True
        Me.GroupsListBox.Location = New System.Drawing.Point(239, 381)
        Me.GroupsListBox.Name = "GroupsListBox"
        Me.GroupsListBox.Size = New System.Drawing.Size(148, 82)
        Me.GroupsListBox.TabIndex = 46
        '
        'ltOK
        '
        Me.ltOK.DialogResult = DialogResult.OK
        Me.ltOK.Location = New System.Drawing.Point(466, 439)
        Me.ltOK.Name = "ltOK"
        Me.ltOK.Size = New System.Drawing.Size(85, 23)
        Me.ltOK.TabIndex = 49
        Me.ltOK.Text = "XXX"
        Me.ltOK.UseVisualStyleBackColor = True
        '
        'GroupingLabel
        '
        Me.GroupingLabel.Location = New System.Drawing.Point(390, 422)
        Me.GroupingLabel.Name = "GroupingLabel"
        Me.GroupingLabel.Size = New System.Drawing.Size(24, 24)
        Me.GroupingLabel.TabIndex = 51
        Me.GroupingLabel.Text = "0"
        Me.GroupingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'FramesDataGridView
        '
        Me.FramesDataGridView.AllowUserToAddRows = False
        Me.FramesDataGridView.AllowUserToDeleteRows = False
        Me.FramesDataGridView.AllowUserToOrderColumns = True
        Me.FramesDataGridView.AllowUserToResizeRows = False
        DataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle3.WrapMode = DataGridViewTriState.[False]
        Me.FramesDataGridView.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle3
        Me.FramesDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.FramesDataGridView.EditMode = DataGridViewEditMode.EditProgrammatically
        Me.FramesDataGridView.Location = New System.Drawing.Point(10, 12)
        Me.FramesDataGridView.Name = "FramesDataGridView"
        DataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle4.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle4.WrapMode = DataGridViewTriState.[False]
        Me.FramesDataGridView.RowHeadersDefaultCellStyle = DataGridViewCellStyle4
        Me.FramesDataGridView.RowHeadersVisible = False
        Me.FramesDataGridView.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White
        Me.FramesDataGridView.RowTemplate.Height = 15
        Me.FramesDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        Me.FramesDataGridView.ShowCellToolTips = False
        Me.FramesDataGridView.Size = New System.Drawing.Size(632, 365)
        Me.FramesDataGridView.TabIndex = 52
        '
        'ltClose
        '
        Me.ltClose.DialogResult = DialogResult.Cancel
        Me.ltClose.Location = New System.Drawing.Point(557, 439)
        Me.ltClose.Name = "ltClose"
        Me.ltClose.Size = New System.Drawing.Size(85, 23)
        Me.ltClose.TabIndex = 59
        Me.ltClose.Text = "XXX"
        Me.ltClose.UseVisualStyleBackColor = True
        '
        'AssociateFoldersListBox
        '
        Me.AssociateFoldersListBox.BackColor = System.Drawing.SystemColors.Window
        Me.AssociateFoldersListBox.FormattingEnabled = True
        Me.AssociateFoldersListBox.Location = New System.Drawing.Point(10, 381)
        Me.AssociateFoldersListBox.Name = "AssociateFoldersListBox"
        Me.AssociateFoldersListBox.Size = New System.Drawing.Size(226, 82)
        Me.AssociateFoldersListBox.TabIndex = 61
        '
        'ViewDesignButton
        '
        Me.ViewDesignButton.Image = Global.GadecCAD_NG.My.Resources.Resources.Design
        Me.ViewDesignButton.Location = New System.Drawing.Point(594, 380)
        Me.ViewDesignButton.Name = "ViewDesignButton"
        Me.ViewDesignButton.Size = New System.Drawing.Size(24, 24)
        Me.ViewDesignButton.TabIndex = 64
        Me.ViewDesignButton.UseVisualStyleBackColor = True
        '
        'ViewProjectButton
        '
        Me.ViewProjectButton.Image = Global.GadecCAD_NG.My.Resources.Resources.Project
        Me.ViewProjectButton.Location = New System.Drawing.Point(570, 380)
        Me.ViewProjectButton.Name = "ViewProjectButton"
        Me.ViewProjectButton.Size = New System.Drawing.Size(24, 24)
        Me.ViewProjectButton.TabIndex = 63
        Me.ViewProjectButton.UseVisualStyleBackColor = True
        '
        'ViewDescriptionButton
        '
        Me.ViewDescriptionButton.Image = Global.GadecCAD_NG.My.Resources.Resources.Descr
        Me.ViewDescriptionButton.Location = New System.Drawing.Point(522, 380)
        Me.ViewDescriptionButton.Name = "ViewDescriptionButton"
        Me.ViewDescriptionButton.Size = New System.Drawing.Size(24, 24)
        Me.ViewDescriptionButton.TabIndex = 62
        Me.ViewDescriptionButton.UseVisualStyleBackColor = True
        '
        'ltSelectAll
        '
        Me.ltSelectAll.Image = Global.GadecCAD_NG.My.Resources.Resources.SelectAll
        Me.ltSelectAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.ltSelectAll.Location = New System.Drawing.Point(390, 380)
        Me.ltSelectAll.Name = "ltSelectAll"
        Me.ltSelectAll.Size = New System.Drawing.Size(132, 24)
        Me.ltSelectAll.TabIndex = 55
        Me.ltSelectAll.Text = "XXX"
        Me.ltSelectAll.TextImageRelation = TextImageRelation.ImageBeforeText
        Me.ltSelectAll.UseVisualStyleBackColor = True
        '
        'ViewClientButton
        '
        Me.ViewClientButton.Image = Global.GadecCAD_NG.My.Resources.Resources.Client
        Me.ViewClientButton.Location = New System.Drawing.Point(546, 380)
        Me.ViewClientButton.Name = "ViewClientButton"
        Me.ViewClientButton.Size = New System.Drawing.Size(24, 24)
        Me.ViewClientButton.TabIndex = 57
        Me.ViewClientButton.UseVisualStyleBackColor = True
        '
        'ViewRevisionButton
        '
        Me.ViewRevisionButton.Image = Global.GadecCAD_NG.My.Resources.Resources.Revision
        Me.ViewRevisionButton.Location = New System.Drawing.Point(618, 380)
        Me.ViewRevisionButton.Name = "ViewRevisionButton"
        Me.ViewRevisionButton.Size = New System.Drawing.Size(24, 24)
        Me.ViewRevisionButton.TabIndex = 56
        Me.ViewRevisionButton.UseVisualStyleBackColor = True
        '
        'DownButton
        '
        Me.DownButton.Image = Global.GadecCAD_NG.My.Resources.Resources.Down
        Me.DownButton.Location = New System.Drawing.Point(389, 440)
        Me.DownButton.Name = "DownButton"
        Me.DownButton.Size = New System.Drawing.Size(24, 24)
        Me.DownButton.TabIndex = 48
        Me.DownButton.UseVisualStyleBackColor = True
        '
        'UpButton
        '
        Me.UpButton.Image = Global.GadecCAD_NG.My.Resources.Resources.Up
        Me.UpButton.Location = New System.Drawing.Point(389, 404)
        Me.UpButton.Name = "UpButton"
        Me.UpButton.Size = New System.Drawing.Size(24, 24)
        Me.UpButton.TabIndex = 47
        Me.UpButton.UseVisualStyleBackColor = True
        '
        'DrawingsDialog
        '
        Me.AcceptButton = Me.ltOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.CancelButton = Me.ltClose
        Me.ClientSize = New System.Drawing.Size(652, 474)
        Me.Controls.Add(Me.ViewDesignButton)
        Me.Controls.Add(Me.ViewProjectButton)
        Me.Controls.Add(Me.ViewDescriptionButton)
        Me.Controls.Add(Me.ltSelectAll)
        Me.Controls.Add(Me.AssociateFoldersListBox)
        Me.Controls.Add(Me.ltClose)
        Me.Controls.Add(Me.ViewClientButton)
        Me.Controls.Add(Me.ViewRevisionButton)
        Me.Controls.Add(Me.FramesDataGridView)
        Me.Controls.Add(Me.ltOK)
        Me.Controls.Add(Me.DownButton)
        Me.Controls.Add(Me.UpButton)
        Me.Controls.Add(Me.GroupsListBox)
        Me.Controls.Add(Me.ltCancel)
        Me.Controls.Add(Me.GroupingLabel)
        Me.Name = "DrawingsDialog"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Text = "fDrawinglist"
        CType(Me.FramesDataGridView, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ltCancel As Button
    Friend WithEvents DownButton As Button
    Friend WithEvents UpButton As Button
    Friend WithEvents GroupsListBox As ListBox
    Friend WithEvents ltOK As Button
    Friend WithEvents GroupingLabel As Label
    Friend WithEvents FramesDataGridView As DataGridView
    Friend WithEvents ltSelectAll As Button
    Friend WithEvents ViewRevisionButton As Button
    Friend WithEvents ViewClientButton As Button
    Friend WithEvents ltClose As Button
    Friend WithEvents AssociateFoldersListBox As ListBox
    Friend WithEvents ViewDescriptionButton As Button
    Friend WithEvents ViewProjectButton As Button
    Friend WithEvents ViewDesignButton As Button
End Class
