#!/bin/bash

xbuild /p:Configuration=Release
cp bin/Release/XRIT.dll build/lib/net45/
LIBVER=`monodis --assembly bin/Release/XRIT.dll |grep Version | cut -d: -f2 | sed -e 's/^[[:space:]]*//'`
cd build

echo "Current Version: ${LIBVER}"
sed "s/|{|VERSION|}|/${LIBVER}-rc/g" XRIT.nuspec.tpl > XRIT-rc.nuspec
nuget pack XRIT-rc.nuspec
