using System;
using System.Threading;
using WebSocketSharp.Server;
using System.Net;
using System.Text;
using System.IO;
using System.Collections.Concurrent;
using WebSocketSharp;
using System.Collections.Generic;
using System.Linq;
using OpenSatelliteProject.Tools;
using OpenSatelliteProject.PacketData.Enums;
using OpenSatelliteProject.Log;
using System.Net.Sockets;

namespace OpenSatelliteProject {
    public class HeadlessMain {

        private static readonly int MAX_CACHED_MESSAGES = 10;

        private static ProgConfig config = new ProgConfig();

        private ImageManager FDImageManager;
        private ImageManager XXImageManager;
        private ImageManager NHImageManager;
        private ImageManager SHImageManager;
        private ImageManager USImageManager;
        private ImageManager FMImageManager;

        private DirectoryHandler directoryHandler;

        private Mutex mtx;
        private Connector cn;
        private DemuxManager demuxManager;
        private Statistics_st statistics;
        private StatisticsModel stModel;
        private HttpServer httpsv;

        private static List<ConsoleMessage> messageList = new List<ConsoleMessage>();
        private static Mutex messageListMutex = new Mutex();

        private bool running = false;

        public static List<ConsoleMessage> GetCachedMessages {
            get {
                List<ConsoleMessage> tmp;
                messageListMutex.WaitOne();
                tmp = messageList.Clone<ConsoleMessage>();
                messageListMutex.ReleaseMutex();
                return tmp;
            }
        }

