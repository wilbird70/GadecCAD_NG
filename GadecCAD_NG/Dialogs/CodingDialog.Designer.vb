<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CodingDialog
    Inherits System.Windows.Forms.Form

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
    'Do not modify it using the code editor
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(CodingDialog))
        Me.ltCancel = New System.Windows.Forms.Button()
        Me.OptionsListBox = New System.Windows.Forms.ListBox()
        Me.ltLoopLine = New System.Windows.Forms.CheckBox()
        Me.ltOK = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'ltCancel
        '
        Me.ltCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.ltCancel.Location = New System.Drawing.Point(282, 139)
        Me.ltCancel.Name = "ltCancel"
        Me.ltCancel.Size = New System.Drawing.Size(85, 23)
        Me.ltCancel.TabIndex = 37
        Me.ltCancel.Text = "XXX"
        Me.ltCancel.UseVisualStyleBackColor = True
        '
        'OptionsListBox
        '
        Me.OptionsListBox.BackColor = System.Drawing.SystemColors.Window
        Me.OptionsListBox.FormattingEnabled = True
        Me.OptionsListBox.Location = New System.Drawing.Point(282, 12)
        Me.OptionsListBox.Name = "OptionsListBox"
        Me.OptionsListBox.Size = New System.Drawing.Size(85, 56)
        Me.OptionsListBox.TabIndex = 38
        '
        'ltLoopLine
        '
        Me.ltLoopLine.AutoSize = True
        Me.ltLoopLine.Location = New System.Drawing.Point(282, 74)
        Me.ltLoopLine.Name = "ltLoopLine"
        Me.ltLoopLine.Size = New System.Drawing.Size(47, 17)
        Me.ltLoopLine.TabIndex = 39
        Me.ltLoopLine.Text = "XXX"
        Me.ltLoopLine.UseVisualStyleBackColor = True
        '
        'ltOK
        '
        Me.ltOK.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.ltOK.Location = New System.Drawing.Point(281, 110)
        Me.ltOK.Name = "ltOK"
        Me.ltOK.Size = New System.Drawing.Size(86, 23)
        Me.ltOK.TabIndex = 36
        Me.ltOK.Text = "XXX"
        Me.ltOK.UseVisualStyleBackColor = True
        '
        'CodingDialog
        '
        Me.AcceptButton = Me.ltOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.ltCancel
        Me.ClientSize = New System.Drawing.Size(379, 174)
        Me.Controls.Add(Me.ltLoopLine)
        Me.Controls.Add(Me.OptionsListBox)
        Me.Controls.Add(Me.ltOK)
        Me.Controls.Add(Me.ltCancel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "CodingDialog"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "XXX"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ltOK As System.Windows.Forms.Button
    Friend WithEvents ltCancel As System.Windows.Forms.Button
    Friend WithEvents OptionsListBox As System.Windows.Forms.ListBox
    Friend WithEvents ltLoopLine As System.Windows.Forms.CheckBox
End Class
