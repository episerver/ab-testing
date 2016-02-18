@echo off
setlocal

sqllocaldb stop v11.0
git clean -X -f -d
sqllocaldb start v11.0