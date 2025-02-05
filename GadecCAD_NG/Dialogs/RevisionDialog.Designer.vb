<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RevisionDialog
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(RevisionDialog))
        Me.ltOK = New System.Windows.Forms.Button()
        Me.ltCancel = New System.Windows.Forms.Button()
        Me.DrawnTextBox = New System.Windows.Forms.TextBox()
        Me.CheckTextBox = New System.Windows.Forms.TextBox()
        Me.ltDescr = New System.Windows.Forms.Label()
        Me.ltCheck = New System.Windows.Forms.Label()
        Me.ltDrawn = New System.Windows.Forms.Label()
        Me.ltDate = New System.Windows.Forms.Label()
        Me.InputDateTimePicker = New System.Windows.Forms.DateTimePicker()
        Me.DescriptionComboBox = New System.Windows.Forms.ComboBox()
        Me.SuspendLayout()
        '
        'ltOK
        '
        Me.ltOK.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.ltOK.Location = New System.Drawing.Point(190, 88)
        Me.ltOK.Name = "ltOK"
        Me.ltOK.Size = New System.Drawing.Size(86, 23)
        Me.ltOK.TabIndex = 2
        Me.ltOK.Text = "XXX"
        Me.ltOK.UseVisualStyleBackColor = True
        '
        'ltCancel
        '
        Me.ltCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.ltCancel.Location = New System.Drawing.Point(282, 88)
        Me.ltCancel.Name = "ltCancel"
        Me.ltCancel.Size = New System.Drawing.Size(85, 23)
        Me.ltCancel.TabIndex = 3
        Me.ltCancel.Text = "XXX"
        Me.ltCancel.UseVisualStyleBackColor = True
        '
        'DrawnTextBox
        '
        Me.DrawnTextBox.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.DrawnTextBox.Location = New System.Drawing.Point(190, 25)
        Me.DrawnTextBox.Name = "DrawnTextBox"
        Me.DrawnTextBox.Size = New System.Drawing.Size(177, 18)
        Me.DrawnTextBox.TabIndex = 28
        '
        'CheckTextBox
        '
        Me.CheckTextBox.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CheckTextBox.Location = New System.Drawing.Point(190, 62)
        Me.CheckTextBox.Name = "CheckTextBox"
        Me.CheckTextBox.Size = New System.Drawing.Size(177, 18)
        Me.CheckTextBox.TabIndex = 29
        '
        'ltDescr
        '
        Me.ltDescr.Location = New System.Drawing.Point(12, 46)
        Me.ltDescr.Name = "ltDescr"
        Me.ltDescr.Size = New System.Drawing.Size(172, 13)
        Me.ltDescr.TabIndex = 31
        Me.ltDescr.Text = "XXX"
        '
        'ltCheck
        '
        Me.ltCheck.Location = New System.Drawing.Point(190, 46)
        Me.ltCheck.Name = "ltCheck"
        Me.ltCheck.Size = New System.Drawing.Size(177, 13)
        Me.ltCheck.TabIndex = 33
        Me.ltCheck.Text = "XXX"
        '
        'ltDrawn
        '
        Me.ltDrawn.Location = New System.Drawing.Point(190, 9)
        Me.ltDrawn.Name = "ltDrawn"
        Me.ltDrawn.Size = New System.Drawing.Size(177, 13)
        Me.ltDrawn.TabIndex = 32
        Me.ltDrawn.Text = "XXX"
        '
        'ltDate
        '
        Me.ltDate.Location = New System.Drawing.Point(12, 9)
        Me.ltDate.Name = "ltDate"
        Me.ltDate.Size = New System.Drawing.Size(172, 13)
        Me.ltDate.TabIndex = 30
        Me.ltDate.Text = "XXX"
        '
        'InputDateTimePicker
        '
        Me.InputDateTimePicker.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.InputDateTimePicker.Location = New System.Drawing.Point(12, 25)
        Me.InputDateTimePicker.Name = "InputDateTimePicker"
        Me.InputDateTimePicker.Size = New System.Drawing.Size(172, 18)
        Me.InputDateTimePicker.TabIndex = 34
        '
        'DescriptionComboBox
        '
        Me.DescriptionComboBox.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.DescriptionComboBox.FormattingEnabled = True
        Me.DescriptionComboBox.Location = New System.Drawing.Point(12, 62)
        Me.DescriptionComboBox.Name = "DescriptionComboBox"
        Me.DescriptionComboBox.Size = New System.Drawing.Size(172, 20)
        Me.DescriptionComboBox.TabIndex = 35
        '
        'RevisionDialog
        '
        Me.AcceptButton = Me.ltOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.ltCancel
        Me.ClientSize = New System.Drawing.Size(379, 123)
        Me.Controls.Add(Me.DescriptionComboBox)
        Me.Controls.Add(Me.InputDateTimePicker)
        Me.Controls.Add(Me.DrawnTextBox)
        Me.Controls.Add(Me.CheckTextBox)
        Me.Controls.Add(Me.ltDescr)
        Me.Controls.Add(Me.ltCheck)
        Me.Controls.Add(Me.ltDrawn)
        Me.Controls.Add(Me.ltDate)
        Me.Controls.Add(Me.ltOK)
        Me.Controls.Add(Me.ltCancel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "RevisionDialog"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "XXX"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ltOK As System.Windows.Forms.Button
    Friend WithEvents ltCancel As System.Windows.Forms.Button
    Friend WithEvents DrawnTextBox As System.Windows.Forms.TextBox
    Friend WithEvents CheckTextBox As System.Windows.Forms.TextBox
    Friend WithEvents ltDescr As System.Windows.Forms.Label
    Friend WithEvents ltCheck As System.Windows.Forms.Label
    Friend WithEvents ltDrawn As System.Windows.Forms.Label
    Friend WithEvents ltDate As System.Windows.Forms.Label
    Friend WithEvents InputDateTimePicker As System.Windows.Forms.DateTimePicker
    Friend WithEvents DescriptionComboBox As System.Windows.Forms.ComboBox
End Class
