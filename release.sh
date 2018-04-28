#!/bin/sh
set -e

# this one is needed for adding xunit targets, msbuild /r doesn't do.
dotnet restore

msbuild /r /t:pack /p:Configuration=Release log4net.Ext.Json
msbuild /r /t:pack /p:Configuration=Release log4net.Ext.Json.Net
msbuild /r /t:build,test /p:Configuration=Release
