@echo off

%~dp0\tools\nuget restore %~dp0\src\ReactiveTrader.sln

C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe %~dp0\ReactiveTrader.proj -p:BUILD_VERSION=0.1.0.0 /t:Package /toolsversion:4.0