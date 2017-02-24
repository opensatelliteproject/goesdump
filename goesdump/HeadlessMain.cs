using System;
using System.Threading;
using WebSocketSharp.Server;
using System.Net;
using System.Text;
using System.IO;
using WebSocketSharp;

namespace OpenSatelliteProject {
    public class HeadlessMain {
        Mutex mtx;
        Connector cn;
        DemuxManager demuxManager;
        Statistics_st statistics;
        StatisticsModel stModel;
        HttpServer httpsv;

        bool running = false;

        public HeadlessMain() {
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
            httpsv = new HttpServer(8090);
            httpsv.Log.Level = LogLevel.Trace;
            httpsv.RootPath = Path.Combine(".", "web");
            httpsv.OnGet += HandleHTTPGet;

            httpsv.AddWebSocketService<WSHandler>("/mainws");
            UIConsole.GlobalConsole.MessageAvailable += (message, priority) => {
                ConsoleModel cm = new ConsoleModel(priority.ToString(), message);
                if (httpsv.IsListening) {
                    httpsv.WebSocketServices["/mainws"].Sessions.Broadcast(cm.toJSON());
                }
            };
        }

        private void HandleHTTPGet(object sender, HttpRequestEventArgs e) {
            var req = e.Request;
            var res = e.Response;

            var path = req.RawUrl;
            if (path == "/")
                path += "index.html";

            var content = httpsv.GetFile(path);
            if (content == null) {
                res.StatusCode = (int)HttpStatusCode.NotFound;
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
            UIConsole.GlobalConsole.Log("Headless Main Starting");
            cn.Start();
            httpsv.Start();
            running = true;
            while (running) {
                Thread.Sleep(1000);
            }
            httpsv.Stop();
        }
    }
}