        public HeadlessMain() {

            #region Create Config File
            config.RecordIntermediateFile = config.RecordIntermediateFile;
            config.ChannelDataServerName = config.ChannelDataServerName;
            config.ChannelDataServerPort = config.ChannelDataServerPort;
            config.ConstellationServerName = config.ConstellationServerName;
            config.ConstellationServerPort = config.ConstellationServerPort;
            config.StatisticsServerName = config.StatisticsServerName;
            config.StatisticsServerPort = config.StatisticsServerPort;
            config.EnableDCS = config.EnableDCS;
            config.EnableEMWIN = config.EnableEMWIN;
            config.EraseFilesAfterGeneratingFalseColor = config.EraseFilesAfterGeneratingFalseColor;
            config.GenerateFDFalseColor = config.GenerateFDFalseColor;
            config.GenerateNHFalseColor = config.GenerateNHFalseColor;
            config.GenerateSHFalseColor = config.GenerateSHFalseColor;
            config.GenerateUSFalseColor = config.GenerateUSFalseColor;
            config.GenerateXXFalseColor = config.GenerateXXFalseColor;
            config.HTTPPort = config.HTTPPort;
            config.GenerateInfraredImages = config.GenerateInfraredImages;
            config.GenerateVisibleImages = config.GenerateVisibleImages;
            config.GenerateWaterVapourImages = config.GenerateWaterVapourImages;
            config.MaxGenerateRetry = config.MaxGenerateRetry;
            config.SysLogServer = config.SysLogServer;
            config.SysLogFacility = config.SysLogFacility;
            config.UseNOAAFormat = config.UseNOAAFormat;
            config.EnableWeatherData = config.EnableWeatherData;
            config.TemporaryFileFolder = config.TemporaryFileFolder;
            config.FinalFileFolder = config.FinalFileFolder;
            config.Save();
            #endregion

            FileHandler.SkipEMWIN = !config.EnableEMWIN;
            FileHandler.SkipDCS = !config.EnableDCS;
            FileHandler.SkipWeatherData = !config.EnableWeatherData;

            if (config.TemporaryFileFolder != null) {
                if (!LLTools.TestFolderAccess(config.TemporaryFileFolder)) {
                    UIConsole.GlobalConsole.Error($"Cannot write file to Temporary Folder {config.TemporaryFileFolder}");
                    throw new ApplicationException($"Cannot write file to Temporary Folder {config.TemporaryFileFolder}");
                }
                FileHandler.TemporaryFileFolder = config.TemporaryFileFolder;
            }

            if (config.FinalFileFolder != null) {
                if (!LLTools.TestFolderAccess(config.FinalFileFolder)) {
                    UIConsole.GlobalConsole.Error($"Cannot write file to Final Folder {config.FinalFileFolder}");
                    throw new ApplicationException($"Cannot write file to Final Folder {config.FinalFileFolder}");
                }
                FileHandler.FinalFileFolder = config.FinalFileFolder;
            }

            ImageManager.EraseFiles = config.EraseFilesAfterGeneratingFalseColor;
            ImageManager.GenerateInfrared = config.GenerateInfraredImages;
            ImageManager.GenerateVisible = config.GenerateVisibleImages;
            ImageManager.GenerateWaterVapour = config.GenerateWaterVapourImages;
            ImageManager.MaxRetryCount = config.MaxGenerateRetry;
            ImageManager.UseNOAAFileFormat = config.UseNOAAFormat;

            Connector.ChannelDataServerName = config.ChannelDataServerName;
            Connector.StatisticsServerName = config.StatisticsServerName;
            Connector.ConstellationServerName = config.ConstellationServerName;

            Connector.ChannelDataServerPort = config.ChannelDataServerPort;
            Connector.StatisticsServerPort = config.StatisticsServerPort;
            Connector.ConstellationServerPort = config.ConstellationServerPort;

            if (LLTools.IsLinux) {
                SyslogClient.SysLogServerIp = config.SysLogServer;
                try {
                    SyslogClient.Send(new Message(config.SysLogFacility, Level.Information, "Your syslog connection is working! OpenSatelliteProject is enabled to send logs."));
                } catch (SocketException) {
                    UIConsole.GlobalConsole.Warn("Your syslog is not enabled to receive UDP request. Please refer to https://opensatelliteproject.github.io/OpenSatelliteProject/");
                }
            }

            string fdFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_FULLDISK);
            string xxFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_AREA_OF_INTEREST);
            string nhFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_NORTHERN);
            string shFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_SOUTHERN);
            string usFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_UNITEDSTATES);
            string fmFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES16_ABI, (int)ScannerSubProduct.NONE);

            FDImageManager = new ImageManager(fdFolder);
            XXImageManager = new ImageManager(xxFolder);
            NHImageManager = new ImageManager(nhFolder);
            SHImageManager = new ImageManager(shFolder);
            USImageManager = new ImageManager(usFolder);
            FMImageManager = new ImageManager(fmFolder);

            directoryHandler = new DirectoryHandler(FileHandler.FinalFileFolder, "/data");

            mtx = new Mutex();
            cn = new Connector();

            demuxManager = new DemuxManager();
            demuxManager.RecordToFile = config.RecordIntermediateFile;
            cn.StatisticsAvailable += (data) => {
                mtx.WaitOne();
                statistics = data;
                mtx.ReleaseMutex();

                stModel.Refresh(statistics);
                httpsv.WebSocketServices.Broadcast(stModel.toJSON());
            };

            cn.ChannelDataAvailable += demuxManager.parseBytes;
            cn.ConstellationDataAvailable += (float[] data) => {
                ConstellationModel cm = new ConstellationModel(data);
                if (httpsv.IsListening) {
                    httpsv.WebSocketServices.Broadcast(cm.toJSON());
                }
            };

            statistics = new Statistics_st();
            stModel = new StatisticsModel(statistics);
            UIConsole.GlobalConsole.Log("Headless Main Created");
            httpsv = new HttpServer(config.HTTPPort);

            httpsv.RootPath = Path.Combine(".", "web");
            httpsv.OnGet += HandleHTTPGet;
            httpsv.AddWebSocketService("/mainws", () => new WSHandler {
                dh = directoryHandler
            });
            var x = httpsv.WebSocketServices ["/mainws"];

            UIConsole.GlobalConsole.MessageAvailable += (data) => {
                ConsoleModel cm = new ConsoleModel(data.Priority.ToString(), data.Message);
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

        private void HandleHTTPGet(object sender, HttpRequestEventArgs e) {
            var req = e.Request;
            var res = e.Response;

            var path = req.RawUrl;
            if (path == "/")
                path += "index.html";

            if (path.StartsWith(directoryHandler.BasePath)) {
                try {
                    directoryHandler.HandleAccess(httpsv, e);
                } catch (Exception ex) {
                    string output = string.Format("Error reading file: {0}", ex);
                    res.StatusCode = (int)HttpStatusCode.InternalServerError;
                    res.WriteContent(Encoding.UTF8.GetBytes(output));
                }
                return;
            }

            var content = httpsv.GetFile(path);
            if (content == null) {
                res.StatusCode = (int)HttpStatusCode.NotFound;
                string res404 = "File not found";
                res.WriteContent(Encoding.UTF8.GetBytes(res404));
                return;
            }

            if (path.EndsWith(".html")) {
                res.ContentType = "text/html";
                res.ContentEncoding = Encoding.UTF8;
            } else if (path.EndsWith(".js")) {
                res.ContentType = "application/javascript";
                res.ContentEncoding = Encoding.UTF8;
            }

            res.WriteContent(content);
        }

        public void Start() {
            Console.CancelKeyPress += delegate {
                UIConsole.GlobalConsole.Log("Hit Ctrl + C! Closing...");
                running = false;
            };

            UIConsole.GlobalConsole.Log("Headless Main Starting");

            FDImageManager.Start();
            XXImageManager.Start();
            NHImageManager.Start();
            SHImageManager.Start();
            USImageManager.Start();
            FMImageManager.Start();

            cn.Start();
            httpsv.Start();
            running = true;

            while (running) {
                Thread.Sleep(10);
            }

            UIConsole.GlobalConsole.Log("Closing program...");
            cn.Stop();
            httpsv.Stop();

            FDImageManager.Stop();
            XXImageManager.Stop();
            NHImageManager.Stop();
            SHImageManager.Stop();
            USImageManager.Stop();
            FMImageManager.Stop();
        }
    }
}

