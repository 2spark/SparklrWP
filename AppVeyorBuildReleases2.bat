echo Uploading artifact to Github...
curl -s -XPOST -H "Authorization:token 6540826d8a3b35ee133e148a1b2000502fd90e07" -H "Content-Type:application/octet-stream" -H "Content-Length:%size%" --data-binary @SparklrForWindowsPhone.xap https://uploads.github.com/repos/2spark/SparklrWP/releases/%id_release%/assets?name=SparklrForWindowsPhone.xap
echo Done. Enjoy your release :)
EXIT
