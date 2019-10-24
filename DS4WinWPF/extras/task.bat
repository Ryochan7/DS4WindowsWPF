@echo off
SET mypath="%~dp0"
cmd.exe /c start "RunDS4Windows" %mypath%\DS4WinWPF.exe -m
exit
