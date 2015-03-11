Module Main


    Function GetDictionary(ByVal str As String) As Dictionary(Of String, String)
        Dim dic As New Dictionary(Of String, String)
        If String.IsNullOrEmpty(str) Then
            Return dic
        End If
        Dim splited = str.Split("&")
        For Each kv As String In splited
            Dim kvsplited = kv.Split("=")
            dic.Add(kvsplited(0), kvsplited(1))
        Next
        Return dic
    End Function

    Sub WriteLog(ByVal msg As String)
        Console.WriteLine(String.Format("[{0}] {1}", Now, msg))
    End Sub

    Dim tw As twitter = Nothing
    Dim cts As New Threading.CancellationTokenSource

    Sub Ctrl_C(ByVal sender As Object, ByVal e As ConsoleCancelEventArgs)
        cts.Cancel()
        WriteLog("Ctrl+C has been created by user.")
        e.Cancel = True
    End Sub

    Dim directory As String = ""

    Sub Main()
        If Not IO.File.Exists("settings.txt") Then
            IO.File.Create("settings.txt").Close()
        End If

        Dim sr As New IO.StreamReader("settings.txt")
        Dim settings = GetDictionary(sr.ReadToEnd())
        sr.Close()

        If settings.ContainsKey("Directory") Then
            directory = settings("Directory")
            WriteLog(String.Format("Download Directory: {0}", directory))
        End If

        If Not settings.ContainsKey("ConsumerKey") Then
            Console.WriteLine("Please put 'ConsumerKey' and 'ConsumerSecretKey' into settings.txt.")
            Return
        End If

        Dim ckey As OAuth.KeyPair
        ckey.Key = settings("ConsumerKey")
        ckey.KeySecret = settings("ConsumerSecretKey")

        tw = New twitter With {.ConsumerKey = ckey}

        If settings.ContainsKey("AccessKey") Then
            Dim akey As OAuth.KeyPair
            akey.Key = settings("AccessKey")
            akey.KeySecret = settings("AccessSecretKey")
            tw.AccessKey = akey
        End If

        If String.IsNullOrEmpty(tw.AccessKey.Key) Then
            Dim url = tw.GetAuthURL()

            Console.WriteLine(url)
            Console.Write("PIN:")
            Dim PIN = Console.ReadLine()
            tw.GetAccessToken(PIN)

            settings("AccessKey") = tw.AccessKey.Key
            settings("AccessSecretKey") = tw.AccessKey.KeySecret

            Dim sw As New IO.StreamWriter("settings.txt", False)
            sw.Write(String.Join("&", (From e In settings Select String.Format("{0}={1}", e.Key, e.Value))))
            sw.Close()
        End If

        AddHandler Console.CancelKeyPress, AddressOf Ctrl_C

        Dim token As Threading.CancellationToken = cts.Token

        While Not token.IsCancellationRequested
            WriteLog("trying to connect userstream...")
            tw.ConnectUserStream(AddressOf UserStreamCallback, token)
            WriteLog("disconnected.")
        End While

        Console.WriteLine("anykey to continue")
        Console.ReadKey()
    End Sub

    Function GetURLs(ByVal ScreenName As String, ByVal StatusID As UInt64) As List(Of String)
        Dim wc As New Net.WebClient()
        Dim page As String = Nothing
        Dim trycount As Integer = 0
        While True
            Try
                page = wc.DownloadString(String.Format("https://twitter.com/{0}/status/{1}", ScreenName, StatusID))
                Exit While
            Catch ex As Net.WebException
                trycount += 1
                If trycount >= 5 Then
                    Throw New Exception("cannot fetch status.", ex)
                End If
            End Try
        End While
        Dim regex As New Text.RegularExpressions.Regex("<meta.*?property=""og:image"".*?content=""(?<url>.*?)"".*?>")
        Dim match = regex.Match(page)

        Dim list As New List(Of String)
        While match.Success
            Dim u = match.Groups("url").Value
            list.Add(u.Remove(u.LastIndexOf(":")))
            match = match.NextMatch
        End While

        Return list
    End Function

    Sub UserStreamCallback(msg As twitter.StreamMessage)
        Static screenname As String = ""
        Select Case msg.Type
            Case twitter.MessageType.FriendsList
                WriteLog("connection established.")
                Dim a = tw.GetAccountSettings()
                screenname = a.ScreenName
                WriteLog(String.Format("screen_name: {0}", screenname))
            Case twitter.MessageType.Event
                Dim e As twitter.Event = msg
                Select Case e.EventType
                    Case twitter.EventType.Favorite
                        If e.Source.ScreenName = screenname Then
                            Dim target As twitter.Tweet = e.TargetObject
                            Dim n As Integer = 0
                            If target.Entities IsNot Nothing AndAlso target.Entities.Media IsNot Nothing Then
                                For Each m In target.Entities.Media

                                    If m.MediaURL.Contains("pbs.twimg.com") Then
                                        Try
                                            Dim urls = GetURLs(target.User.ScreenName, target.ID)
                                            For Each u In urls
                                                Dim url = u + ":orig"
                                                Dim splittedurl As String() = u.Split(".")
                                                Dim ext = splittedurl(splittedurl.Length() - 1)
                                                Dim filename = directory + "\" + String.Format("{0}_{1}_{2}.{3}", target.User.ScreenName, target.ID, n, ext)
                                                Dim wc = New Net.WebClient()
                                                wc.DownloadFile(url, filename)
                                                WriteLog(String.Format("Downloaded: {0}", filename))
                                                n += 1
                                            Next
                                        Catch ex As Exception
                                            WriteLog(String.Format("Failed to fetch image URLs. (screenname:{0},id:{1})", target.User.ScreenName, target.ID))
                                        End Try
                                    End If
                                Next
                            End If
                        End If
                End Select
        End Select
    End Sub

End Module
