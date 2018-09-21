<?xml version="1.0"?>
<package >
  <metadata>
    <id>OpenSatelliteProject.XRIT</id>
    <version>|{|VERSION|}|</version>
    <title>XRIT Library</title>
    <authors>Lucas Teske</authors>
    <owners>Lucas Teske</owners>
    <licenseUrl>https://github.com/opensatelliteproject/goesdump/blob/master/XRIT/LICENSE</licenseUrl>
    <projectUrl>https://github.com/opensatelliteproject/goesdump/</projectUrl>
    <iconUrl>https://github.com/opensatelliteproject/goesdump/raw/master/goesdump/icon.png</iconUrl>
    <requireLicenseAcceptance>true</requireLicenseAcceptance>
    <description>xRIT File / Packet Processing Library</description>
    <releaseNotes>
        |{|RELEASE|}|
    </releaseNotes>
    <copyright>Copyright 2018</copyright>
    <tags>GOES XRIT NOAA Satellite OpenSatelliteProject OSP SDR Ingestor</tags>
    <dependencies>
      <dependency id="DotSpatial.Data" version="[1.9]"/>
      <dependency id="DotSpatial.Mono" version="[1.9]" />
      <dependency id="DotSpatial.Projections" version="[1.9]"/>
      <dependency id="DotSpatial.Serialization" version="[1.9]"/>
      <dependency id="DotSpatial.Topology" version="[1.9]"/>
      <dependency id="SharpZipLib" version="[1.0.0]"/>
      <dependency id="Newtonsoft.Json" version="[11.0.2]"/>
    </dependencies>
  </metadata>
  <files>
     <file src="lib/**" target="lib" />
     <file src="build/**" target="build" />
  </files>
</package>
