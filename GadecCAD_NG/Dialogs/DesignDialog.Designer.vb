<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DesignDialog
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(DesignDialog))
        Me.ltOK = New System.Windows.Forms.Button()
        Me.ltCancel = New System.Windows.Forms.Button()
        Me.ColumnsTextBoxDA = New System.Windows.Forms.TextBox()
        Me.ltRowsDA = New System.Windows.Forms.Label()
        Me.ColumnDistanceTextBoxDA = New System.Windows.Forms.TextBox()
        Me.RowsTextBoxDA = New System.Windows.Forms.TextBox()
        Me.ltColumnsDA = New System.Windows.Forms.Label()
        Me.RowDistanceTextBoxDA = New System.Windows.Forms.TextBox()
        Me.xaLabelDA = New System.Windows.Forms.Label()
        Me.xbLabelDA = New System.Windows.Forms.Label()
        Me.ItemsListBox = New System.Windows.Forms.ListBox()
        Me.FactorySettingsButton = New System.Windows.Forms.Button()
        Me.RotateToLeftButtonDW = New System.Windows.Forms.Button()
        Me.RotateToRightButtonDW = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'ltOK
        '
        Me.ltOK.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.ltOK.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ltOK.Location = New System.Drawing.Point(95, 201)
        Me.ltOK.Name = "ltOK"
        Me.ltOK.Size = New System.Drawing.Size(86, 23)
        Me.ltOK.TabIndex = 9
        Me.ltOK.Text = "XXX"
        Me.ltOK.UseVisualStyleBackColor = True
        '
        'ltCancel
        '
        Me.ltCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.ltCancel.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ltCancel.Location = New System.Drawing.Point(187, 201)
        Me.ltCancel.Name = "ltCancel"
        Me.ltCancel.Size = New System.Drawing.Size(85, 23)
        Me.ltCancel.TabIndex = 8
        Me.ltCancel.Text = "XXX"
        Me.ltCancel.UseVisualStyleBackColor = True
        '
        'ColumnsTextBoxDA
        '
        Me.ColumnsTextBoxDA.Font = New System.Drawing.Font("Arial Narrow", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ColumnsTextBoxDA.Location = New System.Drawing.Point(187, 153)
        Me.ColumnsTextBoxDA.Name = "ColumnsTextBoxDA"
        Me.ColumnsTextBoxDA.Size = New System.Drawing.Size(38, 20)
        Me.ColumnsTextBoxDA.TabIndex = 13
        '
        'ltRowsDA
        '
        Me.ltRowsDA.Font = New System.Drawing.Font("Arial Narrow", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ltRowsDA.Location = New System.Drawing.Point(231, 137)
        Me.ltRowsDA.Name = "ltRowsDA"
        Me.ltRowsDA.Size = New System.Drawing.Size(41, 13)
        Me.ltRowsDA.TabIndex = 14
        Me.ltRowsDA.Text = "XXX"
        '
        'ColumnDistanceTextBoxDA
        '
        Me.ColumnDistanceTextBoxDA.Font = New System.Drawing.Font("Arial Narrow", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ColumnDistanceTextBoxDA.Location = New System.Drawing.Point(187, 175)
        Me.ColumnDistanceTextBoxDA.Name = "ColumnDistanceTextBoxDA"
        Me.ColumnDistanceTextBoxDA.Size = New System.Drawing.Size(38, 20)
        Me.ColumnDistanceTextBoxDA.TabIndex = 15
        '
        'RowsTextBoxDA
        '
        Me.RowsTextBoxDA.Font = New System.Drawing.Font("Arial Narrow", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RowsTextBoxDA.Location = New System.Drawing.Point(234, 153)
        Me.RowsTextBoxDA.Name = "RowsTextBoxDA"
        Me.RowsTextBoxDA.Size = New System.Drawing.Size(38, 20)
        Me.RowsTextBoxDA.TabIndex = 16
        '
        'ltColumnsDA
        '
        Me.ltColumnsDA.Font = New System.Drawing.Font("Arial Narrow", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ltColumnsDA.Location = New System.Drawing.Point(184, 137)
        Me.ltColumnsDA.Name = "ltColumnsDA"
        Me.ltColumnsDA.Size = New System.Drawing.Size(49, 13)
        Me.ltColumnsDA.TabIndex = 17
        Me.ltColumnsDA.Text = "XXX"
        '
        'RowDistanceTextBoxDA
        '
        Me.RowDistanceTextBoxDA.Font = New System.Drawing.Font("Arial Narrow", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RowDistanceTextBoxDA.Location = New System.Drawing.Point(234, 175)
        Me.RowDistanceTextBoxDA.Name = "RowDistanceTextBoxDA"
        Me.RowDistanceTextBoxDA.Size = New System.Drawing.Size(38, 20)
        Me.RowDistanceTextBoxDA.TabIndex = 18
        '
        'xaLabelDA
        '
        Me.xaLabelDA.Font = New System.Drawing.Font("Arial Narrow", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xaLabelDA.Location = New System.Drawing.Point(223, 156)
        Me.xaLabelDA.Name = "xaLabelDA"
        Me.xaLabelDA.Size = New System.Drawing.Size(10, 13)
        Me.xaLabelDA.TabIndex = 19
        Me.xaLabelDA.Text = "x"
        '
        'xbLabelDA
        '
        Me.xbLabelDA.Font = New System.Drawing.Font("Arial Narrow", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.xbLabelDA.Location = New System.Drawing.Point(223, 178)
        Me.xbLabelDA.Name = "xbLabelDA"
        Me.xbLabelDA.Size = New System.Drawing.Size(10, 13)
        Me.xbLabelDA.TabIndex = 20
        Me.xbLabelDA.Text = "x"
        '
        'ItemsListBox
        '
        Me.ItemsListBox.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ItemsListBox.FormattingEnabled = True
        Me.ItemsListBox.ItemHeight = 14
        Me.ItemsListBox.Location = New System.Drawing.Point(12, 9)
        Me.ItemsListBox.Name = "ItemsListBox"
        Me.ItemsListBox.Size = New System.Drawing.Size(169, 186)
        Me.ItemsListBox.TabIndex = 21
        '
        'FactorySettingsButton
        '
        Me.FactorySettingsButton.Image = Global.GadecCAD_NG.My.Resources.Resources.Factory
        Me.FactorySettingsButton.Location = New System.Drawing.Point(188, 110)
        Me.FactorySettingsButton.Name = "FactorySettingsButton"
        Me.FactorySettingsButton.Size = New System.Drawing.Size(24, 24)
        Me.FactorySettingsButton.TabIndex = 24
        Me.FactorySettingsButton.UseVisualStyleBackColor = True
        '
        'RotateToLeftButtonDW
        '
        Me.RotateToLeftButtonDW.Image = Global.GadecCAD_NG.My.Resources.Resources.Rotate
        Me.RotateToLeftButtonDW.Location = New System.Drawing.Point(248, 110)
        Me.RotateToLeftButtonDW.Name = "RotateToLeftButtonDW"
        Me.RotateToLeftButtonDW.Size = New System.Drawing.Size(24, 24)
        Me.RotateToLeftButtonDW.TabIndex = 23
        Me.RotateToLeftButtonDW.UseVisualStyleBackColor = True
        '
        'RotateToRightButtonDW
        '
        Me.RotateToRightButtonDW.Image = Global.GadecCAD_NG.My.Resources.Resources.Rotate2
        Me.RotateToRightButtonDW.Location = New System.Drawing.Point(218, 110)
        Me.RotateToRightButtonDW.Name = "RotateToRightButtonDW"
        Me.RotateToRightButtonDW.Size = New System.Drawing.Size(24, 24)
        Me.RotateToRightButtonDW.TabIndex = 22
        Me.RotateToRightButtonDW.UseVisualStyleBackColor = True
        '
        'DesignDialog
        '
        Me.AcceptButton = Me.ltOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.ltCancel
        Me.ClientSize = New System.Drawing.Size(284, 236)
        Me.Controls.Add(Me.FactorySettingsButton)
        Me.Controls.Add(Me.RotateToLeftButtonDW)
        Me.Controls.Add(Me.RotateToRightButtonDW)
        Me.Controls.Add(Me.ltRowsDA)
        Me.Controls.Add(Me.RowDistanceTextBoxDA)
        Me.Controls.Add(Me.ltColumnsDA)
        Me.Controls.Add(Me.RowsTextBoxDA)
        Me.Controls.Add(Me.ColumnDistanceTextBoxDA)
        Me.Controls.Add(Me.ColumnsTextBoxDA)
        Me.Controls.Add(Me.ltOK)
        Me.Controls.Add(Me.ltCancel)
        Me.Controls.Add(Me.xbLabelDA)
        Me.Controls.Add(Me.xaLabelDA)
        Me.Controls.Add(Me.ItemsListBox)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "DesignDialog"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "XXX"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ltOK As System.Windows.Forms.Button
    Friend WithEvents ltCancel As System.Windows.Forms.Button
    Friend WithEvents ColumnsTextBoxDA As System.Windows.Forms.TextBox
    Friend WithEvents ltRowsDA As System.Windows.Forms.Label
    Friend WithEvents ColumnDistanceTextBoxDA As System.Windows.Forms.TextBox
    Friend WithEvents RowsTextBoxDA As System.Windows.Forms.TextBox
    Friend WithEvents ltColumnsDA As System.Windows.Forms.Label
    Friend WithEvents RowDistanceTextBoxDA As System.Windows.Forms.TextBox
    Friend WithEvents xaLabelDA As System.Windows.Forms.Label
    Friend WithEvents xbLabelDA As System.Windows.Forms.Label
    Friend WithEvents ItemsListBox As System.Windows.Forms.ListBox
    Friend WithEvents RotateToRightButtonDW As System.Windows.Forms.Button
    Friend WithEvents RotateToLeftButtonDW As System.Windows.Forms.Button
    Friend WithEvents FactorySettingsButton As System.Windows.Forms.Button
End Class
