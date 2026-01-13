dotnet publish Astral_ServerChecker.csproj -c Release /p:PublishProfile=Publish_Win_x64.pubxml /p:PublishDir=./publish-artifacts/win-x64/
  
dotnet publish Astral_ServerChecker.csproj -c Release /p:PublishProfile=Publish_Win_Arm64.pubxml /p:PublishDir=./publish-artifacts/win-arm64/
  
dotnet publish Astral_ServerChecker.csproj -c Release /p:PublishProfile=Publish_macOS_Arm64.pubxml /p:PublishDir=./publish-artifacts/macos-arm64/
  
dotnet publish Astral_ServerChecker.csproj -c Release /p:PublishProfile=Publish_Linux_x64.pubxml /p:PublishDir=./publish-artifacts/linux-x64/
  
dotnet publish Astral_ServerChecker.csproj -c Release /p:PublishProfile=Publish_Linux_Arm64.pubxml /p:PublishDir=./publish-artifacts/linux-arm64/

pause