'Gadec Engineerings Software (c) 2022

''' <summary>
''' <para><see cref="UseModel"/> contains details of the user who is using a document file.</para>
''' </summary>
Public Class UseModel
    ''' <summary>
    ''' The users name.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property UserName As String
    ''' <summary>
    ''' The users machine name.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property MachineName As String
    ''' <summary>
    ''' The time the user started using.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property TimeString As String
    ''' <summary>
    ''' True if not the current user.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ByOtherUser As Boolean

    ''' <summary>
    ''' Initializes a new instance of <see cref="UseModel"/> with the specified properties.
    ''' <para><see cref="UseModel"/> contains details of the user who is using a document file.</para>
    ''' </summary>
    ''' <param name="userName">The users name.</param>
    ''' <param name="mchineName">The users machine name.</param>
    ''' <param name="timeString">The time the user started using.</param>
    ''' <param name="byOtherUser">True if not the current user.</param>
    Public Sub New(userName As String, mchineName As String, timeString As String, byOtherUser As Boolean)
        _UserName = userName
        _MachineName = mchineName
        _TimeString = timeString
        _ByOtherUser = byOtherUser
    End Sub

End Class
