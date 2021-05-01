@echo off

set path=%CD%

cd "BombPeli\bin\Debug\net5.0-windows"

rem start BombPeli.exe "config.ini"
rem start BombPeli.exe "config2.ini"

start BombPeli.exe "config.ini"
start BombPeli.exe "config2.ini"

start BombPeli.exe "config3.ini"
start BombPeli.exe "config4.ini"

cd %path%