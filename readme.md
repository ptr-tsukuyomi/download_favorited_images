download_favorited_images
=========================

This is the tool that download images you favorited on twitter.

This tool requires Json.NET (Newtonsoft.Json).

NOTICE
------

Twitter's Userstream has gone. So this tool does not work. 


How to use
----------

1. Create "settings.txt" in the directory that contains executable binary.
2. Write settings on "settings.txt" like this: key1=value1&key2=value2&...
3. Run executable binary and enjoy!

Settings
--------

You must set these parameter: ConsumerKey,ConsumerSecretKey

If you already have accesstokens, you can set these parameter: AccessKey,AccessSecretKey

If you want to change the directory where this program save images,you can set these parameger: Directory


Parameter "Directory" allows both absolute and relative path.

Relative path base is the directory that contains executable binary.

Change Log
----------

2016/08/28
Changed json library. (Windows.Data.Json -> Newtonsoft.Json)
Now you can run this in mono environment.
