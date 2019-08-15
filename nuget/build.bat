@setlocal
@echo off

REM This builds the ResgateIO Service NuGet package.

pushd ..\ResgateIO.Service

nuget >nul 2>&1
if errorlevel 9009 if not errorlevel 9010 (
	echo 'nuget.exe' is not in the path.
	goto End
)

if NOT EXIST bin\Release\netstandard2.0 (
	echo Cannot find .NET core build.
	goto End
)

dir bin\Release\netstandard2.0

nuget pack ResgateIO.Service.csproj -Symbols -SymbolPackageFormat snupkg -Properties Configuration=Release

move *.nupkg ..\nuget 1>NUL
move *.snupkg ..\nuget 1>NUL


:End

popd
