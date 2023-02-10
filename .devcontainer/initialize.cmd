@echo off

:: Copyright (c) 2023 Eliah Kagan
::
:: Permission to use, copy, modify, and/or distribute this software for any
:: purpose with or without fee is hereby granted.
::
:: THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
:: WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
:: MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
:: SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
:: WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION
:: OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN
:: CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

setlocal enableextensions

set "conf_path=.inherited-configuration"
set "script_uuid=7d63c880-c89c-4bc4-b440-57e134bf08d9"

set "script_name=%~nx0"
set "temp_path_prefix=%TEMP%\%script_uuid%-%RANDOM%"

goto begin

:msg
    echo/%script_name%: %* >&2
    exit /b

:delete_if_exist
    if exist "%*" (
        del "%*"
    )
    exit /b

:push_in
    setlocal
    set "name=%1"
    set "temp_path=%temp_path_prefix%-%name%.tmp"

    call :delete_if_exist %temp_path%

    git config -- "%name%" >"%temp_path%"
    if %ERRORLEVEL% NEQ 0 (
        call :msg skipping: %name%
        del "%temp_path%"
        exit /b 0
    )

    for /f "usebackq delims=" %%G in ("%temp_path%") do (
        set "value=%%G"
    )
    if %ERRORLEVEL% NEQ 0 (
        call :msg can't read temporary file: %temp_path%
        exit 1
    )

    del "%temp_path%"
    call :msg carrying in: %name%=%value%
    echo/%name% %value% >>"%conf_path%"
    exit /b

:push_all_in
    :: If the host clones the repo Windows-style, have the container play
    :: along.
    call :push_in core.autocrlf

    :: Usually these are automatically set in the container, but not always.
    call :push_in user.name
    call :push_in user.email

    exit /b

:begin
    call :delete_if_exist %conf_path%
    call :push_all_in
