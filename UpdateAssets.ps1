Push-Location pogo_assets
git fetch
git pull
$sha_assets = & dotnet dotnet-gitversion /output json /showvariable sha
Pop-Location
Push-Location game_masters
git fetch
git pull
$sha_masters = & dotnet dotnet-gitversion /output json /showvariable sha
Pop-Location