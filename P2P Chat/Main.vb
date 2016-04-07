Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Xml

Module Main
    'used to gen 
    Dim rd As New Random()
    'TCP setup
    Dim listenport As Int32
    Dim sendport As Int32 = 8976
    Dim localip As IPAddress = IPAddress.Any
    Dim client As TcpClient
    Dim _listen As Thread
    Dim receiver As TcpListener
    Dim incomingIP As String
    Dim incomingPort As Integer
    'User Data INFO
    Dim IP As String
    Public ID As Int32
    Public name As String
    'XML Files
    Dim Config As XDocument
    Dim Datafile As XDocument
    Dim tempfile As XDocument
    'commands info
    Dim command As New Dictionary(Of String, Action(Of String))
    'nat

    'NAT is been Worked on


    Sub Main()



        Config = XDocument.Load("config.xml")

        If (Config.Element("configuration").Element("port").Value = "") Then

            Config.Element("configuration").SetElementValue("port", rd.Next(300, 5999))

        End If

        If (Config.Element("configuration").Element("UIport").Value = "") Then

            Config.Element("configuration").SetElementValue("UIport", Config.Element("configuration").Element("port").Value + 1)

        End If

        If (Config.Element("configuration").Element("ID").Value = "") Then

            Config.Element("configuration").SetElementValue("ID", rd.Next(0, 9999))

        End If

        If (Config.Element("configuration").Element("name").Value = "") Then

            Console.Write("Name$>")
            Config.Element("configuration").SetElementValue("name", Console.ReadLine())

        End If

        name = Config.Element("configuration").Element("name").Value
        listenport = Config.Element("configuration").Element("port").Value
        ID = Config.Element("configuration").Element("ID").Value
        Config.Save("Config.xml")
        Console.Title = "Name:" & name & ID & ";Port:" & listenport
        'NAT.CreatePortMap(New Mapping(Protocol.Tcp, listenport, listenport))
        receiver = New TcpListener(localip, listenport)

        _listen = New Thread(AddressOf ReadData)
        _listen.IsBackground = True
        _listen.Start()
        Console.Write("IP$>")

        Dim temp = Console.ReadLine()

        If (temp.Contains(";")) Then

            Dim temp1() = temp.Split(";")

            IP = temp1(0)
            sendport = temp1(1)

        Else

            IP = temp
        End If
        Console.WriteLine("IP set to:" & IP & ":" & sendport)

        'Get User Index
        Dim i As Integer = 0
        For Each xel As XElement In Datafile.Element("data").Elements("host")

            If (xel.Element("type").Value.Equals("http")) Then



                Dim request As WebRequest = WebRequest.Create(xel.Element("IP").Value)

                request.Method = "POST"
                Dim info As String
                If (xel.Element("custom").Value = "") Then
                    info = "ID=" & name & "." & ID & "&ip= " & getIP() & "&port=" & listenport & "&v=1"
                Else
                    info = xel.Element("custom")
                    info = info.Replace("{name}", name)
                    info = info.Replace("{id}", ID)
                    info = info.Replace("{port}", listenport)
                    info = info.Replace("{v}", "1")
                    info = info.Replace("{ip}", getIP())

                End If
                Dim byteArray As Byte() = Encoding.UTF8.GetBytes(info)
                request.ContentType = "application/x-www-form-urlencoded"
                request.ContentLength = byteArray.Length
                Dim dataStream As Stream = request.GetRequestStream()
                dataStream.Write(byteArray, 0, byteArray.Length)
                dataStream.Close()
                Dim response As WebResponse = request.GetResponse()
                dataStream = response.GetResponseStream()
                Dim reader As New StreamReader(dataStream)
                Dim responseFromServer As String = reader.ReadToEnd()
                tempfile = XDocument.Load(responseFromServer)
                Datafile.Root.Add(tempfile.Root.Elements())

                reader.Close()
                dataStream.Close()
                response.Close()


            ElseIf (xel.Element("type").Value.Equals("user")) Then

            End If

        Next

        command.Add("chat", AddressOf Commands.Chat)

        Dim input As String
        Do While True
            Console.Write("$>")
            input = Console.ReadLine()
            SendData(ID & ":" & input)
            input = Nothing
        Loop

    End Sub


    Public Sub SendDataPM(msg As String, _ip As String, _port As Integer)

        client = New TcpClient(_ip, _port)

        Dim data As Byte() = Encoding.ASCII.GetBytes(addheader("chat-pm") & name & ">" & msg)
        Dim stream As NetworkStream = client.GetStream
        stream.Write(data, 0, data.Length)

        client.Close()

    End Sub


    Public Sub SendData(msg As String)

        client = New TcpClient(IP, sendport)

        Dim data As Byte() = Encoding.ASCII.GetBytes(addheader("chat") & name & ">" & msg)
        Dim stream As NetworkStream = client.GetStream
        stream.Write(data, 0, data.Length)

        client.Close()

    End Sub

    Public Sub SendCommand(msg As String)

        client = New TcpClient(IP, sendport)

        Dim data As Byte() = Encoding.ASCII.GetBytes(addheader("command") & msg)
        Dim stream As NetworkStream = client.GetStream
        stream.Write(data, 0, data.Length)

        client.Close()

    End Sub

    Public Sub SendDataAll(msg As String)
        For Each xel As XElement In Datafile.Element("data").Elements("user")
            client = New TcpClient(xel.Element("IP"), xel.Element("port"))

            Dim data As Byte() = Encoding.ASCII.GetBytes(name & "." & ID & ";" & msg)
            Dim stream As NetworkStream = client.GetStream
            stream.Write(data, 0, data.Length)
            client.Close()
        Next

    End Sub

    Public Sub ReadData()
        Dim Bytes(1024) As Byte
        Dim data As String = Nothing
        receiver.Start()
        While True

            client = receiver.AcceptTcpClient()
            data = Nothing

            Dim stream As NetworkStream = client.GetStream

            Dim i As Int32

            i = stream.Read(Bytes, 0, Bytes.Length)
            While (i <> 0)

                data = Encoding.ASCII.GetString(Bytes, 0, i)
                i = stream.Read(Bytes, 0, Bytes.Length)

                'SendDataBC(ID & ";" & data)
                Dim temp() = data.Split(";")
                Dim temp1() = temp(1).Split("|")
                If (temp(0).Contains(name & "." & ID)) Then

                Else
                    command(temp1(0))(data)

                End If

            End While
            client.Close()
        End While

    End Sub

    Private Function encrypt(msg As String, key As String) As String



    End Function

    Private Function decode(msg As String) As String
        msg = msg.Replace("%01", ";")
        msg = msg.Replace("%02", "|")
        Return msg

    End Function

    Private Function encode(msg As String) As String
        msg = msg.Replace(";", "%01")
        msg = msg.Replace("|", "%02")
        Return msg

    End Function

    Private Function addheader(type As String) As String

        Return name & "." & ID & ";" & type & "|0.1;"


    End Function

    Private Function getIP() As String
        Try
            Dim ExternalIP As String
            ExternalIP = (New WebClient()).DownloadString("http://checkip.dyndns.org/")
            ExternalIP = (New Regex("\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).Matches(ExternalIP)(0).ToString()
            Return ExternalIP
        Catch

            Dim h As IPHostEntry = Nothing
            h = Dns.GetHostEntry(Dns.GetHostName)
            Return h.AddressList.GetValue(0).ToString

        End Try
    End Function

End Module
