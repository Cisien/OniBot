$ErrorActionPreference = 'Stop';

$tag = $env:APPVEYOR_REPO_BRANCH
if(-not [System.String]::IsNullOrWhitespace($env:APPVEYOR_PULL_REQUEST_NUMBER)) {
	$tag = "$tag-pr-${$env:APPVEYOR_PULL_REQUEST_NUMBER}"
}

if([System.String]::IsNullOrWhitespace($tag)) {
    $tag = "untagged"
}
if (Enter-OncePerDeployment "install_docker_image")
{
	docker stop OniBot
	docker stop MeowBot
	docker rm OniBot
	docker rm MeowBot
	docker pull "cisien/onibot:$tag"
	docker run -d -e "Token=$env:OniBotToken" --name OniBot -v onibot:c:\app\config --restart=always "cisien/onibot:$tag"
	docker run -d -e "Token=$env:MeowBotToken" --name MeowBot -v meowbot:c:\app\config --restart=always "cisien/onibot:$tag"
}