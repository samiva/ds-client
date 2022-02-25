@echo off

set path=%CD%

cd "BombPeli\bin\Debug\net5.0-windows"
start BombPeli.exe "config1.ini"
start BombPeli.exe "config2.ini"
start BombPeli.exe "config3.ini"
start BombPeli.exe "config4.ini"

cd %path%