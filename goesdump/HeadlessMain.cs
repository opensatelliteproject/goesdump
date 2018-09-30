using System;
using System.Threading;
using WebSocketSharp.Server;
using System.Net;
using System.Text;
using System.IO;
using WebSocketSharp;
using System.Collections.Generic;
using OpenSatelliteProject.Tools;
using OpenSatelliteProject.PacketData.Enums;
using OpenSatelliteProject.Log;
using System.Net.Sockets;

namespace OpenSatelliteProject {
    public class HeadlessMain {

        static readonly int MAX_CACHED_MESSAGES = 10;

        readonly ImageManager FDImageManager;
        readonly ImageManager XXImageManager;
        readonly ImageManager NHImageManager;
        readonly ImageManager SHImageManager;
        readonly ImageManager USImageManager;
        readonly ImageManager FMImageManager;
        readonly ImageManager UNKImageManager;

        DirectoryHandler directoryHandler;

        Mutex mtx;
        readonly Connector cn;
        DemuxManager demuxManager;
        Statistics_st statistics;
        StatisticsModel stModel;
        readonly HttpServer httpsv;

        static List<ConsoleMessage> messageList = new List<ConsoleMessage>();
        static Mutex messageListMutex = new Mutex();

        bool running;

        public static List<ConsoleMessage> GetCachedMessages {
            get {
                messageListMutex.WaitOne();
                var tmp = messageList.Clone();
                messageListMutex.ReleaseMutex();
                return tmp;
            }
        }

        static void ManageConfig() {
            // Check if we need to migrate from XML
            if (ConfigurationManager.Get ("migratedXML") == null) {
                // We need.
                UIConsole.Log("First run on SQLite mode. Migrating XML");
                var config = new XMLProgConfig ();
                ProgConfig.SetConfigDefaults ();
                ProgConfig.RecordIntermediateFile = config.RecordIntermediateFile;

                ProgConfig.ChannelDataServerName = config.ChannelDataServerName;
                ProgConfig.ChannelDataServerPort = config.ChannelDataServerPort;

                ProgConfig.ConstellationServerName = config.ConstellationServerName;
                ProgConfig.ConstellationServerPort = config.ConstellationServerPort;

                ProgConfig.StatisticsServerName = config.StatisticsServerName;
                ProgConfig.StatisticsServerPort = config.StatisticsServerPort;

                ProgConfig.EnableDCS = config.EnableDCS;
                ProgConfig.EnableEMWIN = config.EnableEMWIN;
                ProgConfig.EnableWeatherData = config.EnableWeatherData;

                ProgConfig.EraseFilesAfterGeneratingFalseColor = config.EraseFilesAfterGeneratingFalseColor;

                ProgConfig.GenerateFDFalseColor = config.GenerateFDFalseColor;
                ProgConfig.GenerateNHFalseColor = config.GenerateNHFalseColor;
                ProgConfig.GenerateSHFalseColor = config.GenerateSHFalseColor;
                ProgConfig.GenerateUSFalseColor = config.GenerateUSFalseColor;
                ProgConfig.GenerateXXFalseColor = config.GenerateXXFalseColor;

                ProgConfig.GenerateInfraredImages = config.GenerateInfraredImages;
                ProgConfig.GenerateVisibleImages = config.GenerateVisibleImages;
                ProgConfig.GenerateWaterVapourImages = config.GenerateWaterVapourImages;

                ProgConfig.MaxGenerateRetry = config.MaxGenerateRetry;
                ProgConfig.UseNOAAFormat = config.UseNOAAFormat;

                ProgConfig.TemporaryFileFolder = config.TemporaryFileFolder;
                ProgConfig.FinalFileFolder = config.FinalFileFolder;

                ProgConfig.SysLogServer = config.SysLogServer;
                ProgConfig.SysLogFacility = config.SysLogFacility;
                ProgConfig.HTTPPort = config.HTTPPort;

                ConfigurationManager.Set ("migratedXML", true);
            } else {
                ProgConfig.FillConfigDefaults ();
            }
        }

