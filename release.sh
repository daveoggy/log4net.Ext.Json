#!/bin/sh
dotnet restore
msbuild /t:restore,build,test,pack /p:Configuration=Release
