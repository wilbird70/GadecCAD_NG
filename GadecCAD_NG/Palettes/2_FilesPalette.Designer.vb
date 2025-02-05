<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FilesPalette
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
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
    'Do not modify it using the code editor
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.ltDrawings = New System.Windows.Forms.Label()
        Me.FilesDataGridView = New System.Windows.Forms.DataGridView()
        Me.OpenFolderButton = New System.Windows.Forms.Button()
        Me.ltSelectAll = New System.Windows.Forms.Button()
        CType(Me.FilesDataGridView, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ltDrawings
        '
        Me.ltDrawings.AutoSize = True
        Me.ltDrawings.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ltDrawings.Location = New System.Drawing.Point(3, 11)
        Me.ltDrawings.Name = "ltDrawings"
        Me.ltDrawings.Size = New System.Drawing.Size(31, 13)
        Me.ltDrawings.TabIndex = 41
        Me.ltDrawings.Text = "XXX"
        '
        'FilesDataGridView
        '
        Me.FilesDataGridView.AllowUserToAddRows = False
        Me.FilesDataGridView.AllowUserToDeleteRows = False
        Me.FilesDataGridView.AllowUserToResizeColumns = False
        Me.FilesDataGridView.AllowUserToResizeRows = False
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.FilesDataGridView.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle1
        Me.FilesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.FilesDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically
        Me.FilesDataGridView.Location = New System.Drawing.Point(3, 27)
        Me.FilesDataGridView.Name = "FilesDataGridView"
        Me.FilesDataGridView.RowHeadersVisible = False
        Me.FilesDataGridView.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White
        Me.FilesDataGridView.RowTemplate.DefaultCellStyle.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FilesDataGridView.RowTemplate.Height = 15
        Me.FilesDataGridView.RowTemplate.ReadOnly = True
        Me.FilesDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.FilesDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.FilesDataGridView.ShowCellToolTips = False
        Me.FilesDataGridView.Size = New System.Drawing.Size(170, 417)
        Me.FilesDataGridView.TabIndex = 40
        '
        'OpenFolderButton
        '
        Me.OpenFolderButton.Image = Global.GadecCAD_NG.My.Resources.Resources.Folder
        Me.OpenFolderButton.Location = New System.Drawing.Point(150, 445)
        Me.OpenFolderButton.Name = "OpenFolderButton"
        Me.OpenFolderButton.Size = New System.Drawing.Size(24, 24)
        Me.OpenFolderButton.TabIndex = 42
        Me.OpenFolderButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.OpenFolderButton.UseVisualStyleBackColor = True
        '
        'ltSelectAll
        '
        Me.ltSelectAll.Image = Global.GadecCAD_NG.My.Resources.Resources.SelectAll
        Me.ltSelectAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.ltSelectAll.Location = New System.Drawing.Point(2, 445)
        Me.ltSelectAll.Name = "ltSelectAll"
        Me.ltSelectAll.Size = New System.Drawing.Size(148, 24)
        Me.ltSelectAll.TabIndex = 39
        Me.ltSelectAll.Text = "XXX"
        Me.ltSelectAll.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.ltSelectAll.UseVisualStyleBackColor = True
        '
        'Palette2
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.FilesDataGridView)
        Me.Controls.Add(Me.OpenFolderButton)
        Me.Controls.Add(Me.ltDrawings)
        Me.Controls.Add(Me.ltSelectAll)
        Me.Name = "Palette2"
        Me.Size = New System.Drawing.Size(174, 471)
        CType(Me.FilesDataGridView, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ltSelectAll As System.Windows.Forms.Button
    Friend WithEvents ltDrawings As System.Windows.Forms.Label
    Friend WithEvents FilesDataGridView As System.Windows.Forms.DataGridView
    Friend WithEvents OpenFolderButton As System.Windows.Forms.Button

End Class
