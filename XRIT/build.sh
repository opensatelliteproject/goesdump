#!/bin/bash

xbuild /p:Configuration=Release
cp bin/Release/XRIT.dll build/lib/net45/
cd build
nuget pack XRIT-nodep.nuspec -verbosity detailed
