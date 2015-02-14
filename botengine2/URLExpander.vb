Imports System.Net

Public Class URLExpander
    Shared targethostname As String() = {"bit.ly"}

    'Public Shared Async Function TellExpandedURL(ByVal us As String, Optional count As Integer = 0) As Task(Of String)
    '    Dim u = New Uri(us)
    '    If count < 10 AndAlso targethostname.Contains(u.Host) Then
    '        Dim sock As New Windows.Networking.Sockets.StreamSocket()
    '        Await sock.ConnectAsync(New Windows.Networking.HostName(u.Host), "80")
    '        Dim dw As New Windows.Storage.Streams.DataWriter(sock.OutputStream)
    '        dw.WriteString("GET " & u.AbsolutePath & " HTTP/1.0" & vbCrLf & vbCrLf)
    '        Await dw.StoreAsync()
    '        Dim dr As New Windows.Storage.Streams.DataReader(sock.InputStream)
    '        Dim l As Integer = Await dr.LoadAsync(10240 - 1)
    '        Dim s = dr.ReadString(l).Split(vbCrLf)
    '        sock.Dispose()
    '        For Each line In s
    '            If line.Contains("Location: ") Then
    '                Dim regex As New System.Text.RegularExpressions.Regex("Location:\s?(?<url>.*?)$")
    '                Dim match = regex.Match(line)
    '                Return Await TellExpandedURL(match.Groups("url").Value, count + 1)
    '            End If
    '        Next
    '        Return u.OriginalString
    '    Else
    '        Return us
    '    End If
    'End Function

End Class
