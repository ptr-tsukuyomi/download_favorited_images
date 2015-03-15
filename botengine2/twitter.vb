Imports System.Net
Imports Windows.Data.Json
Imports System.IO
Imports System.Reflection

Public Class twitter
    Public Property ConsumerKey As OAuth.KeyPair
    Public Property AccessKey As OAuth.KeyPair
    Public Property RequestKey As OAuth.KeyPair

    'Public Shared  Function ExpandURL(ByVal t As Tweet) As Task(Of Tweet)
    '    If t.RetweetedStatus Is Nothing Then
    '        If t.Entities IsNot Nothing Then
    '            t.Entities =  ExpandURL(t.Entities)
    '            If t.Entities.URLs IsNot Nothing Then
    '                For Each u In t.Entities.URLs
    '                    t.Text = t.Text.Replace(u.URL, u.ExpandedURL)
    '                Next
    '            End If
    '            If t.Entities.Media IsNot Nothing Then
    '                For Each u In t.Entities.Media
    '                    t.Text = t.Text.Replace(u.URL, u.ExpandedURL)
    '                Next
    '            End If
    '        End If
    '    Else
    '        If t.RetweetedStatus.Entities IsNot Nothing Then
    '            t.RetweetedStatus.Entities =  ExpandURL(t.RetweetedStatus.Entities)
    '            If t.RetweetedStatus.Entities.URLs IsNot Nothing Then
    '                For Each u In t.RetweetedStatus.Entities.URLs
    '                    t.RetweetedStatus.Text = t.RetweetedStatus.Text.Replace(u.URL, u.ExpandedURL)
    '                Next
    '            End If
    '            If t.RetweetedStatus.Entities.Media IsNot Nothing Then
    '                For Each u In t.RetweetedStatus.Entities.Media
    '                    t.RetweetedStatus.Text = t.RetweetedStatus.Text.Replace(u.URL, u.ExpandedURL)
    '                Next
    '            End If
    '        End If
    '    End If
    '    Return t
    'End Function

    'Public Shared  Function ExpandURL(ByVal e As Entities) As Task(Of Entities)
    '    If e.URLs IsNot Nothing Then
    '        For Each u In e.URLs
    '            u.ExpandedURL =  URLExpander.TellExpandedURL(u.ExpandedURL)
    '        Next
    '    End If
    '    Return e
    'End Function

    Public Function FollowConversation(ByVal t As Tweet) As List(Of Tweet)
        If t.In_Reply_To_ScreenName IsNot Nothing Then
            Dim list = (FollowConversation(ShowTweet(t.In_Reply_To_StatusID)))
            list.Insert(0, t)
            Return list
        Else
            Dim l = New List(Of Tweet)
            l.Add(t)
            Return l
        End If
    End Function

    Public Class TwitterAPIValues
        Inherits OAuth.OAuthValues
        Sub New(ByVal consumer As OAuth.KeyPair, ByVal method As String, ByVal url As Uri)
            MyBase.New(consumer, method, url)
        End Sub

        Dim _status As String
        Public Property Status As String
            Get
                Return _status
            End Get
            Set(value As String)
                MyBase.values("status") = value
                _status = value
            End Set
        End Property

        Dim _in_reply_to_status_id As String
        Public Property In_Reply_To_Status_Id As String
            Get
                Return _in_reply_to_status_id
            End Get
            Set(value As String)
                MyBase.values("in_reply_to_status_id") = value
                _in_reply_to_status_id = value
            End Set
        End Property

        Dim _count As String
        Public Property Count As String
            Get
                Return _count
            End Get
            Set(value As String)
                MyBase.values("count") = value
                _count = value
            End Set
        End Property

        Dim _since_id As String
        Public Property SinceID As String
            Get
                Return _since_id
            End Get
            Set(value As String)
                MyBase.values("since_id") = value
                _since_id = value
            End Set
        End Property

        Dim _id As String
        Public Property ID As String
            Get
                Return _id
            End Get
            Set(value As String)
                MyBase.values("id") = value
                _id = value
            End Set
        End Property

        Public Function GetQueryString() As String
            MyBase.ComputeOAuthSignature()
            Return String.Join("&", From p In MyBase.values Select p.Key + "=" + p.Value)
        End Function
    End Class

    Enum MessageType
        Tweet
        Delete
        Disconnect
        Warning
        FriendsList
        [Event]
        Unknown
    End Enum

    Enum DisconnectCode
        Shutdown = 1
        DuplicateStream
        ControlRequest
        Stall
        Normal
        TokenRevoked
        AdminLogout
        MaxMessageLimit = 9
        StreamException
        BrokerStall
        ShedLoad
    End Enum

    Public MustInherit Class StreamMessage
        Public MustOverride ReadOnly Property Type As MessageType
    End Class

    Public Class Warning
        Inherits StreamMessage

        Dim Code As String
        Dim Message As String

        Public Overrides ReadOnly Property Type As MessageType
            Get
                Return MessageType.Warning
            End Get
        End Property

        Public Sub New(ByVal msg As JsonObject)
            Code = msg.GetNamedString("code")
            Message = msg.GetNamedString("message")
        End Sub
    End Class

    Public Class Delete
        Inherits StreamMessage

        Public Overrides ReadOnly Property Type As MessageType
            Get
                Return MessageType.Delete
            End Get
        End Property

        Public Property ID As ULong
        Public Property UserID As ULong

        Public Sub New(ByVal _msg As JsonObject)
            Dim msg = _msg.GetNamedObject("status")
            ID = msg.GetNamedString("id_str")
            UserID = msg.GetNamedString("user_id_str")
        End Sub
    End Class

    Public Class Disconnect
        Inherits StreamMessage

        Public Property Code As DisconnectCode
        Public Property StreamName As String
        Public Property Reason As String

        Public Overrides ReadOnly Property Type As MessageType
            Get
                Return MessageType.Delete
            End Get
        End Property

        Public Sub New(ByVal msg As JsonObject)
            Code = CType(msg.GetNamedString("code"), Integer)
            StreamName = msg.GetNamedString("stream_name")
            Reason = msg.GetNamedString("reason")
        End Sub
    End Class

    Public Class FriendsList
        Inherits StreamMessage

        Public Property Friends As ULong()

        Public Overrides ReadOnly Property Type As MessageType
            Get
                Return MessageType.FriendsList
            End Get
        End Property

        Public Sub New(ByVal msg As JsonObject)
            If msg.ContainsKey("friends") Then
                Friends = (From e In msg.GetNamedArray("friends") Select CType(e.GetNumber, ULong)).ToArray()
            Else
                Friends = (From e In msg.GetNamedArray("friends_str") Select CType(e.GetString, ULong)).ToArray()
            End If
        End Sub
    End Class

    Enum EventType
        Access_Revoked
        Block
        UnBlock
        Favorite
        UnFavorite
        Follow
        UnFollow
        List_Created
        List_Destroyed
        List_Updated
        List_Member_Added
        List_Member_Removed
        List_User_Subscribed
        List_User_UnSubscribed
        User_Update
        Unknown
    End Enum

    Public Class [Event]
        Inherits StreamMessage

        Public Property Target As User
        Public Property Source As User
        Public Property EventType As EventType
        Public Property CreatedAt As DateTime
        Public Property TargetObject As StreamMessage

        Public Overrides ReadOnly Property Type As MessageType
            Get
                Return MessageType.Event
            End Get
        End Property

        Public Sub New(ByVal msg As JsonObject)
            Target = New User(msg.GetNamedObject("target"))
            Source = New User(msg.GetNamedObject("source"))
            Dim et = (From e In [Enum].GetValues(EventType.GetType) Select e Where e.ToString.ToUpper = msg.GetNamedString("event").ToUpper).FirstOrDefault()
            EventType = If(et Is Nothing, EventType.Unknown, et)
            CreatedAt = GetDateFromAtString(msg.GetNamedString("created_at"))
            If EventType = twitter.EventType.Favorite Or EventType = twitter.EventType.UnFavorite Then
                TargetObject = New Tweet(msg.GetNamedObject("target_object"))
            End If
        End Sub
    End Class

    Public Class UnknownMessage
        Inherits StreamMessage

        Public Overrides ReadOnly Property Type As MessageType
            Get
                Return MessageType.Unknown
            End Get
        End Property
    End Class

    Public Class Hashtag
        Public Property Indices As Integer()
        Public Property Text As String

        Public Sub New(ByVal hashtag As JsonObject)
            Indices = (From e In hashtag.GetNamedArray("indices") Select CType(e.GetNumber, Integer)).ToArray
            Text = hashtag.GetNamedString("text")
        End Sub
    End Class

    Enum Resize
        Crop
        Fit
    End Enum

    Public Class Size
        Public Property Height As UInteger
        Public Property Width As UInteger
        Public Property Resize As Resize

        Public Sub New(ByVal size As JsonObject)
            Height = size.GetNamedNumber("h")
            Width = size.GetNamedNumber("w")
            Resize = (From e In [Enum].GetValues(Resize.GetType) Select e Where e.ToString.ToUpper = size.GetNamedString("resize").ToUpper).First
        End Sub
    End Class

    Public Class Sizes
        Public Property Thumb As Size
        Public Property Large As Size
        Public Property Medium As Size
        Public Property Small As Size

        Public Sub New(ByVal sizes As JsonObject)
            Thumb = New Size(sizes.GetNamedObject("thumb"))
            Large = New Size(sizes.GetNamedObject("large"))
            Medium = New Size(sizes.GetNamedObject("medium"))
            Small = New Size(sizes.GetNamedObject("small"))
        End Sub
    End Class

    Public Class Media
        Public Property DislplayURL As String
        Public Property ExpandedURL As String
        Public Property ID As ULong
        Public Property Indices As Integer()
        Public Property MediaURL As String
        Public Property Sizes As Sizes
        Public Property SourceStatusID As ULong
        Public Property Type As String
        Public Property URL As String

        Public Sub New(ByVal media As JsonObject)
            DislplayURL = media.GetNamedString("display_url")
            ExpandedURL = media.GetNamedString("expanded_url")
            ID = media.GetNamedNumber("id")
            Indices = (From e In media.GetNamedArray("indices") Select CType(e.GetNumber, Integer)).ToArray
            MediaURL = media.GetNamedString("media_url")
            Sizes = New Sizes(media.GetNamedObject("sizes"))
            Try
                SourceStatusID = media.GetNamedString("source_status_id_str")
            Catch e As System.Exception
                SourceStatusID = 0
            End Try
            Type = media.GetNamedString("type")
            URL = media.GetNamedString("url")
        End Sub
    End Class

    Public Class URL
        Public Property DisplayURL As String
        Public Property ExpandedURL As String
        Public Property Indices As Integer()
        Public Property URL As String

        Public Sub New(ByVal url As JsonObject)
            DisplayURL = url.GetNamedString("display_url")
            ExpandedURL = url.GetNamedString("expanded_url")
            Indices = (From e In url.GetNamedArray("indices") Select CType(e.GetNumber, Integer)).ToArray
            Me.URL = url.GetNamedString("url")
        End Sub
    End Class

    Public Class UserMention
        Public Property ID As ULong
        Public Property Indices As Integer()
        Public Property Name As String
        Public Property ScreenName As String

        Public Sub New(ByVal usermention As JsonObject)
            ID = usermention.GetNamedString("id_str")
            Indices = (From e In usermention.GetNamedArray("indices") Select CType(e.GetNumber, Integer)).ToArray
            Name = usermention.GetNamedString("name")
            ScreenName = usermention.GetNamedString("screen_name")
        End Sub
    End Class

    Public Class Entities
        Public Property Hashtags As Hashtag()
        Public Property Media As Media()
        Public Property URLs As URL()
        Public Property UserMentions As UserMention()

        Public Sub New(ByVal entities As JsonObject)
            If entities.ContainsKey("hashtags") Then
                Hashtags = (From h In entities.GetNamedArray("hashtags") Select New Hashtag(h.GetObject)).ToArray
            End If
            If entities.ContainsKey("media") Then
                Media = (From m In entities.GetNamedArray("media") Select New Media(m.GetObject)).ToArray
            End If
            If entities.ContainsKey("urls") Then
                URLs = (From u In entities.GetNamedArray("urls") Select New URL(u.GetObject)).ToArray
            End If
            If entities.ContainsKey("user_mentions") Then
                UserMentions = (From u In entities.GetNamedArray("user_mentions") Select New UserMention(u.GetObject)).ToArray
            End If
        End Sub
    End Class

    Public Class Tweet
        Inherits StreamMessage
        Public Property CreatedAt As DateTime
        Public Property Entities As Entities
        Public Property FavoriteCount As ULong
        Public Property Favorited As Boolean
        Public Property ID As ULong
        Public Property In_Reply_To_ScreenName As String
        Public Property In_Reply_To_StatusID As ULong
        Public Property In_Reply_To_UserID As ULong
        Public Property RetweetCount As ULong
        Public Property Retweeted As Boolean
        Public Property RetweetedStatus As Tweet
        Public Property Source As String
        Public Property Text As String
        Public Property User As User

        Public Overrides ReadOnly Property Type As MessageType
            Get
                Return MessageType.Tweet
            End Get
        End Property

        Public Sub New(ByVal tweet As JsonObject)
            CreatedAt = GetDateFromAtString(tweet.GetNamedString("created_at"))
            Entities = New Entities(tweet.GetNamedObject("entities"))
            FavoriteCount = tweet.GetNamedNumber("favorite_count")
            Favorited = tweet.GetNamedBoolean("favorited")
            ID = tweet.GetNamedString("id_str")
            If tweet.GetNamedValue("in_reply_to_screen_name").ValueType <> JsonValueType.Null Then
                In_Reply_To_ScreenName = tweet.GetNamedString("in_reply_to_screen_name")
                In_Reply_To_UserID = tweet.GetNamedString("in_reply_to_user_id_str")
            End If
            If tweet.GetNamedValue("in_reply_to_status_id").ValueType <> JsonValueType.Null Then
                In_Reply_To_StatusID = tweet.GetNamedString("in_reply_to_status_id_str")
            End If
            RetweetCount = tweet.GetNamedNumber("retweet_count")
            Retweeted = tweet.GetNamedBoolean("retweeted")
            If tweet.ContainsKey("retweeted_status") Then
                RetweetedStatus = New Tweet(tweet.GetNamedObject("retweeted_status"))
            End If
            Source = GetClientNameFromSource(tweet.GetNamedString("source"))
            Text = tweet.GetNamedString("text")
            User = New User(tweet.GetNamedObject("user"))
        End Sub

        Public Sub New(ByRef t As Tweet)
            CreatedAt = t.CreatedAt
            Entities = t.Entities
            FavoriteCount = t.FavoriteCount
            Favorited = t.Favorited
            ID = t.ID
            In_Reply_To_ScreenName = t.In_Reply_To_ScreenName
            In_Reply_To_StatusID = t.In_Reply_To_StatusID
            In_Reply_To_UserID = t.In_Reply_To_UserID
            RetweetCount = t.RetweetCount
            Retweeted = t.Retweeted
            RetweetedStatus = t.RetweetedStatus
            Source = t.Source
            Text = t.Text
            User = t.User
        End Sub

        Private Shared Function GetClientNameFromSource(ByVal src As String) As String
            If src = "web" Then
                Return src
            Else
                Dim rx As New System.Text.RegularExpressions.Regex("<.*?>(?<cname>.*?)</a>")
                Dim mt = rx.Match(src)
                Return mt.Groups("cname").Value
            End If
        End Function



    End Class

    Public Class User
        Public Property Description As String
        Public Property Entities As Entities
        Public Property Name As String
        Public Property ScreenName As String
        Public Property IsProtected As Boolean
        Public Property URL As String
        Public Property ProfileImageURL As String
        Public Property FavoritesCount As ULong
        Public Property Listed As ULong
        Public Property Followers As ULong
        Public Property Followings As ULong
        Public Property Statuses As ULong
        Public Property Verified As Boolean
        Public Property ID As ULong

        Public Sub New(ByVal user As JsonObject)
            If user.GetNamedValue("description").ValueType <> JsonValueType.Null Then
                Description = user.GetNamedString("description")
            End If
            If user.ContainsKey("entities") Then
                Entities = New Entities(user.GetNamedObject("entities"))
            End If
            FavoritesCount = user.GetNamedNumber("favourites_count") ' On "user", this is correct. :(
            Followings = user.GetNamedNumber("friends_count")
            Followers = user.GetNamedNumber("followers_count")
            ID = user.GetNamedString("id_str")
            Listed = user.GetNamedNumber("listed_count")
            Name = user.GetNamedString("name")
            ProfileImageURL = user.GetNamedString("profile_image_url")
            IsProtected = user.GetNamedBoolean("protected")
            ScreenName = user.GetNamedString("screen_name")
            Statuses = user.GetNamedNumber("statuses_count")
            If user.GetNamedValue("url").ValueType <> JsonValueType.Null Then
                URL = user.GetNamedString("url")
            End If
            Verified = user.GetNamedBoolean("verified")
        End Sub
    End Class

    Public Class TimeZone
        Public Property Name As String
        Public Property TZInfoName As String
        Public Property UTCOffset As Integer
        Public Sub New(ByVal tz As JsonObject)
            Name = tz.GetNamedString("name")
            TZInfoName = tz.GetNamedString("tzinfo_name")
            UTCOffset = tz.GetNamedNumber("utc_offset")
        End Sub
    End Class

    Public Class PlaceType
        Public Property Code As Integer
        Public Property Name As String
        Public Sub New(ByVal pt As JsonObject)
            Code = pt.GetNamedNumber("code")
            Name = pt.GetNamedString("name")
        End Sub
    End Class

    Public Class Place
        Public Property Country As String
        Public Property CountryCode As String
        Public Property Name As String
        Public Property ParentID As Integer
        Public Property PlaceType As PlaceType
        Public Property URL As String
        Public Property WoeID As Integer
        Public Sub New(ByVal tl As JsonObject)
            Country = tl("country").GetString
            CountryCode = tl("countryCode").GetString()
            Name = tl("name").GetString()
            ParentID = tl("parentid").GetNumber()
            PlaceType = New PlaceType(tl("placeType").GetObject)
            URL = tl("url").GetString()
            WoeID = tl("woeid").GetNumber()
        End Sub
    End Class

    Public Class SleepTime
        Public Property Enabled As Boolean
        ' Public Property EndTime As ???
        ' Public Property StartTime As ???
        Public Sub New(ByVal st As JsonObject)
            Enabled = st("enabled").GetBoolean
        End Sub
    End Class

    Public Class AccountSettings
        Public Property AlwaysUseHTTPS As Boolean
        Public Property DiscoverableByEMail As Boolean
        Public Property GeoEnabled As Boolean
        Public Property Language As String
        Public Property [Protected] As Boolean
        Public Property ScreenName As String
        Public Property ShowAllInlineMedia As Boolean
        Public Property SleepTime As SleepTime
        Public Property TimeZone As TimeZone
        Public Property TrendLocation As Place()
        Public Property UseCookiePersonalization As Boolean
        Public Sub New(ByVal a As JsonObject)
            AlwaysUseHTTPS = a("always_use_https").GetBoolean()
            DiscoverableByEMail = a("discoverable_by_email").GetBoolean
            GeoEnabled = a("geo_enabled").GetBoolean
            Language = a("language").GetString
            [Protected] = a("protected").GetBoolean()
            ScreenName = a("screen_name").GetString
            'ShowAllInlineMedia = a("show_all_inline_media").GetBoolean
            SleepTime = New SleepTime(a("sleep_time").GetObject)
            TimeZone = New TimeZone(a("time_zone").GetObject)
            TrendLocation = (From p In a("trend_location").GetArray Select New Place(p.GetObject)).ToArray
            UseCookiePersonalization = a("use_cookie_personalization").GetBoolean
        End Sub
    End Class

    Private Shared Function GetDateFromAtString(ByVal at As String) As DateTime
        Return DateTime.ParseExact(at, "ddd MMM d HH':'mm':'ss zzz yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None)
    End Function

    Public Function GetAuthURL() As String
        RequestKey = OAuth.GetRequestToken(ConsumerKey)
        Return "https://api.twitter.com/oauth/authorize?oauth_token=" + RequestKey.Key
    End Function

    Public Sub GetAccessToken(ByVal pin As String)
        AccessKey = OAuth.GetAccessTokenWithPIN(ConsumerKey, RequestKey, pin)
    End Sub

    Public Sub UpdateStatus(ByVal status As String, Optional in_reply_to_statusid As ULong = 0)
        Dim method As String = "POST"
        Dim url As New Uri("https://api.twitter.com/1.1/statuses/update.json")
        Dim values As New TwitterAPIValues(ConsumerKey, method, url) With {.Token = AccessKey, .Status = OAuth.UrlEncode(status)}
        If in_reply_to_statusid <> 0 Then
            values.In_Reply_To_Status_Id = in_reply_to_statusid
        End If
        Dim content = System.Text.Encoding.UTF8.GetBytes(values.GetQueryString)
        Dim hreq = HttpWebRequest.CreateHttp(url)
        hreq.Method = "POST"
        hreq.ContentType = "application/x-www-form-urlencoded"
        Using reqstrm = hreq.GetRequestStream()
            reqstrm.Write(content, 0, content.Length)
        End Using
        Try
            Using hres = hreq.GetResponse()
                Dim sr As New IO.StreamReader(hres.GetResponseStream)
                Dim result = sr.ReadToEnd()
            End Using
        Catch ex As WebException
            Using res = ex.Response
                Dim strm = res.GetResponseStream()
                Dim rsr As New StreamReader(strm)
                Debug.WriteLine(rsr.ReadToEnd())
            End Using
            Throw
        End Try
    End Sub

    Public Function UpdateStatusWithMedia(ByVal status As String, ByVal image As Windows.Storage.Streams.RandomAccessStream) As Task
        Throw New NotImplementedException
    End Function

    Public Function GetHomeTimeline(Optional count As UInteger = 0, Optional since_id As UInteger = 0) As Tweet()
        Dim method As String = "GET"
        Dim url As New Uri("https://api.twitter.com/1.1/statuses/home_timeline.json")
        Dim values As New TwitterAPIValues(ConsumerKey, method, url) With {.Token = AccessKey}
        If count <> 0 Then
            values.Count = count
        End If
        If since_id <> 0 Then
            values.SinceID = since_id
        End If
        Dim hreq = HttpWebRequest.CreateHttp(url.OriginalString + "?" + values.GetQueryString)
        Using hres = hreq.GetResponse
            Dim sr As New IO.StreamReader(hres.GetResponseStream())
            Dim result = sr.ReadToEnd()

            Dim jarr As JsonArray = JsonArray.Parse(result)
            Return (From t In jarr Select New Tweet(t.GetObject)).ToArray
        End Using
    End Function

    Private Function GetUserStream() As IO.Stream
        Try
            Dim url As New Uri("https://userstream.twitter.com/1.1/user.json")
            Dim method As String = "POST"
            Dim values As New TwitterAPIValues(ConsumerKey, method, url) With {.Token = AccessKey}
            Dim hreq = HttpWebRequest.CreateHttp(url)
            hreq.Method = method
            hreq.ContentType = "application/x-www-form-urlencoded"
            Dim content = System.Text.Encoding.UTF8.GetBytes(values.GetQueryString())
            Using reqstrm = hreq.GetRequestStream()
                reqstrm.Write(content, 0, content.Length)
            End Using
            Dim hres = hreq.GetResponse()
            Return hres.GetResponseStream()
        Catch ex As WebException
            Return Nothing
        End Try
    End Function

    Public Sub ConnectUserStream(ByVal act As Action(Of StreamMessage), Optional ByRef running As Threading.CancellationToken = Nothing)

        Dim strm = GetUserStream()
        If strm Is Nothing Then
            Return
        End If

        Dim cs_t = strm.GetType()
        Dim connection_i As PropertyInfo = Nothing
        For Each p In cs_t.GetRuntimeProperties()
            If p.Name = "Connection" Then
                connection_i = p
            End If
        Next
        Dim connection = connection_i.GetValue(strm)
        Dim connection_t = connection.GetType()
        Dim canread_i As PropertyInfo = Nothing
        For Each p In connection_t.GetRuntimeProperties()
            If p.Name = "CanRead" Then
                canread_i = p
            End If
        Next

        Dim buffer(1023) As Byte
        Dim strbuilder As New Text.StringBuilder

        While strm.CanRead AndAlso DirectCast(canread_i.GetValue(connection), Boolean)
            Try
                Dim t = strm.ReadAsync(buffer, 0, 1024)
                Try
                    t.Wait(running)
                Catch e As OperationCanceledException
                    strm.Close()
                    Return
                End Try

                Dim temp As String = System.Text.Encoding.UTF8.GetChars(buffer, 0, t.Result)
                strbuilder.Append(temp)
                While strbuilder.ToString.Contains(vbCrLf)
                    Dim pos As Integer = strbuilder.ToString.IndexOf(vbLf)
                    Dim msg As String = strbuilder.ToString.Substring(0, pos + 1)

                    If Not String.IsNullOrEmpty(msg) AndAlso Not String.IsNullOrWhiteSpace(msg) Then
                        Debug.WriteLine(strbuilder.ToString())
                        Dim jobj = JsonObject.Parse(strbuilder.ToString)
                        If jobj.ContainsKey("text") Then
                            Dim tw As New Tweet(jobj)
                            act(tw)
                        ElseIf jobj.ContainsKey("delete") Then
                            Dim d As New Delete(jobj.GetNamedObject("delete"))
                            act(d)
                        ElseIf jobj.ContainsKey("disconnect") Then
                            Dim d As New Disconnect(jobj.GetNamedObject("disconnect"))
                            act(d)
                        ElseIf jobj.ContainsKey("warning") Then
                            Dim w As New Warning(jobj.GetNamedObject("warning"))
                            act(w)
                        ElseIf jobj.ContainsKey("friends") Or jobj.ContainsKey("friends_str") Then
                            Dim f As New FriendsList(jobj)
                            act(f)
                        ElseIf jobj.ContainsKey("event") Then
                            Dim e As New [Event](jobj)
                            act(e)
                        Else
                            Dim u As New UnknownMessage
                            act(u)
                        End If
                    End If
                    strbuilder.Remove(0, pos + 1)
                End While
            Catch e As IO.IOException
                Throw
            Catch e As WebException
                Return
            End Try
        End While
    End Sub

    Public Function CreateFavorite(ByVal id As ULong) As Tweet
        Dim url As New Uri("https://api.twitter.com/1.1/favorites/create.json")
        Dim method As String = "POST"
        Dim values As New TwitterAPIValues(ConsumerKey, method, url) With {.Token = AccessKey, .ID = id}
        Dim hreq = HttpWebRequest.CreateHttp(url)
        hreq.ContentType = "application/x-www-form-urlencoded"
        hreq.Method = method
        Dim reqstrm_task = hreq.GetRequestStream()
        Dim content = System.Text.Encoding.UTF8.GetBytes(values.GetQueryString())
        Using reqstrm = reqstrm_task
            reqstrm.Write(content, 0, content.Length)
        End Using
        Dim hres = hreq.GetResponse()
        Using strm = hres.GetResponseStream()
            Dim sr As New StreamReader(strm)
            Dim result = sr.ReadToEnd()
            Return New Tweet(JsonObject.Parse(result))
        End Using
    End Function

    Public Function DestroyFavorite(ByVal id As ULong) As Tweet
        Dim url As New Uri("https://api.twitter.com/1.1/favorites/destroy.json")
        Dim method As String = "POST"
        Dim values As New TwitterAPIValues(ConsumerKey, method, url) With {.Token = AccessKey, .ID = id}
        Dim hreq = HttpWebRequest.CreateHttp(url)
        hreq.ContentType = "application/x-www-form-urlencoded"
        hreq.Method = method
        Dim reqstrm_task = hreq.GetRequestStream()
        Dim content = System.Text.Encoding.UTF8.GetBytes(values.GetQueryString())
        Using reqstrm = reqstrm_task
            reqstrm.Write(content, 0, content.Length)
        End Using
        Dim hres = hreq.GetResponse()
        Using strm = hres.GetResponseStream()
            Dim sr As New StreamReader(strm)
            Dim result = sr.ReadToEnd()
            Return New Tweet(JsonObject.Parse(result))
        End Using
    End Function

    Public Function Retweet(ByVal id As ULong) As Tweet
        Dim url As New Uri("https://api.twitter.com/1.1/statuses/retweet/" & id & ".json")
        Dim method As String = "POST"
        Dim values As New TwitterAPIValues(ConsumerKey, method, url) With {.Token = AccessKey}
        Dim hreq = HttpWebRequest.CreateHttp(url)
        hreq.ContentType = "application/x-www-form-urlencoded"
        Dim reqstrm_task = hreq.GetRequestStream()
        hreq.Method = method
        Dim content = System.Text.Encoding.UTF8.GetBytes(values.GetQueryString())
        Using reqstrm = reqstrm_task
            reqstrm.Write(content, 0, content.Length)
        End Using
        Dim hres = hreq.GetResponse()
        Using strm = hres.GetResponseStream()
            Dim sr As New StreamReader(strm)
            Dim result = sr.ReadToEnd()
            Return New Tweet(JsonObject.Parse(result))
        End Using
    End Function

    Public Function ShowTweet(ByVal id As ULong) As Tweet
        Dim url As New Uri("https://api.twitter.com/1.1/statuses/show.json")
        Dim method As String = "GET"
        Dim values As New TwitterAPIValues(ConsumerKey, method, url) With {.Token = AccessKey}
        values.ID = id
        Dim hreq = HttpWebRequest.CreateHttp(url.OriginalString + "?" + values.GetQueryString)
        Using hres = hreq.GetResponse
            Dim sr As New IO.StreamReader(hres.GetResponseStream())
            Dim result = sr.ReadToEnd
            Return New Tweet(JsonObject.Parse(result))
        End Using
    End Function

    Public Function GetAccountSettings() As AccountSettings
        Dim url As New Uri("https://api.twitter.com/1.1/account/settings.json")
        Dim method As String = "GET"
        Dim values As New TwitterAPIValues(ConsumerKey, method, url) With {.Token = AccessKey}
        Dim hreq = HttpWebRequest.CreateHttp(url.OriginalString + "?" + values.GetQueryString)
        Using hres = hreq.GetResponse
            Dim sr As New IO.StreamReader(hres.GetResponseStream())
            Dim result = sr.ReadToEnd
            Return New AccountSettings(JsonObject.Parse(result))
        End Using
    End Function
End Class
