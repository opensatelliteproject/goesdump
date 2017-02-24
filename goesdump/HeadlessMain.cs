using System;
using System.Threading;
using WebSocketSharp.Server;

namespace OpenSatelliteProject {
    public class HeadlessMain {
        Mutex mtx;
        Connector cn;
        DemuxManager demuxManager;
        Statistics_st statistics;
        StatisticsModel stModel;
        WebSocketServer wssv;

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
                wssv.WebSocketServices.Broadcast(stModel.toJSON());
            };
            cn.ChannelDataAvailable += (byte[] data) => demuxManager.parseBytes(data);
            cn.ConstellationDataAvailable += (float[] data) => {
                ConstellationModel cm = new ConstellationModel(data);
                if (wssv.IsListening) {
                    wssv.WebSocketServices.Broadcast(cm.toJSON());
                }
            };
            statistics = new Statistics_st();
            stModel = new StatisticsModel(statistics);
            UIConsole.GlobalConsole.Log("Headless Main Created");
            wssv = new WebSocketServer ("ws://0.0.0.0:8090");
            wssv.AddWebSocketService<WSHandler> ("/mainws");
            UIConsole.GlobalConsole.MessageAvailable += (message, priority) => {
                ConsoleModel cm = new ConsoleModel(priority.ToString(), message);
                if (wssv.IsListening) {
                    wssv.WebSocketServices.Broadcast(cm.toJSON());
                }
            };
        }


        public void Start() {
            UIConsole.GlobalConsole.Log("Headless Main Starting");
            cn.Start();
            wssv.Start ();
            running = true;
            while (running) {
                Thread.Sleep(1000);
            }
            wssv.Stop();
        }
    }
}