        void SetConfigVars() {
            UIConsole.Log ("Setting Configuration");
            FileHandler.SkipEMWIN = !ProgConfig.EnableEMWIN;
            FileHandler.SkipDCS = !ProgConfig.EnableDCS;
            FileHandler.SkipWeatherData = !ProgConfig.EnableWeatherData;

            if (ProgConfig.TemporaryFileFolder != null) {
                if (!LLTools.TestFolderAccess(ProgConfig.TemporaryFileFolder)) {
                    UIConsole.Error($"Cannot write file to Temporary Folder {ProgConfig.TemporaryFileFolder}");
                    throw new ApplicationException($"Cannot write file to Temporary Folder {ProgConfig.TemporaryFileFolder}");
                }
                FileHandler.TemporaryFileFolder = ProgConfig.TemporaryFileFolder;
            }

            if (ProgConfig.FinalFileFolder != null) {
                if (!LLTools.TestFolderAccess(ProgConfig.FinalFileFolder)) {
                    UIConsole.Error($"Cannot write file to Final Folder {ProgConfig.FinalFileFolder}");
                    throw new ApplicationException($"Cannot write file to Final Folder {ProgConfig.FinalFileFolder}");
                }
                FileHandler.FinalFileFolder = ProgConfig.FinalFileFolder;
            }

            if (ProgConfig.ArchiveFolder != null) {
                if (!LLTools.TestFolderAccess(ProgConfig.ArchiveFolder)) {
                    UIConsole.Error($"Cannot write file to Archive Folder {ProgConfig.ArchiveFolder}");
                    throw new ApplicationException($"Cannot write file to Archive Folder {ProgConfig.ArchiveFolder}");
                }
                FileHandler.ArchiveFolder = ProgConfig.ArchiveFolder;
            }

            ImageManager.EraseFiles = ProgConfig.EraseFilesAfterGeneratingFalseColor;
            ImageManager.GenerateInfrared = ProgConfig.GenerateInfraredImages;
            ImageManager.GenerateVisible = ProgConfig.GenerateVisibleImages;
            ImageManager.GenerateWaterVapour = ProgConfig.GenerateWaterVapourImages;
            ImageManager.MaxRetryCount = ProgConfig.MaxGenerateRetry;
            ImageManager.UseNOAAFileFormat = ProgConfig.UseNOAAFormat;
            ImageManager.GenerateLabels = ProgConfig.GenerateLabels;
            ImageManager.GenerateLatLonOverlays = ProgConfig.GenerateLatLonOverlays;
            ImageManager.GenerateMapOverlays = ProgConfig.GenerateMapOverlays;
            ImageManager.GenerateLatLonLabel = ProgConfig.GenerateLatLonLabel;
            ImageManager.EnableArchive = ProgConfig.EnableArchive;

            Connector.ChannelDataServerName = ProgConfig.ChannelDataServerName;
            Connector.StatisticsServerName = ProgConfig.StatisticsServerName;
            Connector.ConstellationServerName = ProgConfig.ConstellationServerName;

            Connector.ChannelDataServerPort = ProgConfig.ChannelDataServerPort;
            Connector.StatisticsServerPort = ProgConfig.StatisticsServerPort;
            Connector.ConstellationServerPort = ProgConfig.ConstellationServerPort;

            if (LLTools.IsLinux) {
                SyslogClient.SysLogServerIp = ProgConfig.SysLogServer;
                try {
                    SyslogClient.Send(ProgConfig.SysLogFacility, Level.Information, "Your syslog connection is working! OpenSatelliteProject is enabled to send logs.");
                } catch (SocketException) {
                    UIConsole.Warn("Your syslog is not enabled to receive UDP request. Please refer to https://opensatelliteproject.github.io/OpenSatelliteProject/");
                }
            }

            if (ProgConfig.CurveFilename != null) {
                Presets.LoadVisibleFalseColorCurve(ProgConfig.CurveFilename);
            }

            if (ProgConfig.LUTFilename != null) {
                Presets.LoadFalseColorLUT(ProgConfig.LUTFilename);
            }
        }

