@echo off
@REM utf8
chcp 65001

@del /s /q .\data\csharp\*.cs > nul
@del /s /q .\data\jscode\*.js > nul
@del /s /q .\data\json\*.bytes > nul

@call .\bin\Debug\net8.0\e2cs.exe -i "../../../" -o "../../../data" -f Config -l "propLanguage.xlsx"
@echo export excel succeed.

@echo ----------------------------------------------------------------
@echo copy *.cs start
@echo ----------------------------------------------------------------

@del /q /s ..\client\Assets\Scripts\Config\*.cs > nul
@xcopy .\data\csharp\*.cs ..\client\Assets\Scripts\Config\ /S /Y 

@echo ****************************************************************
@echo copy *.cs finished
@echo ****************************************************************

@echo ----------------------------------------------------------------
@echo copy *.bytes start
@echo ----------------------------------------------------------------

@del /q /s ..\client\Assets\Data\Jsons\Excels\*.bytes > nul
@xcopy .\data\json\*.bytes ..\client\Assets\Data\Jsons\Excels /S /Y 

@echo ****************************************************************
@echo copy *.bytes finished
@echo ****************************************************************

@echo ----------------------------------------------------------------
@echo copy *.js start
@echo ----------------------------------------------------------------

@del /q /s ..\server\http\server\resmng\config\*.js > nul
@xcopy .\data\jscode\*.js ..\server\http\server\resmng\config /S /Y 

@echo ****************************************************************
@echo copy *.js finished
@echo ****************************************************************

@echo copy succeed

@pause