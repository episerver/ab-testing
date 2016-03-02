@echo off
setlocal

ECHO.
ECHO Building Project.json for all the XProjs in the solution
ECHO.

CALL dnvm update-self
CALL dnvm install "1.0.0-rc1-update1" -runtime CLR -arch x86 -alias default
CALL dnvm use default

IF "%1"=="Debug" (set Configuration=Debug) ELSE (set Configuration=Release)
ECHO Building in %Configuration%

CALL dnu restore --quiet
CD ..
CALL dnu build **\project.json --quiet --configuration "%Configuration%" --out artifacts
IF %ERRORLEVEL% NEQ 0 (
		ECHO BUILD failed
		exit /b %ERRORLEVEL%
	)
	
CD build
ECHO Run Unit tests...
CALL tests

REM FOR /D /R %%D IN (..\test\*Test) do (
	REM pushd %%D
	REM CALL dnx --configuration "%Configuration%" test
	REM IF %ERRORLEVEL% NEQ 0 (
		REM ECHO Test failed
		REM exit /b %ERRORLEVEL%
	REM )
	REM popd
REM )

REM CALL run-specs %Configuration% -x Performance
REM CALL run-pester

IF %ERRORLEVEL% NEQ 0 (
	ECHO XPROJ Build failed
	exit /b %ERRORLEVEL%
)

ECHO Generate Nuget package "EPiServer.Marketing.Multivariate"
CALL generatepackages
IF %ERRORLEVEL% NEQ 0 (
	ECHO Nuget package creation failed for "EPiServer.Marketing.Multivariate" project
	exit /b %ERRORLEVEL%
)

ECHO Generate Nuget package "EPiServer.Marketing.Messaging"
CALL generatepackagesformessaging
IF %ERRORLEVEL% NEQ 0 (
	ECHO Nuget package creation failed for "EPiServer.Marketing.Messaging" project
	exit /b %ERRORLEVEL%
)

ECHO Generate Nuget package "EPiServer.Marketing.Multivariate.Testing"
CALL generatepackagesfortestpages
IF %ERRORLEVEL% NEQ 0 (
	ECHO Nuget package creation failed for "EPiServer.Marketing.Multivariate.Testing" project
	exit /b %ERRORLEVEL%
)


