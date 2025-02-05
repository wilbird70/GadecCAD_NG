'Gadec Engineerings Software (c) 2022
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' <para><see cref="TextBalloon"/> provides a text balloon.</para>
''' <para>It is used to display data of a frame if it doesn't have an image yet.</para>
''' </summary>
Public Class TextBalloon
    ''' <summary>
    ''' Contains a timer that ticks (20 ms) to slide in the image.
    ''' </summary>
    Private ReadOnly _slideInTimer As New Timer
    ''' <summary>
    ''' Contains the color for the border.
    ''' </summary>
    Private ReadOnly _borderColor As Color
    ''' <summary>
    ''' Contains the top-left or top-right (depending on the alignment) position of the balloon.
    ''' </summary>
    Private ReadOnly _position As Point
    ''' <summary>
    ''' Determine to which side of the position the image will slide in.
    ''' </summary>
    Private ReadOnly _alignment As HorizontalAlignment
    ''' <summary>
    ''' Contains the width of the preview image.
    ''' </summary>
    Private ReadOnly _previewWidth As Integer = 236
    ''' <summary>
    ''' The present framelist record.
    ''' </summary>
    Private ReadOnly _frameListRow As DataRow
    ''' <summary>
    ''' Controls the insertion speed, which is the number of units per tick.
    ''' </summary>
    Private ReadOnly _speed As Integer

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="TextBalloon"/> with the specified properties.
    ''' <para><see cref="TextBalloon"/> provides a text balloon.</para>
    ''' <para>It is used to display data of a frame if it doesn't have an image yet.</para>
    ''' </summary>
    ''' <param name="frameListRow">The framelist record to display.</param>
    ''' <param name="position">The starting point (appendix) of the balloon.</param>
    ''' <param name="alignment">Side where the image should be displayed.</param>
    ''' <param name="borderColor">The color for the border.</param>
    Sub New(frameListRow As DataRow, position As Point, alignment As HorizontalAlignment, borderColor As Color)
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        _frameListRow = frameListRow
        _position = New Point(position.X, position.Y - 20)
        _alignment = alignment
        _borderColor = borderColor

        If NotNothing(_frameListRow) Then
            For Each control In Me.Controls.ToArray
                If Not control.Name = "cPreview" Then
                    control.Text = _frameListRow.GetString(control.Name.EraseStart(1))
                    If control.Name = "cDescr3" Then cDescr4.Left = cDescr3.Left + cDescr3.Width + 3
                    If control.Name = "cClient3" Then cClient4.Left = cClient3.Left + cClient3.Width + 3
                End If
            Next
            For Each control In Me.Controls.ToArray
                control.Tag = control.Left
                Select Case True
                    Case control.Name = "cBack"
                    Case control.Left + control.Width + 5 < _previewWidth
                    Case Else : _previewWidth = control.Left + control.Width + 5
                End Select
            Next
        End If
        Me.TopMost = True

        _speed = _previewWidth / 5

        Me.SetDesktopLocation(0, 0)
        Me.TopMost = True
        Me.Show()
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the dialogbox is loading.
    ''' <para>It changes the size and position of controls depending on the alignment.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            If _alignment = HorizontalAlignment.Left Then
                For Each control In Me.Controls.ToArray
                    Select Case True
                        Case control.Name = "cBack"
                        Case Else : control.Left -= _previewWidth
                    End Select
                Next
            End If
            Me.Width = 0
            AddHandler _slideInTimer.Tick, AddressOf TimerTickSlideInEventHandler
            _slideInTimer.Interval = 20
            _slideInTimer.Enabled = True
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the dialog is painted.
    ''' <para>It redraws the border and arrow of the balloon.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        Try
            Select Case _alignment
                Case HorizontalAlignment.Left
                    Dim contour = PointArray((4, 0), (Me.Width - 1, 0), (Me.Width - 1, Me.Height - 1), (4, Me.Height - 1), (4, 24), (0, 20), (4, 16), (4, 0))
                    Dim arrow = PointArray((5, 25), (0, 20), (5, 15))
                    e.Graphics.FillPolygon(New SolidBrush(Color.Black), arrow)
                    e.Graphics.DrawLines(New Pen(_borderColor, 1), contour)
                Case HorizontalAlignment.Right
                    Dim contour = PointArray((Me.Width - 5, 0), (0, 0), (0, Me.Height - 1), (Me.Width - 5, Me.Height - 1), (Me.Width - 5, 24), (Me.Width - 1, 20), (Me.Width - 5, 16), (Me.Width - 5, 0))
                    Dim arrow = PointArray((Me.Width - 6, 25), (Me.Width - 1, 20), (Me.Width - 6, 15))
                    e.Graphics.FillPolygon(New SolidBrush(Color.Black), arrow)
                    e.Graphics.DrawLines(New Pen(_borderColor, 1), contour)
            End Select
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' EventHandler for the event that occurs when the user hovers the mouse over the dialog.
    ''' <para>It hides (disposes) the dialog.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        Try
            Me.Dispose()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'eventhandlers

    ''' <summary>
    ''' EventHandler for the event that occurs when the timer ticks.
    ''' <para>It changes the width of the dialog with the speed value until the data is fully displayed.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TimerTickSlideInEventHandler(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If Me.IsDisposed Then _slideInTimer.Enabled = False
            Dim newWidth = Me.Width + _speed
            Select Case _alignment
                Case HorizontalAlignment.Left
                    Me.SetDesktopLocation(_position.X, _position.Y)
                    Select Case newWidth < _previewWidth
                        Case True
                            Me.Width = newWidth
                            For Each c As Control In Me.Controls
                                Select Case True
                                    Case c.Name = "cBack"
                                    Case Else : c.Left += _speed
                                End Select
                            Next
                        Case Else
                            For Each c As Control In Me.Controls
                                Select Case True
                                    Case c.Name = "cBack"
                                    Case Else : c.Left = c.Tag
                                End Select
                            Next
                            Me.Width = _previewWidth
                            BackPictureBox.Left = 5
                            BackPictureBox.Width = Me.Width - 6
                            _slideInTimer.Enabled = False
                    End Select
                Case HorizontalAlignment.Right
                    Select Case newWidth < _previewWidth
                        Case True
                            Me.Width = newWidth
                            Me.SetDesktopLocation(_position.X - Me.Width, _position.Y)
                        Case Else
                            Me.Width = _previewWidth
                            BackPictureBox.Left = 1
                            BackPictureBox.Width = Me.Width - 6
                            Me.SetDesktopLocation(_position.X - Me.Width, _position.Y)
                            _slideInTimer.Enabled = False
                    End Select
            End Select
            Me.Refresh()
        Catch ex As Exception
            _slideInTimer.Enabled = False
        End Try
    End Sub

    'private functions

    ''' <summary>
    ''' Turns a list of tuples (with X and Y values) into a list of points.
    ''' </summary>
    ''' <param name="points">The list of tuples (with X and Y values).</param>
    ''' <returns>The list of points.</returns>
    Private Function PointArray(ParamArray points As (x As Integer, y As Integer)()) As Point()
        Return points.Select(Function(x) New Point(x.x, x.y)).ToArray
    End Function

End Class