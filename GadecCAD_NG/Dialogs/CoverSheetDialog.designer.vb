Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CoverSheetDialog
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
    'Do not modify it using the code editor
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(CoverSheetDialog))
        Me.ltCancel = New Button()
        Me.Description1TextBox = New TextBox()
        Me.Description2TextBox = New TextBox()
        Me.ltSystem = New Label()
        Me.ltDescription = New Label()
        Me.ltStatus = New Label()
        Me.StatusComboBox = New ComboBox()
        Me.SystemTextBox = New TextBox()
        Me.ltAttachments = New Button()
        Me.ltOK = New Button()
        Me.InformationButton = New Button()
        Me.SuspendLayout()
        '
        'ltCancel
        '
        Me.ltCancel.DialogResult = DialogResult.Cancel
        Me.ltCancel.Location = New System.Drawing.Point(282, 110)
        Me.ltCancel.Name = "ltCancel"
        Me.ltCancel.Size = New System.Drawing.Size(85, 23)
        Me.ltCancel.TabIndex = 3
        Me.ltCancel.Text = "XXX"
        Me.ltCancel.UseVisualStyleBackColor = True
        '
        'Description1TextBox
        '
        Me.Description1TextBox.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Description1TextBox.Location = New System.Drawing.Point(190, 36)
        Me.Description1TextBox.Name = "Description1TextBox"
        Me.Description1TextBox.Size = New System.Drawing.Size(177, 18)
        Me.Description1TextBox.TabIndex = 28
        '
        'Description2TextBox
        '
        Me.Description2TextBox.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Description2TextBox.Location = New System.Drawing.Point(190, 60)
        Me.Description2TextBox.Name = "Description2TextBox"
        Me.Description2TextBox.Size = New System.Drawing.Size(177, 18)
        Me.Description2TextBox.TabIndex = 29
        '
        'ltSystem
        '
        Me.ltSystem.Location = New System.Drawing.Point(12, 12)
        Me.ltSystem.Name = "ltSystem"
        Me.ltSystem.Size = New System.Drawing.Size(77, 18)
        Me.ltSystem.TabIndex = 31
        Me.ltSystem.Text = "XXX"
        '
        'ltDescription
        '
        Me.ltDescription.Location = New System.Drawing.Point(12, 38)
        Me.ltDescription.Name = "ltDescription"
        Me.ltDescription.Size = New System.Drawing.Size(77, 18)
        Me.ltDescription.TabIndex = 32
        Me.ltDescription.Text = "XXX"
        '
        'ltStatus
        '
        Me.ltStatus.Location = New System.Drawing.Point(12, 86)
        Me.ltStatus.Name = "ltStatus"
        Me.ltStatus.Size = New System.Drawing.Size(77, 18)
        Me.ltStatus.TabIndex = 30
        Me.ltStatus.Text = "XXX"
        '
        'StatusComboBox
        '
        Me.StatusComboBox.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.StatusComboBox.FormattingEnabled = True
        Me.StatusComboBox.Location = New System.Drawing.Point(190, 84)
        Me.StatusComboBox.Name = "StatusComboBox"
        Me.StatusComboBox.Size = New System.Drawing.Size(177, 20)
        Me.StatusComboBox.TabIndex = 35
        '
        'SystemTextBox
        '
        Me.SystemTextBox.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.SystemTextBox.Location = New System.Drawing.Point(190, 12)
        Me.SystemTextBox.Name = "SystemTextBox"
        Me.SystemTextBox.Size = New System.Drawing.Size(177, 18)
        Me.SystemTextBox.TabIndex = 36
        '
        'ltAttachments
        '
        Me.ltAttachments.Location = New System.Drawing.Point(44, 110)
        Me.ltAttachments.Name = "ltAttachments"
        Me.ltAttachments.Size = New System.Drawing.Size(140, 23)
        Me.ltAttachments.TabIndex = 37
        Me.ltAttachments.Text = "XXX"
        Me.ltAttachments.UseVisualStyleBackColor = True
        '
        'ltOK
        '
        Me.ltOK.Image = CType(resources.GetObject("ltOK.Image"), System.Drawing.Image)
        Me.ltOK.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.ltOK.Location = New System.Drawing.Point(190, 110)
        Me.ltOK.Name = "ltOK"
        Me.ltOK.Size = New System.Drawing.Size(86, 23)
        Me.ltOK.TabIndex = 2
        Me.ltOK.Text = "XXX"
        Me.ltOK.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.ltOK.UseVisualStyleBackColor = True
        '
        'InformationButton
        '
        Me.InformationButton.Image = Global.GadecCAD_NG.My.Resources.Resources.Information
        Me.InformationButton.Location = New System.Drawing.Point(15, 110)
        Me.InformationButton.Name = "InformationButton"
        Me.InformationButton.Size = New System.Drawing.Size(23, 23)
        Me.InformationButton.TabIndex = 38
        Me.InformationButton.UseVisualStyleBackColor = True
        '
        'CoverSheetDialog
        '
        Me.AcceptButton = Me.ltOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.CancelButton = Me.ltCancel
        Me.ClientSize = New System.Drawing.Size(379, 145)
        Me.Controls.Add(Me.InformationButton)
        Me.Controls.Add(Me.ltAttachments)
        Me.Controls.Add(Me.SystemTextBox)
        Me.Controls.Add(Me.StatusComboBox)
        Me.Controls.Add(Me.Description1TextBox)
        Me.Controls.Add(Me.Description2TextBox)
        Me.Controls.Add(Me.ltSystem)
        Me.Controls.Add(Me.ltDescription)
        Me.Controls.Add(Me.ltStatus)
        Me.Controls.Add(Me.ltOK)
        Me.Controls.Add(Me.ltCancel)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "CoverSheetDialog"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Text = "XXX"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ltOK As Button
    Friend WithEvents ltCancel As Button
    Friend WithEvents Description1TextBox As TextBox
    Friend WithEvents Description2TextBox As TextBox
    Friend WithEvents ltSystem As Label
    Friend WithEvents ltDescription As Label
    Friend WithEvents ltStatus As Label
    Friend WithEvents StatusComboBox As ComboBox
    Friend WithEvents SystemTextBox As TextBox
    Friend WithEvents ltAttachments As Button
    Friend WithEvents InformationButton As Button
End Class
