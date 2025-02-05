'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Windows.Forms
Imports GadecCAD_NG.Extensions

''' <summary>
''' <para><see cref="CodingDialog"/> allows the user setting the parameters for coding symbols (blockreferences).</para>
''' </summary>
Public Class CodingDialog
    ''' <summary>
    ''' Contains the detailed information about encoding symbols.
    ''' </summary>
    Private ReadOnly _symbolCodes As New SymbolCodeModel
    ''' <summary>
    ''' The database with the different encoding options.
    ''' </summary>
    Private ReadOnly _symbolData As DataTable
    ''' <summary>
    ''' A list of option names.
    ''' </summary>
    Private ReadOnly _optionList As String()
    ''' <summary>
    ''' Determines if the dialogbox is fully loaded.
    ''' </summary>
    Private ReadOnly _loaded As Boolean = False
    ''' <summary>
    ''' Color of the field where the code adds up.
    ''' </summary>
    Private ReadOnly _countUpColor As Drawing.Color = Drawing.Color.FromArgb(222, 255, 222)
    ''' <summary>
    ''' Color of the field where the code counts down.
    ''' </summary>
    Private ReadOnly _countDownColor As Drawing.Color = Drawing.Color.FromArgb(255, 222, 222)
    ''' <summary>
    ''' Color of the field that is disabled.
    ''' </summary>
    Private ReadOnly _disabledColor As Drawing.Color = Drawing.Color.LightCyan
    ''' <summary>
    ''' Color of the field that has an empty string.
    ''' </summary>
    Private ReadOnly _noStringColor As Drawing.Color = Drawing.Color.LightSteelBlue

    ''' <summary>
    ''' The current selected option name.
    ''' </summary>
    Private _currentOption As String
    ''' <summary>
    ''' The list of tags of attribute references that can be code.
    ''' </summary>
    Private _tags As String()
    ''' <summary>
    ''' A list of (previously) entered texts (key is the tag of an attribute reference).
    ''' </summary>
    Private _texts As Dictionary(Of String, String)
    ''' <summary>
    ''' Keeps track of whether the option loading is busy.
    ''' </summary>
    Private _isBusy As Boolean = False

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="CodingDialog"/>.
    ''' <para><see cref="CodingDialog"/> allows the user setting the parameters for coding symbols (blockreferences).</para>
    ''' </summary>
    ''' <param name="symbolData">The database with the different encoding options.</param>
    Sub New(symbolData As DataTable)
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        Me.Text = Registerizer.GetApplicationVersion()
        Translator.TranslateControls(Me)

        CreateControlArray()
        _symbolData = symbolData
        _optionList = symbolData.GetStringsFromColumn("Name")

        _currentOption = Registerizer.UserSetting("CodeAttributeOption")
        _symbolCodes.drawLoopLine = Registerizer.UserSetting("CodeAttributeLoopLine") = "True"

        Dim selection = 0
        For i = 0 To _optionList.Count - 1
            OptionsListBox.Items.Add(_optionList(i).Translate)
            If _optionList(i) = _currentOption Then selection = i
        Next
        OptionsListBox.SelectedIndex = selection
        _loaded = True
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

    'functions

    ''' <summary>
    ''' Gets the detailed information about encoding symbols.
    ''' </summary>
    ''' <returns>A <see cref="SymbolCodeModel"/></returns>
    Function GetSymbolCodeInfo() As SymbolCodeModel
        LoadSettingsFromRegistry()
        For Each key In _texts.Keys.ToList
            Select Case _texts(key)
                Case "" : _texts.Remove(key)
                Case " " : _texts(key) = ""
            End Select
        Next
        _symbolCodes.Texts = _texts.ToIniStringArray
        Return _symbolCodes
    End Function

    'buttons

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the OK button.
    ''' <para>It sets the buttonvalue, saves the settings to the registry and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AcceptButton_Click(sender As Object, e As EventArgs) Handles ltOK.Click
        Try
            SaveSettingsToRegistry()
            Me.Hide()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user clicks the Cancel button.
    ''' <para>It sets the buttonvalue, saves the settings to the registry and closes the dialogbox.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub CancelButton_Click(sender As Object, e As EventArgs) Handles ltCancel.Click
        Try
            SaveSettingsToRegistry()
            Me.Hide()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'checkboxes

    ''' <summary>
    ''' EventHandler for the event that occurs when the user toggles the LoopLine checkbox.
    ''' <para>It stores the value in the present <see cref="SymbolCodeModel"/>.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub LoopLineCheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles ltLoopLine.CheckedChanged
        Try
            If ltLoopLine.Enabled Then _symbolCodes.drawLoopLine = ltLoopLine.Checked
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'listboxes

    ''' <summary>
    ''' EventHandler for the event that occurs when the user changes the selected option.
    ''' <para>It saves the settings, changes the option and loads the corresponding settings.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub OptionsListBox_SelectedIndexChanged(sender As ListBox, e As EventArgs) Handles OptionsListBox.SelectedIndexChanged
        Try
            If _loaded Then SaveSettingsToRegistry()
            _currentOption = _optionList(OptionsListBox.SelectedIndex)
            LoadSettingsFromRegistry()
            LoadOption()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'eventHandlers

    ''' <summary>
    ''' EventHandler for the event that occurs when user doubleclicks an attribute textbox.
    ''' <para>It toggles between counting up, counting down and no counting for that particular attribute.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AttributeDoubleClickEventHandler(sender As Object, e As EventArgs)
        Try
            Dim control = DirectCast(sender, Control)
            Dim textBox = Me.Controls("t{0}".Compose(control.Name.MidString(2)))
            Select Case textBox.BackColor
                Case _countUpColor : _symbolCodes.CounterStep = -1
                Case _countDownColor : _symbolCodes.CounterTag = ""
                Case Else
                    _symbolCodes.CounterTag = textBox.Tag
                    _symbolCodes.CounterStep = 1
                    If Not textBox.Text.HasNumber Then _texts(textBox.Tag) = "001"
            End Select
            LoadOption()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user makes a change to an attribute textbox.
    ''' <para>It keeps track of the settings and whether the count attribute still contains a number.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AttributeChangeEventHandler(sender As Object, e As EventArgs)
        Try
            Dim textBox = DirectCast(sender, TextBox)
            Select Case True
                Case Not textBox.Tag = _symbolCodes.CounterTag
                Case Not textBox.Text.HasNumber : _symbolCodes.CounterTag = ""
            End Select
            _texts(textBox.Tag) = textBox.Text
            LoadOption()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'private subs

    ''' <summary>
    ''' Loads the selected option.
    ''' </summary>
    Private Sub LoadOption()
        If _isBusy Then Exit Sub

        _isBusy = True
        Select Case _symbolCodes.CounterTag = ""
            Case True : ltLoopLine.Enabled = False : ltLoopLine.Checked = False
            Case Else : ltLoopLine.Enabled = True : ltLoopLine.Checked = _symbolCodes.drawLoopLine
        End Select
        For i = 0 To 11
            Dim textBox = DirectCast(Me.Controls("tAttr{0}".Compose(i)), TextBox)
            Dim label = DirectCast(Me.Controls("lAttr{0}".Compose(i)), Label)
            label.Text = _tags(i).Translate
            If Not _texts.ContainsKey(_tags(i)) Then
                textBox.BackColor = _disabledColor
                textBox.Text = ""
                textBox.Enabled = False
                label.Enabled = False
                Continue For
            End If

            textBox.Tag = _tags(i)
            textBox.Text = _texts(_tags(i))
            Select Case True
                Case _tags(i) = _symbolCodes.CounterTag
                    Select Case _symbolCodes.CounterStep = 1
                        Case True : label.Text = "{0} +1".Compose(_tags(i).Translate) : textBox.BackColor = _countUpColor
                        Case Else : label.Text = "{0} -1".Compose(_tags(i).Translate) : textBox.BackColor = _countDownColor
                    End Select
                Case _texts(_tags(i)) = "" : textBox.BackColor = _noStringColor
                Case Else : textBox.BackColor = Drawing.Color.White
            End Select
            textBox.Enabled = True
            label.Enabled = True
        Next
        _isBusy = False
    End Sub

    ''' <summary>
    ''' Creates the textboxes for the attributes and its labels.
    ''' </summary>
    Private Sub CreateControlArray()
        Dim countX = 0
        Dim countY = -1
        Me.SuspendLayout()
        For i = 0 To 11
            countY += 1
            If countY > 3 Then countY = 0 : countX += 1
            Dim label = New Label
            Me.Controls.Add(label)
            With label
                .Name = "lAttr{0}".Compose(i)
                .AutoSize = False
                .TextAlign = Drawing.ContentAlignment.BottomLeft
                .Left = 10 + (countX * 90)
                .Top = 7 + (countY * 38)
                .Width = 90
                .Height = 18
                AddHandler .DoubleClick, AddressOf AttributeDoubleClickEventHandler
            End With
            Dim textBox = New TextBox
            Me.Controls.Add(textBox)
            With textBox
                .Name = "tAttr{0}".Compose(i)
                .BackColor = _noStringColor
                .Left = 10 + (countX * 90)
                .Top = 27 + (countY * 38)
                .Width = 85
                .Height = 18
                AddHandler .TextChanged, AddressOf AttributeChangeEventHandler
                AddHandler .DoubleClick, AddressOf AttributeDoubleClickEventHandler
            End With
        Next
        Me.ResumeLayout()
    End Sub

    ''' <summary>
    ''' Saves the optionsettings to the windows registry.
    ''' </summary>
    Private Sub SaveSettingsToRegistry()
        Dim texts = New List(Of String)
        For i = 0 To 11
            Dim textBox = DirectCast(Me.Controls("tAttr{0}".Compose(i)), TextBox)
            Select Case textBox.BackColor
                Case _countUpColor : texts.Add("###{0}".Compose(textBox.Text))
                Case _countDownColor : texts.Add("%%%{0}".Compose(textBox.Text))
                Case Else : texts.Add(textBox.Text)
            End Select
        Next
        Registerizer.UserSetting("CodeAttribute{0}".Compose(_currentOption), String.Join(";", texts))
        Registerizer.UserSetting("CodeAttributeOption", _currentOption)
        Registerizer.UserSetting("CodeAttributeLoopLine", _symbolCodes.drawLoopLine)
    End Sub

    ''' <summary>
    ''' Loads the optionsettings from the windows registry.
    ''' </summary>
    Private Sub LoadSettingsFromRegistry()
        Dim symbolRow = _symbolData.Rows.Find(_currentOption)
        _tags = "{0};;;;;;;;;;;;".Compose(symbolRow.GetString("Tags")).Cut
        _symbolCodes.LocationTag = symbolRow.GetString("LTag")
        _symbolCodes.CounterTag = ""
        Dim output = New Dictionary(Of String, String)
        Dim texts = "{0};;;;;;;;;;;;".Compose(Registerizer.UserSetting("CodeAttribute{0}".Compose(_currentOption))).Cut
        For i = 0 To 11
            Dim text = texts(i)
            Select Case True
                Case text.StartsWith("###")
                    text = text.EraseStart(3)
                    _symbolCodes.CounterTag = _tags(i)
                    _symbolCodes.CounterStep = 1
                Case text.StartsWith("%%%")
                    text = text.EraseStart(3)
                    _symbolCodes.CounterTag = _tags(i)
                    _symbolCodes.CounterStep = -1
            End Select
            If Not _tags(i) = "NA" Then output.TryAdd(_tags(i), text)
        Next
        _texts = output
    End Sub

End Class