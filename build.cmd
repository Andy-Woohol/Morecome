@echo off
cd /d "%~dp0"
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
set ICON=
if exist notepad.ico set ICON=/win32icon:notepad.ico /resource:notepad.ico,notepad.ico
"%CSC%" /nologo /target:winexe /optimize /codepage:65001 /out:Morecome.exe %ICON% /r:System.Windows.Forms.dll /r:System.Drawing.dll Morecome.cs
if errorlevel 1 (echo BUILD FAILED & exit /b 1)
echo BUILD OK: Morecome.exe
