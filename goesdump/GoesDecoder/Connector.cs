using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace OpenSatelliteProject {
    public class Connector {

        public static string StatisticsServerName { get; set; }
        public static string ChannelDataServerName { get; set; }
        public static string ConstellationServerName { get; set; }
        public static int StatisticsServerPort { get; set; }
        public static int ChannelDataServerPort { get; set; }
        public static int ConstellationServerPort { get; set; }

        static Connector() {
            StatisticsServerName = "localhost";
            ChannelDataServerName = "localhost";
            ConstellationServerName = "localhost";
            StatisticsServerPort = 5002;
            ChannelDataServerPort = 5001;
            ConstellationServerPort = 9000;
        }

        #region Delegate

        public delegate void StatisticsEvent(Statistics_st data);
        public delegate void ChannelDataEvent(byte[] data);
        public delegate void ConstellationDataEvent(float[] data);

        #endregion
        #region Event

        public event StatisticsEvent StatisticsAvailable;
        public event ChannelDataEvent ChannelDataAvailable;
        public event ConstellationDataEvent ConstellationDataAvailable;

        #endregion
        #region Threads

        private Thread statisticsThread;
        private Thread channelDataThread;
        private Thread constellationDataThread;

        #endregion
        #region Private Variables

        private bool statisticsThreadRunning;
        private bool channelDataThreadRunning;
        private bool constellationDataThreadRunning;

        #endregion
        #region Constructor / Destructor

        public Connector() {
            statisticsThread = new Thread(new ThreadStart(statisticsLoop));
            channelDataThread = new Thread(new ThreadStart(channelDataLoop));
            constellationDataThread = new Thread(new ThreadStart(constellationDataLoop));

            statisticsThread.IsBackground = true;
            channelDataThread.IsBackground = true;
            constellationDataThread.IsBackground = true;

            statisticsThread.Priority = ThreadPriority.BelowNormal;
            channelDataThread.Priority = ThreadPriority.AboveNormal;
            constellationDataThread.Priority = ThreadPriority.BelowNormal;
        }

        public void Start() {
            statisticsThreadRunning = true;
            channelDataThreadRunning = true;
            constellationDataThreadRunning = true;

            statisticsThread.Start();
            channelDataThread.Start();
            constellationDataThread.Start();
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
            constellationDataThreadRunning = false;

            if (statisticsThread != null) {
                statisticsThread.Join();
                statisticsThread = null;
            }

            if (channelDataThread != null) {
                channelDataThread.Join();
                channelDataThread = null;
            }

            if (constellationDataThread != null) {
                constellationDataThread.Join();
                constellationDataThread = null;
            }
        }

        private void postStatistics(object st) {
            StatisticsAvailable?.Invoke((Statistics_st)st);
        }

        private void postChannelData(object data) {
            ChannelDataAvailable?.Invoke((byte[])data);
        }

        private void postConstellationData(object data) {
            ConstellationDataAvailable?.Invoke((float[])data);
        }

        private void statisticsLoop() {
            UIConsole.Log("Statistics Thread Started");
            byte[] buffer = new byte[4165];

            IPHostEntry ipHostInfo = Dns.GetHostEntry(StatisticsServerName);
            IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
            foreach (IPAddress ip in ipHostInfo.AddressList) {
              if (ip.AddressFamily != AddressFamily.InterNetworkV6) {
                ipAddress = ip;
                break;
              }
            }

            IPEndPoint remoteEP = new IPEndPoint(ipAddress, StatisticsServerPort);
            Socket sender = null;
            bool isConnected;

            while (statisticsThreadRunning) {
                try {
                    UIConsole.Log("Statistics Thread connect");
                    sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sender.ReceiveTimeout = 5000;
                    sender.Connect(remoteEP);
                    isConnected = true;

                    UIConsole.Log(String.Format("Socket connected to {0}", sender.RemoteEndPoint.ToString()));
                    int nullReceive = 0;
                    while (isConnected) {
                        try {
                            var receivedBytes = sender.Receive(buffer);
                            if (receivedBytes < buffer.Length && receivedBytes != 0) {
                                nullReceive = 0;
                                UIConsole.Error("Received less than Statistics Packet size!");
                                Thread.Sleep(200);
                            } else  if (receivedBytes == 0) {
                                nullReceive++;
                                if (nullReceive == 5) {
                                    UIConsole.Error("Cannot reach server. Dropping connection!");
                                    isConnected = false;
                                    sender.Shutdown(SocketShutdown.Both);
                                    sender.Disconnect(false);
                                    sender.Close();
                                }
                            } else {
                                nullReceive = 0;
                                Statistics_st sst = Statistics_st.fromByteArray(buffer);
                                this.postStatistics(sst);
                            }
                        } catch (ArgumentNullException ane) {
                            UIConsole.Error(String.Format("ArgumentNullException : {0}", ane.ToString()));
                            isConnected = false;
                        } catch (SocketException se) {
                            // That's usually timeout.  I would say that is best to handle and show some message
                            // But for now, that would make it confusing for the users. So let's keep without a notice.
                            //UIConsole.GlobalConsole.Error(String.Format("SocketException : {0}", se.ToString()));
                            isConnected = false;
                        } catch (Exception e) {
                            UIConsole.Error(String.Format("Unexpected exception : {0}", e.ToString()));
                            isConnected = false;
                        }

                        StatisticsConnected = isConnected;

                        if (!statisticsThreadRunning) {
                            break;
                        }
                        Thread.Sleep(1);
                    }

                    sender.Shutdown(SocketShutdown.Both);
                    sender.Disconnect(false);
                    sender.Close();

                } catch (ArgumentNullException ane) {
                    UIConsole.Error(String.Format("ArgumentNullException : {0}", ane.ToString()));
                } catch (SocketException se) {
                    UIConsole.Error(String.Format("SocketException : {0}", se.ToString()));
                } catch (Exception e) {
                    UIConsole.Error(String.Format("Unexpected exception : {0}", e.ToString()));
                }

                if (statisticsThreadRunning) {
                    UIConsole.Warn("Socket closed. Waiting 1s before trying again.");
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
                UIConsole.Debug(String.Format("Exception thrown when closing socket: {0} Ignoring.", e.ToString()));
            }
            UIConsole.Log("Statistics Thread closed");
        }

        private void channelDataLoop() {
            UIConsole.Log("Channel Data Loop started");
            byte[] buffer = new byte[892];

            IPHostEntry ipHostInfo = Dns.GetHostEntry(ChannelDataServerName);
            IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
            foreach (IPAddress ip in ipHostInfo.AddressList) {
                if (ip.AddressFamily != AddressFamily.InterNetworkV6) {
                    ipAddress = ip;
                    break;
                }
            }
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, ChannelDataServerPort);
            Socket sender = null;

            while (channelDataThreadRunning) {

                bool isConnected = true;
                UIConsole.Log("Channel Data Thread connect");
                try {
                    sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sender.ReceiveTimeout = 3000;
                    sender.Connect(remoteEP);
                    isConnected = true;
                    UIConsole.Log(String.Format("Socket connected to {0}", sender.RemoteEndPoint.ToString()));
                    int nullReceive = 0;
                    while (isConnected) {
                        try {
                            var receivedBytes = sender.Receive(buffer);
                            if (receivedBytes < buffer.Length && receivedBytes != 0) {
                                UIConsole.Error("Received less bytes than channel data!");
                                Thread.Sleep(200);
                                nullReceive = 0;
                            } else  if (receivedBytes == 0) {
                                nullReceive++;
                                if (nullReceive == 5) {
                                    UIConsole.Error("Cannot reach server. Dropping connection!");
                                    isConnected = false;
                                    sender.Shutdown(SocketShutdown.Both);
                                    sender.Disconnect(false);
                                    sender.Close();
                                }
                            } else {
                                nullReceive = 0;
                                this.postChannelData(buffer);
                            }
                        } catch (ArgumentNullException ane) {
                            UIConsole.Error(String.Format("ArgumentNullException : {0}", ane.ToString()));
                            isConnected = false;
                        } catch (SocketException se) {
                            // That's usually timeout.  I would say that is best to handle and show some message
                            // But for now, that would make it confusing for the users. So let's keep without a notice.
                            //UIConsole.GlobalConsole.Error(String.Format("SocketException : {0}", se.ToString()));
                            isConnected = false;
                        } catch (Exception e) {
                            UIConsole.Error(String.Format("Unexpected exception : {0}", e.ToString()));
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
                    UIConsole.Error(String.Format("ArgumentNullException : {0}", ane.ToString()));
                } catch (SocketException se) {
                    UIConsole.Error(String.Format("SocketException : {0}", se.ToString()));
                } catch (Exception e) {
                    UIConsole.Error(String.Format("Unexpected exception : {0}", e.ToString()));
                }
                if (channelDataThreadRunning) {
                    UIConsole.Warn("Socket closed. Waiting 1s before trying again.");
                    Thread.Sleep(1000);
                }
            }

            UIConsole.Debug("Requested to close Channel Data Thread!");
            try {
                if (sender != null) {
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Disconnect(false);
                    sender.Close();
                }
            } catch (Exception e) {
                UIConsole.Debug(String.Format("Exception thrown when closing socket: {0} Ignoring.", e.ToString()));
            }

            UIConsole.Log("Channel Data Thread closed.");
        }

        private void constellationDataLoop() {
            UIConsole.Log("Constellation Data Loop started");
            byte[] buffer = null;
            float[] data = new float[1024];
            for (int i = 0; i < 1024; i++) {
                data[i] = 0;
            }

            IPHostEntry ipHostInfo = Dns.GetHostEntry(ConstellationServerName);
            IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
            foreach (IPAddress ip in ipHostInfo.AddressList) {
                if (ip.AddressFamily != AddressFamily.InterNetworkV6) {
                    ipAddress = ip;
                    break;
                }
            }

            IPEndPoint remoteEP = new IPEndPoint(ipAddress, ConstellationServerPort);
            UdpClient udpClient = new UdpClient(ConstellationServerPort);
            udpClient.Client.ReceiveTimeout = 200;

            while (constellationDataThreadRunning) {
                try {
                    buffer = udpClient.Receive(ref remoteEP);
                    if (buffer != null && buffer.Length == 1024) {
                        for (int i = 0; i < 1024; i++) {
                            sbyte t = (sbyte)buffer[i];
                            data[i] = t;
                            data[i] /= 128f;
                        }
                        this.postConstellationData(data);
                    }
                } catch (SocketException) {
                    // Do nothing, timeout on UDP
                }
            }
           
            UIConsole.Log("Constellation Thread closed.");
        }

        #endregion
    }
}

