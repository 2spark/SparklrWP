echo Adding tag...
git tag %PLATFORM%_%APPVEYOR_BUILD_VERSION% 
echo Pushing tag...
git push https://6540826d8a3b35ee133e148a1b2000502fd90e07:@github.com/2spark/SparklrWP.git --tags
echo Creating release...
echo {"tag_name": "%PLATFORM%_%APPVEYOR_BUILD_VERSION%","target_commitish": "%APPVEYOR_REPO_BRANCH%","name": "2spark v%APPVEYOR_BUILD_VERSION% for %PLATFORM% devices","body": "### Commit by %APPVEYOR_REPO_COMMIT_AUTHOR% \n## Changes:\n%APPVEYOR_REPO_COMMIT_MESSAGE%\n%APPVEYOR_REPO_COMMIT_MESSAGE_EXTENDED%","draft": false,"prerelease": true} > json.json
curl -s -XPOST -H 'Content-Type:application/json' -H 'Accept:application/json' --data-binary @json.json https://api.github.com/repos/2spark/SparklrWP/releases?access_token=6540826d8a3b35ee133e148a1b2000502fd90e07 -o response.json
del json.json
echo Search the release id...
type response.json | findrepl id | findrepl /O:1:1 >> raw_id.txt
del response.json
echo Refining the id...
set /p raw_id_release=<raw_id.txt
set raw_id_release2=%raw_id_release:*"id": =%
set id_release=%raw_id_release2:,=%
echo The ID is %id_release% , yay!
del raw_id.txt
echo Moving the artifact...
move c:\projects\SparklrWP\SparklrForWindowsPhone\SparklrForWindowsPhone\Bin\%PLATFORM%\%CONFIGURATION%\SparklrForWindowsPhone_%CONFIGURATION%_%PLATFORM%.xap c:\projects\SparklrWP
rename c:\projects\SparklrWP\SparklrForWindowsPhone_%CONFIGURATION%_%PLATFORM%.xap SparklrForWindowsPhone.xap
echo Checking file size...
file_size.bat "c:\projects\SparklrWP\SparklrForWindowsPhone.xap"
