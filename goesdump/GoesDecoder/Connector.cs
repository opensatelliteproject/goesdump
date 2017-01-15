using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace OpenSatelliteProject {
    public class Connector {
        #region Delegate

        public delegate void StatisticsEvent(Statistics_st data);

        public delegate void ChannelDataEvent(byte[] data);

        #endregion
        #region Event

        public event StatisticsEvent StatisticsAvailable;
        public event ChannelDataEvent ChannelDataAvailable;

        #endregion
        #region Threads

        private Thread statisticsThread;
        private Thread channelDataThread;

        #endregion
        #region Private Variables

        private bool statisticsThreadRunning;
        private bool channelDataThreadRunning;

        #endregion
        #region Constructor / Destructor

        public Connector() {
            statisticsThread = new Thread(new ThreadStart(statisticsLoop));
            channelDataThread = new Thread(new ThreadStart(channelDataLoop));

            statisticsThread.IsBackground = true;
            channelDataThread.IsBackground = true;

            statisticsThreadRunning = true;
            channelDataThreadRunning = true;

            statisticsThread.Start();
            channelDataThread.Start();
        }

        ~Connector() {
            Stop();
        }

        #endregion
        #region Properties

        public bool StatisticsConnected { get; set; }
        public bool DataConnected { get; set; }

        #endregion
        #region Methods

        public void Stop() {
            statisticsThreadRunning = false;
            channelDataThreadRunning = false;

            if (statisticsThread != null) {
                statisticsThread.Join();
                statisticsThread = null;
            }

            if (channelDataThread != null) {
                channelDataThread.Join();
                channelDataThread = null;
            }
        }

        private void postStatistics(object st) {
            StatisticsAvailable((Statistics_st)st);
        }

        private void postChannelData(object data) {
            ChannelDataAvailable((byte[])data);
        }

        private void statisticsLoop() {
            UIConsole.GlobalConsole.Log("Statistics Thread Started");
            byte[] buffer = new byte[4165];

            IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
            foreach (IPAddress ip in ipHostInfo.AddressList) {
              if (ip.AddressFamily != AddressFamily.InterNetworkV6) {
                ipAddress = ip;
                break;
              }
            }

            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 5002);
            Socket sender = null;
            bool isConnected;

            while (statisticsThreadRunning) {
                try {
                    UIConsole.GlobalConsole.Log("Statistics Thread connect");
                    sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sender.ReceiveTimeout = 200;
                    sender.Connect(remoteEP);
                    isConnected = true;

                    UIConsole.GlobalConsole.Log(String.Format("Socket connected to {0}", sender.RemoteEndPoint.ToString()));

                    while (isConnected) {
                        try {
                            if (sender.Available > buffer.Length) {
                                sender.Receive(buffer);
                                Statistics_st sst = Statistics_st.fromByteArray(buffer);
                                this.postStatistics(sst);
                            }
                        } catch (ArgumentNullException ane) {
                            UIConsole.GlobalConsole.Error(String.Format("ArgumentNullException : {0}", ane.ToString()));
                            isConnected = false;
                        } catch (SocketException se) {
                            UIConsole.GlobalConsole.Error(String.Format("SocketException : {0}", se.ToString()));
                            isConnected = false;
                        } catch (Exception e) {
                            UIConsole.GlobalConsole.Error(String.Format("Unexpected exception : {0}", e.ToString()));
                            isConnected = false;
                        }

                        StatisticsConnected = isConnected;

                        if (!statisticsThreadRunning) {
                            break;
                        }
                    }

                    sender.Shutdown(SocketShutdown.Both);
                    sender.Disconnect(false);
                    sender.Close();

                } catch (ArgumentNullException ane) {
                    UIConsole.GlobalConsole.Error(String.Format("ArgumentNullException : {0}", ane.ToString()));
                } catch (SocketException se) {
                    UIConsole.GlobalConsole.Error(String.Format("SocketException : {0}", se.ToString()));
                } catch (Exception e) {
                    UIConsole.GlobalConsole.Error(String.Format("Unexpected exception : {0}", e.ToString()));
                }

                if (statisticsThreadRunning) {
                    UIConsole.GlobalConsole.Warn("Socket closed. Waiting 1s before trying again.");
                    Thread.Sleep(1000);
                }
            }
            Console.WriteLine("Requested to close Statistics Thread!");
            try {
                if (sender != null) {
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Disconnect(false);
                    sender.Close();
                }
            } catch (Exception e) {
                UIConsole.GlobalConsole.Debug(String.Format("Exception thrown when closing socket: {0} Ignoring.", e.ToString()));
            }
            UIConsole.GlobalConsole.Log("Statistics Thread closed");
        }

        private void channelDataLoop() {
            UIConsole.GlobalConsole.Log("Channel Data Loop started");
            byte[] buffer = new byte[892];

            IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
            foreach (IPAddress ip in ipHostInfo.AddressList) {
              if (ip.AddressFamily != AddressFamily.InterNetworkV6) {
                ipAddress = ip;
                break;
              }
            }
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 5001);
            Socket sender = null;

            while (channelDataThreadRunning) {

                bool isConnected = true;
                UIConsole.GlobalConsole.Log("Channel Data Thread connect");
                try {
                    sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sender.Connect(remoteEP);
                    isConnected = true;
                    UIConsole.GlobalConsole.Log(String.Format("Socket connected to {0}", sender.RemoteEndPoint.ToString()));

                    while (isConnected) {
                        try {
                            if (sender.Available > buffer.Length) {
                                sender.Receive(buffer);
                                this.postChannelData(buffer);
                            }
                        } catch (ArgumentNullException ane) {
                            UIConsole.GlobalConsole.Error(String.Format("ArgumentNullException : {0}", ane.ToString()));
                            isConnected = false;
                        } catch (SocketException se) {
                            UIConsole.GlobalConsole.Error(String.Format("SocketException : {0}", se.ToString()));
                            isConnected = false;
                        } catch (Exception e) {
                            UIConsole.GlobalConsole.Error(String.Format("Unexpected exception : {0}", e.ToString()));
                            isConnected = false;
                        }

                        DataConnected = isConnected;

                        if (!channelDataThreadRunning) {
                            break;
                        }
                    }

                    sender.Shutdown(SocketShutdown.Both);
                    sender.Disconnect(false);
                    sender.Close();

                } catch (ArgumentNullException ane) {
                    UIConsole.GlobalConsole.Error(String.Format("ArgumentNullException : {0}", ane.ToString()));
                } catch (SocketException se) {
                    UIConsole.GlobalConsole.Error(String.Format("SocketException : {0}", se.ToString()));
                } catch (Exception e) {
                    UIConsole.GlobalConsole.Error(String.Format("Unexpected exception : {0}", e.ToString()));
                }
                if (channelDataThreadRunning) {
                    UIConsole.GlobalConsole.Warn("Socket closed. Waiting 1s before trying again.");
                    Thread.Sleep(1000);
                }
            }

            UIConsole.GlobalConsole.Debug("Requested to close Channel Data Thread!");
            try {
                if (sender != null) {
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Disconnect(false);
                    sender.Close();
                }
            } catch (Exception e) {
                UIConsole.GlobalConsole.Debug(String.Format("Exception thrown when closing socket: {0} Ignoring.", e.ToString()));
            }

            UIConsole.GlobalConsole.Log("Channel Data Thread closed.");
        }

        #endregion
    }
}

