<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ListBoxDialog
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ListBoxDialog))
        Me.ltOK = New System.Windows.Forms.Button()
        Me.ltCancel = New System.Windows.Forms.Button()
        Me.InputListBox = New System.Windows.Forms.ListBox()
        Me.lDescr = New System.Windows.Forms.Label()
        Me.InputCheckBox = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        '
        'ltOK
        '
        Me.ltOK.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.ltOK.Location = New System.Drawing.Point(190, 139)
        Me.ltOK.Name = "ltOK"
        Me.ltOK.Size = New System.Drawing.Size(86, 23)
        Me.ltOK.TabIndex = 8
        Me.ltOK.Text = "XXX"
        Me.ltOK.UseVisualStyleBackColor = True
        '
        'ltCancel
        '
        Me.ltCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.ltCancel.Location = New System.Drawing.Point(282, 139)
        Me.ltCancel.Name = "ltCancel"
        Me.ltCancel.Size = New System.Drawing.Size(85, 23)
        Me.ltCancel.TabIndex = 9
        Me.ltCancel.Text = "XXX"
        Me.ltCancel.UseVisualStyleBackColor = True
        '
        'InputListBox
        '
        Me.InputListBox.FormattingEnabled = True
        Me.InputListBox.Location = New System.Drawing.Point(12, 25)
        Me.InputListBox.Name = "InputListBox"
        Me.InputListBox.Size = New System.Drawing.Size(355, 108)
        Me.InputListBox.TabIndex = 10
        '
        'lDescr
        '
        Me.lDescr.AutoSize = True
        Me.lDescr.Location = New System.Drawing.Point(12, 9)
        Me.lDescr.Name = "lDescr"
        Me.lDescr.Size = New System.Drawing.Size(28, 13)
        Me.lDescr.TabIndex = 11
        Me.lDescr.Text = "XXX"
        '
        'InputCheckBox
        '
        Me.InputCheckBox.AutoSize = True
        Me.InputCheckBox.Location = New System.Drawing.Point(14, 144)
        Me.InputCheckBox.Name = "InputCheckBox"
        Me.InputCheckBox.Size = New System.Drawing.Size(47, 17)
        Me.InputCheckBox.TabIndex = 12
        Me.InputCheckBox.Text = "XXX"
        Me.InputCheckBox.UseVisualStyleBackColor = True
        Me.InputCheckBox.Visible = False
        '
        'ListBoxDialog
        '
        Me.AcceptButton = Me.ltOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.ltCancel
        Me.ClientSize = New System.Drawing.Size(379, 174)
        Me.Controls.Add(Me.InputCheckBox)
        Me.Controls.Add(Me.InputListBox)
        Me.Controls.Add(Me.ltOK)
        Me.Controls.Add(Me.ltCancel)
        Me.Controls.Add(Me.lDescr)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "ListBoxDialog"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "XXX"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ltOK As System.Windows.Forms.Button
    Friend WithEvents ltCancel As System.Windows.Forms.Button
    Friend WithEvents InputListBox As System.Windows.Forms.ListBox
    Friend WithEvents lDescr As System.Windows.Forms.Label
    Friend WithEvents InputCheckBox As System.Windows.Forms.CheckBox
End Class
