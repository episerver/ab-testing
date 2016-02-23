@echo off
setlocal

sqllocaldb stop v11.0
git clean -d -x -f
sqllocaldb start v11.0