@echo off

set path=%CD%

cd "BombPeli\bin\Debug\net5.0-windows"
<<<<<<< HEAD
rem start BombPeli.exe "config.ini"
rem start BombPeli.exe "config2.ini"
=======
start BombPeli.exe "config.ini"
start BombPeli.exe "config2.ini"
>>>>>>> 943b109507bae2ab37a656d2f26e6467c51e1b97
start BombPeli.exe "config3.ini"
start BombPeli.exe "config4.ini"

cd %path%