        public HeadlessMain() {
            AppDomain.CurrentDomain.UnhandledException += CrashReport.DefaultExceptionHandler;
            ManageConfig ();

            EventMaster.On(EventTypes.ConfigChangeEvent, d => {
                var data = (ConfigChangeEventData)d.Data;
                ProgConfig.UpdateProperty(data.Name, data.Value);
                EventMaster.Post("configSaved", data.Name);
                SetConfigVars();
            });

            SetConfigVars ();

            var fdFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_FULLDISK);
            var xxFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_AREA_OF_INTEREST);
            var nhFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_NORTHERN);
            var shFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_SOUTHERN);
            var usFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_UNITEDSTATES);
            var fmFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES16_ABI, (int)ScannerSubProduct.NONE);
            var unkFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.NONE); // Same for any unknown ABI

            FDImageManager = new ImageManager(fdFolder, "Full Disk");
            XXImageManager = new ImageManager(xxFolder, "Area of Interest");
            NHImageManager = new ImageManager(nhFolder, "Northern Hemisphere");
            SHImageManager = new ImageManager(shFolder, "Southern Hemisphere");
            USImageManager = new ImageManager(usFolder, "United States");
            FMImageManager = new ImageManager(fmFolder, "FM1");
            UNKImageManager = new ImageManager (unkFolder, "Unknown");

            FDImageManager.InitMapDrawer ();
            XXImageManager.InitMapDrawer ();
            NHImageManager.InitMapDrawer ();
            SHImageManager.InitMapDrawer ();
            USImageManager.InitMapDrawer ();
            FMImageManager.InitMapDrawer ();
            UNKImageManager.InitMapDrawer ();

            directoryHandler = new DirectoryHandler(FileHandler.FinalFileFolder, "/data");

            mtx = new Mutex();
            cn = new Connector();

            demuxManager = new DemuxManager {RecordToFile = ProgConfig.RecordIntermediateFile};
            cn.StatisticsAvailable += data => {
                mtx.WaitOne();
                statistics = data;
                mtx.ReleaseMutex();

                if (ProgConfig.SaveStatistics) {
                    ThreadPool.QueueUserWorkItem((a) => StatisticsManager.Update (new DBStatistics {
                        SCID = data.scid,
                        VCID = data.vcid,
                        PacketNumber = (long)data.packetNumber,
                        VitErrors = data.vitErrors,
                        FrameBits = data.frameBits,
                        RSErrors0 = data.rsErrors [0],
                        RSErrors1 = data.rsErrors [1],
                        RSErrors2 = data.rsErrors [2],
                        RSErrors3 = data.rsErrors [3],
                        SignalQuality = data.signalQuality,
                        SyncCorrelation = data.syncCorrelation,
                        PhaseCorrection = data.phaseCorrection,
                        LostPackets = (long)data.lostPackets,
                        AverageVitCorrections = data.averageVitCorrections,
                        AverageRSCorrections = data.averageRSCorrections,
                        DroppedPackets = (long)data.droppedPackets,
                        SyncWord =
                            $"{data.syncWord[0]:X02}{data.syncWord[1]:X02}{data.syncWord[2]:X02}{data.syncWord[3]:X02}",
                        FrameLock = data.frameLock > 0,
                    }));
                }

                stModel.Refresh(statistics);
                httpsv.WebSocketServices.Broadcast(stModel.toJSON());
            };

            cn.ChannelDataAvailable += demuxManager.parseBytes;
            cn.ConstellationDataAvailable += data => {
                var cm = new ConstellationModel (data);
                if (httpsv.IsListening) {
                    httpsv.WebSocketServices.Broadcast (cm.toJSON ());
                }
            };

            statistics = new Statistics_st();
            stModel = new StatisticsModel(statistics);
            UIConsole.Log("Headless Main Created");
            UIConsole.Log($"HTTP Server at port {ProgConfig.HTTPPort}");
            httpsv = new HttpServer(ProgConfig.HTTPPort) {RootPath = Path.GetFullPath(Path.Combine(".", "web"))};

            httpsv.OnGet += HandleHTTPGet;
            httpsv.AddWebSocketService("/mainws", () => new WSHandler {
                dh = directoryHandler
            });

            UIConsole.MessageAvailable += (data) => {
                var cm = new ConsoleModel(data.Priority.ToString(), data.Message);
                if (httpsv.IsListening) {
                    httpsv.WebSocketServices["/mainws"].Sessions.Broadcast(cm.toJSON());
                }

                messageListMutex.WaitOne();
                if (messageList.Count >= MAX_CACHED_MESSAGES) {
                    messageList.RemoveAt(0);
                }
                messageList.Add(data);
                messageListMutex.ReleaseMutex();
            };
        }

        private byte[] GetFile(string path) {
            // TODO: Make it better.
            var absPath = Path.GetFullPath (Path.Combine (httpsv.RootPath, "./" + path));
            if (!absPath.StartsWith(httpsv.RootPath) || !File.Exists(absPath)) {
                return null;
            }

            return File.ReadAllBytes (absPath);
        }

        private void HandleHTTPGet(object sender, HttpRequestEventArgs e) {
            var req = e.Request;
            var res = e.Response;

            var path = req.RawUrl;
            if (path.Contains ("?")) {
                path = path.Split(new char [] {'?'}, 2)[0];
            }

            if (path == "/") {
                path += "index.html";
            }

            // Workarround for the React Single Page issue
            if (
                path.StartsWith ("/console") ||
                path.StartsWith ("/explorer") ||
                path.StartsWith ("/charts") ||
                path.StartsWith ("/config")
            ) {
                path = "/index.html";
            }

            if (path.StartsWith(directoryHandler.BasePath)) {
                try {
                    directoryHandler.HandleAccess(httpsv, e);
                } catch (Exception ex) {
                    var output = $"Error reading file: {ex}";
                    res.StatusCode = (int)HttpStatusCode.InternalServerError;
                    res.WriteContent(Encoding.UTF8.GetBytes(output));
                }
                return;
            }

            var content = GetFile(path);
            if (content == null) {
                res.StatusCode = (int)HttpStatusCode.NotFound;
                const string res404 = "File not found";
                res.WriteContent(Encoding.UTF8.GetBytes(res404));
                return;
            }

            if (path.EndsWith (".html")) {
                res.ContentEncoding = Encoding.UTF8;
            } else if (path.EndsWith (".js")) {
                res.ContentEncoding = Encoding.UTF8;
            }

            res.ContentType = MimeTypes.GetMimeType (Path.GetExtension(path));

            res.WriteContent(content);
        }

        public void Start() {
            Console.CancelKeyPress += delegate {
                UIConsole.Log("Hit Ctrl + C! Closing...");
                running = false;
            };

            UIConsole.Log("Headless Main Starting");

            FDImageManager.Start();
            XXImageManager.Start();
            NHImageManager.Start();
            SHImageManager.Start();
            USImageManager.Start();
            FMImageManager.Start();
            UNKImageManager.Start ();

            cn.Start();
            httpsv.Start();
            running = true;

            while (running) {
                Thread.Sleep(10);
            }

            UIConsole.Log("Closing program...");
            cn.Stop();
            httpsv.Stop();

            FDImageManager.Stop();
            XXImageManager.Stop();
            NHImageManager.Stop();
            SHImageManager.Stop();
            USImageManager.Stop();
            FMImageManager.Stop();
            UNKImageManager.Stop ();
        }
    }
}

