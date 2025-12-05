@echo off
setlocal

REM ==============================
REM  PARÁMETROS: REPO_URL y COMMIT_MSG
REM ==============================
REM Uso:
REM   subir_unity_github.bat https://github.com/tu_usuario/tu_repo.git "Mensaje de commit"

if "%~1"=="" (
    echo [ERROR] Debes pasar la URL del repositorio como parametro.
    echo.
    echo Uso:
    echo   %~nx0 https://github.com/tu_usuario/tu_repo.git "Mensaje de commit opcional"
    pause
    exit /b 1
)

set "REPO_URL=%~1"

if "%~2"=="" (
    set "COMMIT_MSG=Commit automatico del proyecto Unity"
) else (
    set "COMMIT_MSG=%~2"
)

REM ==============================
REM  COMPROBAR GIT
REM ==============================
git --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Git no esta instalado o no esta en el PATH.
    pause
    exit /b 1
)

REM Ir a la carpeta donde esta el script (raiz del proyecto Unity)
cd /d "%~dp0"

echo.
echo === Inicializando repositorio Git ===
if not exist ".git" (
    git init
)

REM ==============================
REM  CONFIGURAR GIT LFS
REM ==============================
echo.
echo === Configurando Git LFS (para archivos grandes) ===
git lfs version >nul 2>&1
if errorlevel 1 (
    echo [ADVERTENCIA] Git LFS no esta instalado. Los archivos grandes se subiran sin LFS.
) else (
    git lfs install

    REM Tipicos tipos de archivo grandes en Unity
    git lfs track "*.psd"
    git lfs track "*.tga"
    git lfs track "*.tif"
    git lfs track "*.wav"
    git lfs track "*.aif"
    git lfs track "*.aiff"
    git lfs track "*.fbx"
    git lfs track "*.mp4"
    git lfs track "*.mov"
)

REM ==============================
REM  .gitignore PARA UNITY
REM ==============================
echo.
echo === Creando .gitignore de Unity (si no existe) ===
if not exist ".gitignore" (
    echo # Unity.gitignore> .gitignore
    echo /[Ll]ibrary/>> .gitignore
    echo /[Tt]emp/>> .gitignore
    echo /[Oo]bj/>> .gitignore
    echo /[Bb]uild/>> .gitignore
    echo /[Bb]uilds/>> .gitignore
    echo /[Ll]ogs/>> .gitignore
    echo /[Uu]ser[Ss]ettings/>> .gitignore
    echo /.vs/>> .gitignore
    echo /*.csproj>> .gitignore
    echo /*.sln>> .gitignore
    echo /*.user>> .gitignore
)

REM ==============================
REM  AÑADIR Y HACER COMMIT
REM ==============================
echo.
echo === Añadiendo archivos al staging ===
git add .

REM Comprobar si hay algo que commitear
git diff --cached --quiet
if errorlevel 1 (
    echo.
    echo === Haciendo commit con mensaje: %COMMIT_MSG% ===
    git commit -m "%COMMIT_MSG%"
) else (
    echo No hay cambios nuevos para commitear.
)

REM ==============================
REM  REMOTO Y PUSH
REM ==============================
echo.
echo === Configurando remoto origin ===
git remote -v | find "origin" >nul
if errorlevel 1 (
    git remote add origin "%REPO_URL%"
)

echo.
echo === Asegurando rama main ===
git branch > tmp_branch.txt
findstr /R /C:"\* main" tmp_branch.txt >nul
if errorlevel 1 (
    git branch -M main
)
del tmp_branch.txt

echo.
echo === Haciendo push a GitHub (origin/main) ===
git push -u origin main

echo.
echo === PROCESO TERMINADO ===
pause
endlocal
