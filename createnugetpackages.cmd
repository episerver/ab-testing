@echo off

cd build
generatepackages.cmd && generatepackagesforkpi.cmd && generatepackagesformessaging.cmd && generatepackagesforkpicommerce.cmd && cd ..