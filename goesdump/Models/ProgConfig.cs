using System;

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
        public static int HTTPPort {
            get { return ConfigurationManager.GetInt (KeyHTTPPort); }
            set { ConfigurationManager.Set (KeyHTTPPort, value); }
        }
        #endregion

        #region Folders
        public static string TemporaryFileFolder {
            get { return ConfigurationManager.Get (KeyTemporaryFileFolder); }
            set { ConfigurationManager.Set(KeyTemporaryFileFolder, value); }
        }
        public static string FinalFileFolder {
            get { return ConfigurationManager.Get (KeyFinalFileFolder); }
            set { ConfigurationManager.Set(KeyFinalFileFolder, value); }
        }
        #endregion

        #region Decoder Data

        public static bool RecordIntermediateFile {
            get { return ConfigurationManager.GetBool (KeyRecordIntermediateFile); }
            set { ConfigurationManager.Set(KeyRecordIntermediateFile, value); }
        }

        public static string ChannelDataServerName {
            get { return ConfigurationManager.Get(KeyChannelDataServerName); }
            set { ConfigurationManager.Set(KeyChannelDataServerName, value); }
        }
        public static int ChannelDataServerPort {
            get { return ConfigurationManager.GetInt(KeyChannelDataServerPort); }
            set { ConfigurationManager.Set (KeyChannelDataServerPort, value); }
        }

        public static string StatisticsServerName {
            get { return ConfigurationManager.Get(KeyStatisticsServerName); }
            set { ConfigurationManager.Set(KeyStatisticsServerName, value); }
        }
        public static int StatisticsServerPort {
            get { return ConfigurationManager.GetInt(KeyStatisticsServerPort); }
            set { ConfigurationManager.Set(KeyStatisticsServerPort, value); }
        }

        public static string ConstellationServerName {
            get { return ConfigurationManager.Get(KeyConstellationServerName); }
            set { ConfigurationManager.Set(KeyConstellationServerName, value); }
        }
        public static int ConstellationServerPort {
            get { return ConfigurationManager.GetInt(KeyConstellationServerPort); }
            set { ConfigurationManager.Set(KeyConstellationServerPort, value); }
        }
        #endregion

        #region Image Processing
        public static bool GenerateVisibleImages {
            get { return ConfigurationManager.GetBool(KeyGenerateVisibleImages); }
            set { ConfigurationManager.Set(KeyGenerateVisibleImages, value); }
        }
        public static bool GenerateInfraredImages {
            get { return ConfigurationManager.GetBool(KeyGenerateInfraredImages); }
            set { ConfigurationManager.Set(KeyGenerateInfraredImages, value); }
        }
        public static bool GenerateWaterVapourImages {
            get { return ConfigurationManager.GetBool(KeyGenerateWaterVapourImages); }
            set { ConfigurationManager.Set(KeyGenerateWaterVapourImages, value); }
        }
        public static bool GenerateOtherImages {
            get { return ConfigurationManager.GetBool(KeyGenerateOtherImages); }
            set { ConfigurationManager.Set(KeyGenerateOtherImages, value); }
        }
        public static int MaxGenerateRetry {
            get { return ConfigurationManager.GetInt(KeyMaxGenerateRetry); }
            set { ConfigurationManager.Set(KeyMaxGenerateRetry, value); }
        }
        public static bool GenerateFDFalseColor {
            get { return ConfigurationManager.GetBool(KeyGenerateFDFalseColor); }
            set { ConfigurationManager.Set(KeyGenerateFDFalseColor, value); }
        }
        public static bool GenerateXXFalseColor {
            get { return ConfigurationManager.GetBool(KeyGenerateXXFalseColor); }
            set { ConfigurationManager.Set(KeyGenerateXXFalseColor, value); }
        }

        public static bool GenerateNHFalseColor {
            get { return ConfigurationManager.GetBool(KeyGenerateNHFalseColor); }
            set { ConfigurationManager.Set(KeyGenerateNHFalseColor, value); }
        }

        public static bool GenerateSHFalseColor {
            get { return ConfigurationManager.GetBool(KeyGenerateSHFalseColor); }
            set { ConfigurationManager.Set(KeyGenerateSHFalseColor, value); }
        }

        public static bool GenerateUSFalseColor {
            get { return ConfigurationManager.GetBool(KeyGenerateUSFalseColor); }
            set { ConfigurationManager.Set(KeyGenerateUSFalseColor, value); }
        }

        public static bool EraseFilesAfterGeneratingFalseColor {
            get { return ConfigurationManager.GetBool(KeyEraseFilesAfterGeneratingFalseColor); }
            set { ConfigurationManager.Set(KeyEraseFilesAfterGeneratingFalseColor, value); }
        }

        public static bool UseNOAAFormat {
            get { return ConfigurationManager.GetBool(KeyUseNOAAFormat); }
            set { ConfigurationManager.Set(KeyUseNOAAFormat, value); }
        }

        #endregion

        #region Packet Processing
        public static bool EnableDCS {
            get { return ConfigurationManager.GetBool(KeyEnableDCS); }
            set { ConfigurationManager.Set(KeyEnableDCS, value); }
        }

        public static bool EnableEMWIN {
            get { return ConfigurationManager.GetBool(KeyEnableEMWIN); }
            set { ConfigurationManager.Set(KeyEnableEMWIN, value); }
        }

        public static bool EnableWeatherData {
            get { return ConfigurationManager.GetBool(KeyEnableWeatherData); }
            set { ConfigurationManager.Set(KeyEnableWeatherData, value); }
        }
        #endregion

        #region Syslog Configuration
        public static string SysLogServer {
            get { return ConfigurationManager.Get(UIConsole.SYSLOGSERVERDBKEY); }
            set { ConfigurationManager.Set(UIConsole.SYSLOGSERVERDBKEY, value); }
        }

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
            HTTPPort = 8090;

            TemporaryFileFolder = "tmp";
            FinalFileFolder = "output";

            RecordIntermediateFile = false;

            ChannelDataServerName = "localhost";
            ChannelDataServerPort = 5001;

            StatisticsServerName = "localhost";
            StatisticsServerPort = 5002;

            ConstellationServerName = "localhost";
            ConstellationServerPort = 9000;

            GenerateVisibleImages = false;
            GenerateInfraredImages = false;
            GenerateWaterVapourImages = false;
            GenerateOtherImages = false;

            GenerateFDFalseColor = true;
            GenerateXXFalseColor = true;
            GenerateNHFalseColor = true;
            GenerateSHFalseColor = true;
            GenerateUSFalseColor = true;

            EraseFilesAfterGeneratingFalseColor = false;

            UseNOAAFormat = false;

            EnableDCS = false;
            EnableEMWIN = false;
            EnableWeatherData = true;

            SysLogServer = "localhost";
            SysLogFacility = "LOG_USER";

            MaxGenerateRetry = 3;
        }

        /// <summary>
        /// Sets configuration defaults for fields that doesn't exists on database
        /// </summary>
        public static void FillConfigDefaults() {
            HTTPPort = HTTPPort == 0 ? 8090 : HTTPPort;

            TemporaryFileFolder = TemporaryFileFolder ?? "tmp";
            FinalFileFolder = FinalFileFolder ?? "output";

            RecordIntermediateFile = ConfigurationManager.Get (KeyRecordIntermediateFile) != null && RecordIntermediateFile;

            ChannelDataServerName = ChannelDataServerName ?? "localhost";
            ChannelDataServerPort = ChannelDataServerPort == 0 ? 5001 : ChannelDataServerPort;

            StatisticsServerName = StatisticsServerName ?? "localhost";
            StatisticsServerPort = StatisticsServerPort == 0 ? 5002 : StatisticsServerPort;

            ConstellationServerName = ConstellationServerName ?? "localhost";
            ConstellationServerPort = ConstellationServerPort == 0 ? 9000 : ConstellationServerPort;

            GenerateVisibleImages = ConfigurationManager.Get (KeyGenerateVisibleImages) != null && GenerateVisibleImages;
            GenerateInfraredImages = ConfigurationManager.Get (KeyGenerateInfraredImages) != null && GenerateInfraredImages;
            GenerateWaterVapourImages = ConfigurationManager.Get (KeyGenerateWaterVapourImages) != null && GenerateWaterVapourImages;
            GenerateOtherImages = ConfigurationManager.Get (KeyGenerateOtherImages) != null && GenerateOtherImages;

            GenerateFDFalseColor = ConfigurationManager.Get (KeyGenerateFDFalseColor) == null || GenerateFDFalseColor;
            GenerateXXFalseColor = ConfigurationManager.Get (KeyGenerateXXFalseColor) == null || GenerateXXFalseColor;
            GenerateNHFalseColor = ConfigurationManager.Get (KeyGenerateNHFalseColor) == null || GenerateNHFalseColor;
            GenerateSHFalseColor = ConfigurationManager.Get (KeyGenerateSHFalseColor) == null || GenerateSHFalseColor;
            GenerateUSFalseColor = ConfigurationManager.Get (KeyGenerateUSFalseColor) == null || GenerateUSFalseColor;

            EraseFilesAfterGeneratingFalseColor = ConfigurationManager.Get (KeyEraseFilesAfterGeneratingFalseColor) != null && EraseFilesAfterGeneratingFalseColor;

            UseNOAAFormat = ConfigurationManager.Get (KeyUseNOAAFormat) != null && UseNOAAFormat;

            EnableDCS = ConfigurationManager.Get (KeyEnableDCS) != null && EnableDCS;
            EnableEMWIN = ConfigurationManager.Get (KeyEnableEMWIN) != null && EnableEMWIN;
            EnableWeatherData = ConfigurationManager.Get (KeyEnableWeatherData) == null || EnableWeatherData;

            SysLogServer = SysLogServer ?? "localhost";
            SysLogFacility = SysLogFacility ?? "LOG_USER";

            MaxGenerateRetry = MaxGenerateRetry == 0 ? 3 : MaxGenerateRetry;
        }
        #endregion
    }
}

