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

namespace OpenSatelliteProject {
    public class HeadlessMain {

        private static readonly int MAX_CACHED_MESSAGES = 10;

        private static ProgConfig config = new ProgConfig();

        private ImageManager FDImageManager;
        private ImageManager XXImageManager;
        private ImageManager NHImageManager;
        private ImageManager SHImageManager;
        private ImageManager USImageManager;

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
            config.GenerateUSFalseColor = config.GenerateXXFalseColor;
            config.HTTPPort = config.HTTPPort;
            config.GenerateInfraredImages = config.GenerateInfraredImages;
            config.GenerateVisibleImages = config.GenerateVisibleImages;
            config.GenerateWaterVapourImages = config.GenerateWaterVapourImages;
            config.MaxGenerateRetry = config.MaxGenerateRetry;
            config.SysLogServer = config.SysLogServer;
            config.SysLogFacility = config.SysLogFacility;
            config.Save();
            #endregion

            FileHandler.SkipEMWIN = !config.EnableEMWIN;
            FileHandler.SkipDCS = !config.EnableDCS;
            ImageManager.EraseFiles = config.EraseFilesAfterGeneratingFalseColor;
            ImageManager.GenerateInfrared = config.GenerateInfraredImages;
            ImageManager.GenerateVisible = config.GenerateVisibleImages;
            ImageManager.GenerateWaterVapour = config.GenerateWaterVapourImages;
            ImageManager.MaxRetryCount = config.MaxGenerateRetry;

            Connector.ChannelDataServerName = config.ChannelDataServerName;
            Connector.StatisticsServerName = config.StatisticsServerName;
            Connector.ConstellationServerName = config.ConstellationServerName;

            Connector.ChannelDataServerPort = config.ChannelDataServerPort;
            Connector.StatisticsServerPort = config.StatisticsServerPort;
            Connector.ConstellationServerPort = config.ConstellationServerPort;

            SyslogClient.SysLogServerIp = config.SysLogServer;

            if (LLTools.IsLinux) {
                try {
                    SyslogClient.Send(new Message(config.SysLogFacility, Level.Information, "Your syslog connection is working! OpenSatelliteProject is enabled to send logs."));
                } catch (WebSocketException) {
                    UIConsole.GlobalConsole.Warn("Your syslog is not enabled to receive UDP request. Please refer to https://opensatelliteproject.github.io/OpenSatelliteProject/");
                }
            }

            if (config.GenerateFDFalseColor) {
                string fdFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_FULLDISK);
                FDImageManager = new ImageManager(Path.Combine("channels", fdFolder));
            }

            if (config.GenerateXXFalseColor) {
                string xxFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_AREA_OF_INTEREST);
                XXImageManager = new ImageManager(Path.Combine("channels", xxFolder));
            }

            if (config.GenerateNHFalseColor) {
                string nhFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_NORTHERN);
                NHImageManager = new ImageManager(Path.Combine("channels", nhFolder));
            }

            if (config.GenerateSHFalseColor) {
                string shFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_SOUTHERN);
                SHImageManager = new ImageManager(Path.Combine("channels", shFolder));
            }

            if (config.GenerateUSFalseColor) {
                string usFolder = PacketManager.GetFolderByProduct(NOAAProductID.GOES13_ABI, (int)ScannerSubProduct.INFRARED_UNITEDSTATES);
                USImageManager = new ImageManager(Path.Combine("channels", usFolder));
            }

            directoryHandler = new DirectoryHandler("channels", "/data");

            mtx = new Mutex();
            cn = new Connector();

            demuxManager = new DemuxManager();
            cn.StatisticsAvailable += (Statistics_st data) => {
                mtx.WaitOne();
                statistics = data;
                mtx.ReleaseMutex();

                stModel.Refresh(statistics);
                httpsv.WebSocketServices.Broadcast(stModel.toJSON());
            };

            cn.ChannelDataAvailable += (byte[] data) => demuxManager.parseBytes(data);
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

            httpsv.AddWebSocketService<WSHandler>("/mainws");

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

            if (config.GenerateFDFalseColor) {
                FDImageManager.Start();
            }
            if (config.GenerateXXFalseColor) {
                XXImageManager.Start();
            }
            if (config.GenerateNHFalseColor) {
                NHImageManager.Start();
            }
            if (config.GenerateSHFalseColor) {
                SHImageManager.Start();
            }
            if (config.GenerateUSFalseColor) {
                USImageManager.Start();
            }

            cn.Start();
            httpsv.Start();
            running = true;

            while (running) {
                Thread.Sleep(10);
            }

            UIConsole.GlobalConsole.Log("Closing program...");
            cn.Stop();
            httpsv.Stop();

            if (config.GenerateFDFalseColor) {
                FDImageManager.Stop();
            }
            if (config.GenerateXXFalseColor) {
                XXImageManager.Stop();
            }
            if (config.GenerateNHFalseColor) {
                NHImageManager.Stop();
            }
            if (config.GenerateSHFalseColor) {
                SHImageManager.Stop();
            }
            if (config.GenerateUSFalseColor) {
                USImageManager.Stop();
            }
        }
    }
}

