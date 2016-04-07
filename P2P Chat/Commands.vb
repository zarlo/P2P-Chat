Imports System
Public Module Commands

    Public Sub Chat(msg As String)
        Dim data() = msg.Split(";")
        Console.WriteLine(data(2))
        SendDataAll(msg)
    End Sub

    Public Sub Chat_PM(msg As String)

        Dim data() = msg.Split(";")
        If (data(1).Contains(Main.name & "." & Main.ID)) Then

        End If


    End Sub

End Module
