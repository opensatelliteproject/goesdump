using System;
using System.Threading;

namespace OpenSatelliteProject {
    public class HeadlessMain {
        Mutex mtx;
        Connector cn;
        DemuxManager demuxManager;
        Statistics_st statistics;

        bool running = false;

        public HeadlessMain() {
            mtx = new Mutex();
            cn = new Connector();            
            demuxManager = new DemuxManager();
            cn.StatisticsAvailable += (Statistics_st data) => {
                mtx.WaitOne();
                statistics = data;
                mtx.ReleaseMutex();
                /*
                frameLockLed.Color = data.frameLock == 1 ? Color.Lime : Color.Red;
                lastPhaseCorrection = data.phaseCorrection;
                satelliteBusyLed.Color = data.vcid != 63 && data.frameLock == 1 ? Color.Lime : Color.Red;
                if (heartBeatLed.Color != Color.Lime) {
                    heartBeatLed.Color = Color.Lime;
                    heartBeatCount = 0;
                }*/
            };
            cn.ChannelDataAvailable += (byte[] data) => demuxManager.parseBytes(data);
            //cn.ConstellationDataAvailable += (float[] data) => cons.updateConstellationData(data);
            statistics = new Statistics_st();
            UIConsole.GlobalConsole.Log("Headless Main Created");
        }


        public void Start() {
            UIConsole.GlobalConsole.Log("Headless Main Starting");
            cn.Start();
            running = true;
            while (running) {
                Thread.Sleep(1000);
            }
        }
    }
}

