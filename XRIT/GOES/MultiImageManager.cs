using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OpenSatelliteProject.GOES {
    public class MultiImageManager {
        private readonly List<ImageManager> imageManagers;
        private readonly bool multiThread;
        private bool running;
        private Thread imageThread;

        public MultiImageManager(IEnumerable<string> folders, bool MultiThreaded = true) {
            this.multiThread = MultiThreaded;
            imageManagers = folders.Select(folder => new ImageManager(folder)).ToList();
        }


        public void Start() {
            if (running) return;
            if (multiThread) {
                UIConsole.Log("Starting Multi-Image Manager in Multi Thread mode.");
                imageManagers.ForEach(im => im.Start());
            } else {
                UIConsole.Log("Starting Multi-Image Manager in Single Thread mode.");
                imageThread = new Thread(ThreadLoop) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal
                };
                imageThread.Start();
            }

            running = true;
        }

        public void InitMapDrawer() {
            imageManagers.ForEach(im => im.InitMapDrawer());
        }

        public void InitMapDrawer(string filename) {
            imageManagers.ForEach(im => im.InitMapDrawer(filename));
        }

        public void Stop() {
            if (!running) return;
            UIConsole.Log("Stopping Multi-Image Manager.");
            if (multiThread) {
                imageManagers.ForEach(im => im.Stop());
            } else {
                running = false;
                imageThread?.Join();
                imageThread = null;
            }
        }

        private static void ManageImageManager(ImageManager im) {
            try {
                UIConsole.Debug($"Processing folder {im.Folder}");
                im.RunningSingleThread = true;
                im.SingleThreadRun();
                Thread.Sleep(200);
            } catch (Exception e) {
                UIConsole.Error($"Error processing image manager single thread: {e}");
                CrashReport.Report(e);
                throw;
            }
        }

        private void ThreadLoop() {
            UIConsole.Log("MultiImage Thread running.");
            while (running) {
                imageManagers.ForEach(ManageImageManager);
            }
            UIConsole.Log("MultiImage Thread stopped.");
        }
    }
}
