git tag %PLATFORM%_%APPVEYOR_BUILD_VERSION% 
git push https://6540826d8a3b35ee133e148a1b2000502fd90e07:@github.com/2spark/SparklrWP.git --tags
echo {"tag_name": "%PLATFORM%_%APPVEYOR_BUILD_VERSION%","target_commitish": "GC","name": "2spark v%APPVEYOR_BUILD_VERSION% for %PLATFORM% devices","body": "Release of 2spark app v%PLATFORM%_%APPVEYOR_BUILD_VERSION%\n %APPVEYOR_REPO_COMMIT_MESSAGE%","draft": false,"prerelease": true} > json.json
curl -XPOST -H 'Content-Type:application/json' -H 'Accept:application/json' --data-binary @json.json https://api.github.com/repos/2spark/SparklrWP/releases?access_token=6540826d8a3b35ee133e148a1b2000502fd90e07
del json.json