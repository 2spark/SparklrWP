git tag %PLATFORM%_%APPVEYOR_BUILD_VERSION% 
git push https://6540826d8a3b35ee133e148a1b2000502fd90e07:@github.com/2spark/SparklrWP.git --tags
SET JSON='{"tag_name": "%PLATFORM%_%APPVEYOR_BUILD_VERSION%","target_commitish": "GC","name": "2spark v%APPVEYOR_BUILD_VERSION% for %PLATFORM% devices","body": "Release of 2spark app v%PLATFORM%_%APPVEYOR_BUILD_VERSION%\n %APPVEYOR_REPO_COMMIT_MESSAGE%","draft": false,"prerelease": true}'
SET 2SPARK_APP_PATH=SparklrForWindowsPhone\SparklrForWindowsPhone\Bin\%PLATFORM%\%CONFIGURATION%\SparklrForWindowsPhone_%CONFIGURATION%_%PLATFORM%.xap
SET GITHUB_URL_2SPARK=https://uploads.github.com/repos/2spark/SparklrWP/releases/477960/assets?name=SparklrForWindowsPhone_%CONFIGURATION%_%PLATFORM%.xap
curl --data %JSON% https://api.github.com/repos/2spark/SparklrWP/releases?access_token=6540826d8a3b35ee133e148a1b2000502fd90e07
curl -H "Authorization: token 6540826d8a3b35ee133e148a1b2000502fd90e07" \ -H "Accept: application/vnd.github.manifold-preview" \ -H "Content-Type: application/octet-stream" \ --data-binary %2SPARK_APP_PATH% \ %GITHUB_URL_2SPARK% 
PAUSE