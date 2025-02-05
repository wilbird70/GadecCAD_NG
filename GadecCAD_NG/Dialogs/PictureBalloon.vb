'Gadec Engineerings Software (c) 2022
Imports System.Windows.Forms
Imports System.Drawing

''' <summary>
''' <para><see cref="PictureBalloon"/> provides an image balloon.</para>
''' <para>It is used to show an image of a frame (eg. framepalette), drawing (eg. filepalette) or symbol (eg. designcenter).</para>
''' </summary>
Public Class PictureBalloon
    ''' <summary>
    ''' Contains a timer that ticks (20 ms) to slide in the image.
    ''' </summary>
    Private ReadOnly _slideInTimer As New Timer
    ''' <summary>
    ''' Contains the image to show.
    ''' </summary>
    Private ReadOnly _bitmap As Bitmap
    ''' <summary>
    ''' Contains the color for the border.
    ''' </summary>
    Private ReadOnly _borderColor As Color
    ''' <summary>
    ''' Contains the desired height of the balloon.
    ''' </summary>
    Private ReadOnly _height As Integer
    ''' <summary>
    ''' Contains the top-left or top-right (depending on the alignment) position of the balloon.
    ''' </summary>
    Private ReadOnly _position As Point
    ''' <summary>
    ''' Determine to which side of the position the image will slide in.
    ''' </summary>
    Private ReadOnly _alignment As HorizontalAlignment
    ''' <summary>
    ''' Contains the text to display in the balloon.
    ''' </summary>
    Private ReadOnly _text As String
    ''' <summary>
    ''' Contains the desired height of the text.
    ''' </summary>
    Private ReadOnly _textHeight As Integer
    ''' <summary>
    ''' Contains the width of the preview image. The initial value is for an invalid image.
    ''' </summary>
    Private ReadOnly _previewWidth As Integer = 88
    ''' <summary>
    ''' Controls the insertion speed, which is the number of units per tick.
    ''' </summary>
    Private ReadOnly _speed As Integer

    'form

    ''' <summary>
    ''' Initializes a new instance of <see cref="PictureBalloon"/> with the specified properties.
    ''' <para><see cref="PictureBalloon"/> provides an image balloon.</para>
    ''' <para>It is used to show an image of a frame (eg. framepalette), drawing (eg. filepalette) or symbol (eg. designcenter).</para>
    ''' </summary>
    ''' <param name="bitmap">The image to slide in.</param>
    ''' <param name="position">The starting point (appendix) of the balloon.</param>
    ''' <param name="alignment">Side where the image should be displayed.</param>
    ''' <param name="borderColor">The color for the border.</param>
    ''' <param name="height">The desired height of the balloon.</param>
    ''' <param name="text">The text to display in the balloon.</param>
    ''' <param name="textHeight">The desired height of the text.</param>
    Sub New(bitmap As Bitmap, position As Point, alignment As HorizontalAlignment, borderColor As Color, height As Integer, text As String, textHeight As Integer)
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        _bitmap = bitmap
        _position = New Point(position.X, position.Y - 20)
        _height = height
        _alignment = alignment
        _text = text
        _textHeight = textHeight
        _borderColor = borderColor

        Me.Height = _height
        BackPictureBox.Height = _height - 2
        PreviewPictureBox.Height = _height - 10

        If IsNothing(_bitmap) Then _bitmap = PreviewPictureBox.ErrorImage
        If NotNothing(_bitmap) Then _previewWidth = _bitmap.Width * (PreviewPictureBox.Height / _bitmap.Height)

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
            Select Case True
                Case IsNothing(_bitmap)
                Case _alignment = HorizontalAlignment.Left
                    PreviewPictureBox.Width = _previewWidth
                    BackPictureBox.Width = _previewWidth + 8
                    PreviewPictureBox.Left = -PreviewPictureBox.Width - 5
                    Me.Width = 0
                    PreviewPictureBox.SizeMode = PictureBoxSizeMode.Zoom
                    PreviewPictureBox.Image = _bitmap
                    PreviewPictureBox.BringToFront()
                Case _alignment = HorizontalAlignment.Right
                    PreviewPictureBox.Width = _previewWidth
                    BackPictureBox.Width = _previewWidth + 8
                    PreviewPictureBox.Left = 5
                    Me.Width = 0
                    PreviewPictureBox.SizeMode = PictureBoxSizeMode.Zoom
                    PreviewPictureBox.Image = _bitmap
                    PreviewPictureBox.BringToFront()
            End Select
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

    'pictureboxes

    ''' <summary>
    ''' EventHandler for the event that occurs when the user hovers the mouse over the preview.
    ''' <para>It hides (disposes) the dialog.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PreviewPictureBox_MouseMove(sender As Object, e As MouseEventArgs) Handles PreviewPictureBox.MouseMove
        Try
            Me.Dispose()
        Catch ex As Exception
            GadecException(ex)
        End Try
    End Sub

    'private eventhandlers

    ''' <summary>
    ''' EventHandler for the event that occurs when the timer ticks.
    ''' <para>It changes the width of the dialog with the speed value until the preview is fully displayed.</para>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TimerTickSlideInEventHandler(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If Me.IsDisposed Then _slideInTimer.Enabled = False
            Dim newWidth = Me.Width + _speed
            Select Case _alignment
                Case 0
                    Me.SetDesktopLocation(_position.X, _position.Y)
                    Select Case newWidth < _previewWidth + 14
                        Case True
                            PreviewPictureBox.Left += _speed
                            Me.Width = newWidth
                        Case Else
                            PreviewPictureBox.Left = 10
                            BackPictureBox.Left = 5
                            Me.Width = _previewWidth + 14
                            If Not _text = "" Then ShowLabel(_text, _textHeight)
                            _slideInTimer.Enabled = False
                    End Select
                Case 1
                    Select Case newWidth < _previewWidth + 14
                        Case True
                            Me.Width = newWidth
                            Me.SetDesktopLocation(_position.X - Me.Width, _position.Y)
                        Case Else
                            Me.Width = _previewWidth + 14
                            Me.SetDesktopLocation(_position.X - Me.Width, _position.Y)
                            If Not _text = "" Then ShowLabel(_text, _textHeight)
                            _slideInTimer.Enabled = False
                    End Select
            End Select
            Me.Refresh()
        Catch ex As Exception
            _slideInTimer.Enabled = False
        End Try
    End Sub

    'private subs

    ''' <summary>
    ''' Shows an outlined label with the specified text and text height.
    ''' </summary>
    ''' <param name="text">The text to display.</param>
    ''' <param name="textHeight">The height of the text.</param>
    Private Sub ShowLabel(text As String, textHeight As Integer)
        Dim label = New OutlinedLabel(Color.Gray, 2) With {
            .Text = IO.Path.GetFileName(text),
            .Font = FontHelper.SansSerifBold(textHeight),
            .AutoSize = True,
            .ForeColor = Color.White,
            .BackColor = Color.Transparent,
            .Left = 0,
            .Top = 0
        }
        Me.Controls.Add(label)
        label.Parent = PreviewPictureBox
        label.BringToFront()
    End Sub

    'private functions

    ''' <summary>
    ''' Turns a list of tuples (with X and Y values) into a list of points.
    ''' </summary>
    ''' <param name="points">The list of tuples (with X and Y values).</param>
    ''' <returns>The list of points.</returns>
    Private Function PointArray(ParamArray points As (X As Integer, Y As Integer)()) As Point()
        Return points.Select(Function(point) New Point(point.X, point.Y)).ToArray
    End Function

End Class