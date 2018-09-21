#!/usr/bin/env python

import sys

if len(sys.argv) < 3:
  print "Missing output name"
  exit(1)

outputname = sys.argv[1]
version = sys.argv[2]

f = open("git-log.txt", "r")
gitlog = f.read().replace("\n", "\n        ").strip()
f.close()

f = open("build/XRIT.nuspec.tpl", "r")
tpl = f.read()
f.close()

tpl = tpl.replace("|{|RELEASE|}|", gitlog)
tpl = tpl.replace("|{|VERSION|}|", version)

f = open(outputname, "w")
f.write(tpl)
f.close()