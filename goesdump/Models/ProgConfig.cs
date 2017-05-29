using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSatelliteProject {
    public static class ProgConfig {
        #region Database Keys
        #region Web Server
        public const string KeyHTTPPort = "HTTPPort";
        #endregion

        #region Folders
        public const string KeyTemporaryFileFolder = "TemporaryFileFolder";
        public const string KeyFinalFileFolder = "FinalFileFolder";
        #endregion

        #region Decoder Data
        public const string KeyRecordIntermediateFile = "RecordIntermediateFile";

        public const string KeyChannelDataServerName = "ChannelDataServerName";
        public const string KeyChannelDataServerPort = "ChannelDataServerPort";

        public const string KeyStatisticsServerPort = "StatisticsServerPort";
        public const string KeyStatisticsServerName = "StatisticsServerName";

        public const string KeyConstellationServerPort = "ConstellationServerPort";
        public const string KeyConstellationServerName = "ConstellationServerName";
        #endregion

        #region Image Processing
        public const string KeyGenerateVisibleImages = "GenerateVisibleImages";
        public const string KeyGenerateInfraredImages = "GenerateInfraredImages";
        public const string KeyGenerateWaterVapourImages = "GenerateWaterVapourImages";
        public const string KeyGenerateOtherImages = "GenerateOtherImages";

        public const string KeyGenerateFDFalseColor = "GenerateFDFalseColor";
        public const string KeyGenerateXXFalseColor = "GenerateXXFalseColor";
        public const string KeyGenerateNHFalseColor = "GenerateNHFalseColor";
        public const string KeyGenerateSHFalseColor = "GenerateSHFalseColor";
        public const string KeyGenerateUSFalseColor = "GenerateUSFalseColor";

        public const string KeyMaxGenerateRetry = "MaxGenerateRetry";

        public const string KeyEraseFilesAfterGeneratingFalseColor = "EraseFilesAfterGeneratingFalseColor";

        public const string KeyUseNOAAFormat = "UseNOAAFormat";
        #endregion

        #region Packet Processing
        public const string KeyEnableDCS = "EnableDCS";
        public const string KeyEnableEMWIN = "EnableEMWIN";
        public const string KeyEnableWeatherData = "EnableWeatherData";
        #endregion
        #endregion

        #region Properties
        #region Web Server
        [ConfigDescription("HTTP Listening Port (requires restart)", 8090)]
        public static int HTTPPort {
            get { return ConfigurationManager.GetInt (KeyHTTPPort); }
            set { ConfigurationManager.Set (KeyHTTPPort, value); }
        }
        #endregion

        #region Folders
        [ConfigDescription("Folder where temporary files will go (requires restart)", "tmp")]
        public static string TemporaryFileFolder {
            get { return ConfigurationManager.Get (KeyTemporaryFileFolder); }
            set { ConfigurationManager.Set(KeyTemporaryFileFolder, value); }
        }
        [ConfigDescription("Folder where final files will go (requires restart)", "output")]
        public static string FinalFileFolder {
            get { return ConfigurationManager.Get (KeyFinalFileFolder); }
            set { ConfigurationManager.Set(KeyFinalFileFolder, value); }
        }
        #endregion

        #region Decoder Data
        [ConfigDescription("Record an intermediate file that can be replay aftewards (used for debugging)", false)]
        public static bool RecordIntermediateFile {
            get { return ConfigurationManager.GetBool (KeyRecordIntermediateFile); }
            set { ConfigurationManager.Set(KeyRecordIntermediateFile, value); }
        }

        [ConfigDescription("Channel Data Server Hostname", "localhost")]
        public static string ChannelDataServerName {
            get { return ConfigurationManager.Get(KeyChannelDataServerName); }
            set { ConfigurationManager.Set(KeyChannelDataServerName, value); }
        }

        [ConfigDescription("Channel Data Server Port", 5001)]
        public static int ChannelDataServerPort {
            get { return ConfigurationManager.GetInt(KeyChannelDataServerPort); }
            set { ConfigurationManager.Set (KeyChannelDataServerPort, value); }
        }

        [ConfigDescription("Statistics Server Hostname", "localhost")]
        public static string StatisticsServerName {
            get { return ConfigurationManager.Get(KeyStatisticsServerName); }
            set { ConfigurationManager.Set(KeyStatisticsServerName, value); }
        }

        [ConfigDescription("Statistics Server Port", 5002)]
        public static int StatisticsServerPort {
            get { return ConfigurationManager.GetInt(KeyStatisticsServerPort); }
            set { ConfigurationManager.Set(KeyStatisticsServerPort, value); }
        }

        [ConfigDescription("Constellation Server Hostname", "localhost")]
        public static string ConstellationServerName {
            get { return ConfigurationManager.Get(KeyConstellationServerName); }
            set { ConfigurationManager.Set(KeyConstellationServerName, value); }
        }

        [ConfigDescription("Constellation Server Port", 9000)]
        public static int ConstellationServerPort {
            get { return ConfigurationManager.GetInt(KeyConstellationServerPort); }
            set { ConfigurationManager.Set(KeyConstellationServerPort, value); }
        }
        #endregion

        #region Image Processing

        [ConfigDescription("Generate Images for Visible Channel", false)]
        public static bool GenerateVisibleImages {
            get { return ConfigurationManager.GetBool(KeyGenerateVisibleImages); }
            set { ConfigurationManager.Set(KeyGenerateVisibleImages, value); }
        }

        [ConfigDescription("Generate Images for Infrared Channel", false)]
        public static bool GenerateInfraredImages {
            get { return ConfigurationManager.GetBool(KeyGenerateInfraredImages); }
            set { ConfigurationManager.Set(KeyGenerateInfraredImages, value); }
        }

        [ConfigDescription("Generate Images for Water Vapour Channel", false)]
        public static bool GenerateWaterVapourImages {
            get { return ConfigurationManager.GetBool(KeyGenerateWaterVapourImages); }
            set { ConfigurationManager.Set(KeyGenerateWaterVapourImages, value); }
        }

        [ConfigDescription("Generate images for other channels", false)]
        public static bool GenerateOtherImages {
            get { return ConfigurationManager.GetBool(KeyGenerateOtherImages); }
            set { ConfigurationManager.Set(KeyGenerateOtherImages, value); }
        }

        [ConfigDescription("Max number of retries if a problem occurs when generating images.", 3)]
        public static int MaxGenerateRetry {
            get { return ConfigurationManager.GetInt(KeyMaxGenerateRetry); }
            set { ConfigurationManager.Set(KeyMaxGenerateRetry, value); }
        }

        [ConfigDescription("Generate False Color Images for Full Disk", true)]
        public static bool GenerateFDFalseColor {
            get { return ConfigurationManager.GetBool(KeyGenerateFDFalseColor); }
            set { ConfigurationManager.Set(KeyGenerateFDFalseColor, value); }
        }

        [ConfigDescription("Generate False Color Images for Area of Interest", true)]
        public static bool GenerateXXFalseColor {
            get { return ConfigurationManager.GetBool(KeyGenerateXXFalseColor); }
            set { ConfigurationManager.Set(KeyGenerateXXFalseColor, value); }
        }

        [ConfigDescription("Generate False Color Images for Northern Hemisphere", true)]
        public static bool GenerateNHFalseColor {
            get { return ConfigurationManager.GetBool(KeyGenerateNHFalseColor); }
            set { ConfigurationManager.Set(KeyGenerateNHFalseColor, value); }
        }

        [ConfigDescription("Generate False Color Images for Southern Hemisphere", true)]
        public static bool GenerateSHFalseColor {
            get { return ConfigurationManager.GetBool(KeyGenerateSHFalseColor); }
            set { ConfigurationManager.Set(KeyGenerateSHFalseColor, value); }
        }

        [ConfigDescription("Generate False Color Images for United States", true)]
        public static bool GenerateUSFalseColor {
            get { return ConfigurationManager.GetBool(KeyGenerateUSFalseColor); }
            set { ConfigurationManager.Set(KeyGenerateUSFalseColor, value); }
        }

        [ConfigDescription("If .lrit files should be erased after generating requested images", false)]
        public static bool EraseFilesAfterGeneratingFalseColor {
            get { return ConfigurationManager.GetBool(KeyEraseFilesAfterGeneratingFalseColor); }
            set { ConfigurationManager.Set(KeyEraseFilesAfterGeneratingFalseColor, value); }
        }

        [ConfigDescription("If NOAA Filename format should be used instead OSP format.", false)]
        public static bool UseNOAAFormat {
            get { return ConfigurationManager.GetBool(KeyUseNOAAFormat); }
            set { ConfigurationManager.Set(KeyUseNOAAFormat, value); }
        }

        #endregion

        #region Packet Processing
        [ConfigDescription("If DCS files should be saved.", false)]
        public static bool EnableDCS {
            get { return ConfigurationManager.GetBool(KeyEnableDCS); }
            set { ConfigurationManager.Set(KeyEnableDCS, value); }
        }

        [ConfigDescription("If EMWIN files should be saved", false)]
        public static bool EnableEMWIN {
            get { return ConfigurationManager.GetBool(KeyEnableEMWIN); }
            set { ConfigurationManager.Set(KeyEnableEMWIN, value); }
        }

        [ConfigDescription("If Weather Data should be saved", true)]
        public static bool EnableWeatherData {
            get { return ConfigurationManager.GetBool(KeyEnableWeatherData); }
            set { ConfigurationManager.Set(KeyEnableWeatherData, value); }
        }
        #endregion

        #region Syslog Configuration
        [ConfigDescription("Syslog Server Hostname", "localhost")]
        public static string SysLogServer {
            get { return ConfigurationManager.Get(UIConsole.SYSLOGSERVERDBKEY); }
            set { ConfigurationManager.Set(UIConsole.SYSLOGSERVERDBKEY, value); }
        }

        [ConfigDescription("Syslog Facility", "LOG_USER")]
        public static string SysLogFacility {
            get { return ConfigurationManager.Get(UIConsole.SYSLOGFACILITYDBKEY); }
            set { ConfigurationManager.Set(UIConsole.SYSLOGFACILITYDBKEY, value); }
        }
        #endregion
        #endregion

        #region Auxiliary Methods
        /// <summary>
        /// Sets the configuration defaults.
        /// </summary>
        public static void SetConfigDefaults() {
            HTTPPort = (int) GetDefaultPropertyValue("HTTPPort");

            TemporaryFileFolder = (string) GetDefaultPropertyValue("TemporaryFileFolder");
            FinalFileFolder = (string) GetDefaultPropertyValue("FinalFileFolder");

            RecordIntermediateFile = (bool) GetDefaultPropertyValue("RecordIntermediateFile");

            ChannelDataServerName = (string) GetDefaultPropertyValue("ChannelDataServerName");
            ChannelDataServerPort = (int) GetDefaultPropertyValue("ChannelDataServerPort");

            StatisticsServerName = (string) GetDefaultPropertyValue("StatisticsServerName");
            StatisticsServerPort = (int) GetDefaultPropertyValue("StatisticsServerPort");

            ConstellationServerName = (string) GetDefaultPropertyValue("ConstellationServerName");
            ConstellationServerPort = (int) GetDefaultPropertyValue("ConstellationServerPort");

            GenerateVisibleImages = (bool) GetDefaultPropertyValue("GenerateVisibleImages");
            GenerateInfraredImages = (bool) GetDefaultPropertyValue("GenerateInfraredImages");
            GenerateWaterVapourImages = (bool) GetDefaultPropertyValue("GenerateWaterVapourImages");
            GenerateOtherImages = (bool) GetDefaultPropertyValue("GenerateOtherImages");

            GenerateFDFalseColor = (bool) GetDefaultPropertyValue("GenerateFDFalseColor");
            GenerateXXFalseColor = (bool) GetDefaultPropertyValue("GenerateXXFalseColor");
            GenerateNHFalseColor = (bool) GetDefaultPropertyValue("GenerateNHFalseColor");
            GenerateSHFalseColor = (bool) GetDefaultPropertyValue("GenerateSHFalseColor");
            GenerateUSFalseColor = (bool) GetDefaultPropertyValue("GenerateUSFalseColor");

            EraseFilesAfterGeneratingFalseColor = (bool) GetDefaultPropertyValue("EraseFilesAfterGeneratingFalseColor");

            UseNOAAFormat = (bool) GetDefaultPropertyValue("UseNOAAFormat");

            EnableDCS = (bool) GetDefaultPropertyValue("EnableDCS");
            EnableEMWIN = (bool) GetDefaultPropertyValue("EnableEMWIN");
            EnableWeatherData = (bool) GetDefaultPropertyValue("EnableWeatherData");

            SysLogServer = (string) GetDefaultPropertyValue("SysLogServer");
            SysLogFacility = (string) GetDefaultPropertyValue("SysLogFacility");

            MaxGenerateRetry = (int) GetDefaultPropertyValue("MaxGenerateRetry");
        }

        /// <summary>
        /// Sets configuration defaults for fields that doesn't exists on database
        /// </summary>
        public static void FillConfigDefaults() {
            HTTPPort = HTTPPort == 0 ? (int) GetDefaultPropertyValue("HTTPPort") : HTTPPort;

            TemporaryFileFolder = TemporaryFileFolder ?? (string) GetDefaultPropertyValue("TemporaryFileFolder");
            FinalFileFolder = FinalFileFolder ?? (string) GetDefaultPropertyValue("FinalFileFolder");

            RecordIntermediateFile = ConfigurationManager.Get (KeyRecordIntermediateFile) == null ? 
                (bool) GetDefaultPropertyValue("RecordIntermediateFile") : 
                RecordIntermediateFile;

            ChannelDataServerName = ChannelDataServerName ?? (string) GetDefaultPropertyValue("ChannelDataServerName");
            ChannelDataServerPort = ChannelDataServerPort == 0 ? 
                (int) GetDefaultPropertyValue("ChannelDataServerPort") : 
                ChannelDataServerPort;

            StatisticsServerName = StatisticsServerName ?? (string) GetDefaultPropertyValue("StatisticsServerName");
            StatisticsServerPort = StatisticsServerPort == 0 ? 
                (int) GetDefaultPropertyValue("StatisticsServerPort") : 
                StatisticsServerPort;

            ConstellationServerName = ConstellationServerName ?? (string) GetDefaultPropertyValue("ConstellationServerName");
            ConstellationServerPort = ConstellationServerPort == 0 ? 
                (int) GetDefaultPropertyValue("ConstellationServerPort") : 
                ConstellationServerPort;

            GenerateVisibleImages = ConfigurationManager.Get (KeyGenerateVisibleImages) == null ? 
                (bool) GetDefaultPropertyValue("GenerateVisibleImages") : 
                GenerateVisibleImages;
            GenerateInfraredImages = ConfigurationManager.Get (KeyGenerateInfraredImages) == null ?
                (bool) GetDefaultPropertyValue("GenerateInfraredImages"): 
                GenerateInfraredImages;
            GenerateWaterVapourImages = ConfigurationManager.Get (KeyGenerateWaterVapourImages) == null ?
                (bool) GetDefaultPropertyValue("GenerateWaterVapourImages") : 
                GenerateWaterVapourImages;
            GenerateOtherImages = ConfigurationManager.Get (KeyGenerateOtherImages) == null ? 
                (bool) GetDefaultPropertyValue("GenerateOtherImages") : 
                GenerateOtherImages;

            GenerateFDFalseColor = ConfigurationManager.Get (KeyGenerateFDFalseColor) == null ?
                (bool) GetDefaultPropertyValue("GenerateFDFalseColor") :
                GenerateFDFalseColor;
            GenerateXXFalseColor = ConfigurationManager.Get (KeyGenerateXXFalseColor) == null ? 
                (bool) GetDefaultPropertyValue("GenerateXXFalseColor") : 
                GenerateXXFalseColor;
            GenerateNHFalseColor = ConfigurationManager.Get (KeyGenerateNHFalseColor) == null ? 
                (bool) GetDefaultPropertyValue("GenerateNHFalseColor") : 
                GenerateNHFalseColor;
            GenerateSHFalseColor = ConfigurationManager.Get (KeyGenerateSHFalseColor) == null ?
                (bool) GetDefaultPropertyValue("GenerateSHFalseColor") :
                GenerateSHFalseColor;
            GenerateUSFalseColor = ConfigurationManager.Get (KeyGenerateUSFalseColor) == null ?
                (bool) GetDefaultPropertyValue("GenerateUSFalseColor") :
                GenerateUSFalseColor;

            EraseFilesAfterGeneratingFalseColor = ConfigurationManager.Get (KeyEraseFilesAfterGeneratingFalseColor) == null ?
                (bool) GetDefaultPropertyValue("EraseFilesAfterGeneratingFalseColor") :
                EraseFilesAfterGeneratingFalseColor;

            UseNOAAFormat = ConfigurationManager.Get (KeyUseNOAAFormat) == null ?
                (bool) GetDefaultPropertyValue("UseNOAAFormat") :
                UseNOAAFormat;

            EnableDCS = ConfigurationManager.Get (KeyEnableDCS) == null ?
                (bool) GetDefaultPropertyValue("EnableDCS") :
                EnableDCS;
            EnableEMWIN = ConfigurationManager.Get (KeyEnableEMWIN) == null ?
                (bool) GetDefaultPropertyValue("EnableEMWIN") :
                EnableEMWIN;
            EnableWeatherData = ConfigurationManager.Get (KeyEnableWeatherData) == null ?
                (bool) GetDefaultPropertyValue("EnableWeatherData") : 
                EnableWeatherData;

            SysLogServer = SysLogServer ?? (string) GetDefaultPropertyValue("SysLogServer");
            SysLogFacility = SysLogFacility ?? (string) GetDefaultPropertyValue("SysLogFacility");

            MaxGenerateRetry = MaxGenerateRetry == 0 ? 
                (int) GetDefaultPropertyValue("MaxGenerateRetry") : 
                MaxGenerateRetry;
        }

        public static void UpdateProperty(string name, string value) {
            var prop = typeof(ProgConfig).GetProperty (name);
            if (prop != null) {
                switch (prop.PropertyType.Name.ToLower()) {
                    case "string":
                        prop.SetValue (null, value);
                        break;
                    case "bool":
                    case "boolean":
                        prop.SetValue (null, value.ToLower() == "true");
                        break;
                    case "int":
                    case "int32":
                        try {
                            prop.SetValue (null, int.Parse (value));
                        } catch (Exception) {
                            UIConsole.Error ($"Cannot set config {name} to {value}: Failed to parse integer");
                        }
                        break;
                    case "float":
                        try {
                            prop.SetValue (null, float.Parse (value));
                        } catch (Exception) {
                            UIConsole.Error ($"Cannot set config {name} to {value}: Failed to parse float");
                        }
                        break;
                    case "double":
                        try {
                            prop.SetValue (null, double.Parse (value));
                        } catch (Exception) {
                            UIConsole.Error ($"Cannot set config {name} to {value}: Failed to parse double");
                        }
                        break;
                }
            }
        }

        public static object GetDefaultPropertyValue(string name) {
            object ret = null;
            var prop = typeof(ProgConfig).GetProperty (name);

            if (prop != null) {
                ConfigDescription cd = (ConfigDescription)Attribute.GetCustomAttribute (prop, typeof(ConfigDescription));
                if (cd != null) {
                    ret = cd.Default;
                }
            }

            return ret;
        }

        public static Dictionary<string, ConfigEntryInfo> GetConfig() {
            Dictionary<string, ConfigEntryInfo> config = new Dictionary<string, ConfigEntryInfo> ();

            var properties = typeof(ProgConfig).GetProperties ().ToArray();

            foreach (var prop in properties) {
                ConfigDescription cd = (ConfigDescription) Attribute.GetCustomAttribute (prop, typeof(ConfigDescription));
                config.Add (prop.Name, new ConfigEntryInfo {
                    Name = prop.Name,
                    Value = prop.GetValue(null),
                    Type = prop.PropertyType.Name,
                    Description = cd != null ? cd.Description : "",
                    DefaultValue = cd != null ? cd.Default : null,
                });
            }

            return config;
        }
        #endregion
    }
}

