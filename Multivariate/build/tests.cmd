FOR /D /R %%D IN (..\test\*Test) do (
  pushd %%D
	CALL dnx --configuration "%Configuration%" test
	 IF %ERRORLEVEL% NEQ 0 (
		 ECHO Test failed
		 exit /b %ERRORLEVEL%
	 )
	 popd
 )