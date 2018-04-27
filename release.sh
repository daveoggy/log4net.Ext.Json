#!/bin/sh
msbuild /t:restore,build,test,pack /p:Configuration=Release
