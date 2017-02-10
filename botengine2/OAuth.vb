Imports System.Net

Public Class OAuth

	Structure KeyPair
		Dim Key As String
		Dim KeySecret As String
	End Structure

	Class OAuthValues
		Protected values As New SortedDictionary(Of String, String)
		Sub New(ByVal consumer As KeyPair, ByVal method As String, ByVal url As Uri)
			values("oauth_timestamp") = GetUNIXTime()
			values("oauth_nonce") = DateTime.Now.Millisecond
			values("oauth_signature_method") = "HMAC-SHA1"
			values("oauth_version") = "1.0"
			HTTPMethod = method
			AccessURL = url
			ConsumerKey = consumer
		End Sub

		Public Property HTTPMethod As String
		Public Property AccessURL As Uri

		Dim _consumerKey As KeyPair
		Public Property ConsumerKey As KeyPair
			Get
				Return _consumerKey
			End Get
			Set(value As KeyPair)
				_consumerKey = value
				values("oauth_consumer_key") = value.Key
			End Set
		End Property

		Dim _token As KeyPair
		Public Property Token As KeyPair
			Get
				Return _token
			End Get
			Set(value As KeyPair)
				_token = value
				values("oauth_token") = _token.Key
			End Set
		End Property

		Public Property Verifier As String
			Get
				Return values("oauth_verifier")
			End Get
			Set(value As String)
				values("oauth_verifier") = value
			End Set
		End Property

		Protected Sub ComputeOAuthSignature()
			Dim valuestring = String.Join("&", (From p In values Select p.Key + "=" + p.Value).ToArray)

			Dim encodedvalues = UrlEncode(valuestring)

			Dim target = System.String.Format("{0}&{1}&{2}", UrlEncode(HTTPMethod), UrlEncode(AccessURL.OriginalString), encodedvalues)
			Dim key = UrlEncode(_consumerKey.KeySecret) + "&" + If(values.ContainsKey("oauth_token"), UrlEncode(_token.KeySecret), "")

			values("oauth_signature") = UrlEncode(GetHMACSHA1Signature(key, target))
		End Sub

		Public Function GetAuthorizationString() As String
			ComputeOAuthSignature()
			Return String.Join(", ", (From p In values Select p.Key + "=""" + p.Value + """").ToArray())
		End Function
	End Class

	Public Shared Function GetHMACSHA1Signature(ByVal key As String, ByVal src As String) As String
		Dim hmac As New System.Security.Cryptography.HMACSHA1(System.Text.Encoding.UTF8.GetBytes(key))
		Return System.Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(src)))
	End Function

	Public Shared Function UrlEncode(ByVal str As String) As String
		Dim UnreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~"
		Dim sb As New Text.StringBuilder
		Dim bytes = System.Text.Encoding.UTF8.GetBytes(str)
		For Each b In bytes
			If UnreservedChars.IndexOf(System.Convert.ToChar(b)) <> -1 Then
				sb.Append(System.Convert.ToChar(b))
			Else
				sb.AppendFormat("%{0:X2}", b)
			End If
		Next
		Return sb.ToString()
	End Function

	Private Shared Function GetUNIXTime() As UInt64
		Dim elapsedtime As TimeSpan = DateTime.Now.ToUniversalTime() - New DateTime(1970, 1, 1, 0, 0, 0, 0)
		Return elapsedtime.TotalSeconds
	End Function

	Public Shared Function GetValueFromKeyValueString(ByVal kvs As String, ByVal key As String) As String
		Return (From p In (From sp In kvs.Split("&")) Where p.Split("=")(0) = key Select p.Split("=")(1)).First
	End Function

	Public Shared Function GetRequestToken(ByVal consumer As KeyPair) As KeyPair
		Dim url As New Uri("https://api.twitter.com/oauth/request_token")
		Dim method As String = "POST"
		Dim values As New OAuthValues(consumer, method, url)
		'Dim hreq = HttpWebRequest.CreateHttp(url)
		Dim hreq = HttpWebRequest.Create(url)
		hreq.Method = method
		hreq.Headers.Item("Authorization") = "OAuth " & values.GetAuthorizationString

		Using hres = hreq.GetResponse()
			Dim strm = hres.GetResponseStream()
			Dim sr As New IO.StreamReader(strm)
			Dim result = sr.ReadToEnd()
			Dim reqkey As OAuth.KeyPair
			reqkey.Key = GetValueFromKeyValueString(result, "oauth_token")
			reqkey.KeySecret = GetValueFromKeyValueString(result, "oauth_token_secret")
			Return reqkey
		End Using
	End Function

	Public Shared Function GetAccessTokenWithPIN(ByVal consumer As KeyPair, ByVal request As KeyPair, ByVal pin As String) As KeyPair
		Dim method As String = "POST"
		Dim url As New Uri("https://api.twitter.com/oauth/access_token")
		Dim values As New OAuthValues(consumer, method, url) With {.Token = request, .Verifier = pin}

		Dim hreq = Net.HttpWebRequest.Create(url)
		hreq.Method = "POST"
		hreq.Headers.Item("Authorization") = "OAuth " & values.GetAuthorizationString

		Using hres = hreq.GetResponse()
			Dim sr As New IO.StreamReader(hres.GetResponseStream())
			Dim result = sr.ReadToEnd()

			Dim accesskey As OAuth.KeyPair
			accesskey.Key = GetValueFromKeyValueString(result, "oauth_token")
			accesskey.KeySecret = GetValueFromKeyValueString(result, "oauth_token_secret")
			Return accesskey
		End Using
	End Function

End Class
