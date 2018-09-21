#!/bin/bash

msbuild /p:Configuration=Release
cp bin/Release/XRIT.dll build/lib/net45/
cp bin/Release/XRIT.dll build/lib/net452/
LIBVER=`monodis --assembly bin/Release/XRIT.dll |grep Version | cut -d: -f2 | sed -e 's/^[[:space:]]*//'`


echo "Current Version: ${LIBVER%.*}"
# sed "s/|{|VERSION|}|/${LIBVER%.*}/g" XRIT.nuspec.tpl > XRIT.nuspec
python update-tpl-release.py build/XRIT.nuspec "${LIBVER%.*}"

cd build
nuget pack XRIT.nuspec -verbosity detailed
