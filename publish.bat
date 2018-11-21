@echo off

%~dp0\tools\nuget restore %~dp0\src\ReactiveTrader.sln

echo %1

C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe %~dp0\ReactiveTrader.proj -p:BUILD_VERSION=0.3.0.0 /t:Publish /toolsversion:4.0 /p:VisualStudioVersion=12.0 "/p:BlobTargetKey=%1"