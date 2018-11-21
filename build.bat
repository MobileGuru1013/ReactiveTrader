@echo off

%~dp0\tools\nuget restore %~dp0\src\ReactiveTrader.sln

call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" x86

C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe %~dp0\ReactiveTrader.proj -p:BUILD_VERSION=0.1.0.0 

pause