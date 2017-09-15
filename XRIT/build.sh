#!/bin/bash

msbuild /p:Configuration=Release
cp bin/Release/XRIT.dll build/lib/net45/
cd build
nuget pack XRIT.nuspec -verbosity detailed
