'Gadec Engineerings Software (c) 2022

''' <summary>
''' A type to indicate where a value (on the x or y axis) is located relative to the start- and endvalue.
''' </summary>
Public Enum Between As Byte
    onStartPoint = 1
    onEndPoint = 2
    onMidPoint = 3
    onFirstHalf = 4
    onSecondHalf = 5
End Enum
