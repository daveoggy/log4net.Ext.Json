#!/bin/sh
msbuild /t:restore,build,pack /p:Configuration=Release
