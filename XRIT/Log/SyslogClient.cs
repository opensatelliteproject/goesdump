using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace OpenSatelliteProject.Log {
    public class SyslogClient {
        private static IPHostEntry ipHostInfo;
        private static IPAddress ipAddress;
        private static IPEndPoint ipLocalEndPoint;
        private static UdpClient udpClient;
        private static Dictionary<string,Facility> FacilityMap;
        public static int Port { get; set; }
        public static string SysLogServerIp { get; set; }
        public static bool IsActive { get; set; }

        static SyslogClient() {
            if (!Tools.LLTools.IsLinux) {
                Console.WriteLine("Syslog only works on Linux");
                return;
            }

            ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList[0];
            ipLocalEndPoint = new IPEndPoint(ipAddress, 0);
            udpClient= new UdpClient(ipLocalEndPoint);
            Port = 514;
            InitFacilityMap();
        }

        public void Close() {
            if (IsActive) {
                udpClient.Close();
                IsActive = false;
            }
        }

        public static void Send(Message message) {
            if (Tools.LLTools.IsLinux) {
                if (!IsActive) {
                    udpClient.Connect(SysLogServerIp, Port);
                    IsActive = true;
                }

                if (IsActive) {
                    int priority = (int)FacilityMap[message.Facility] * 8 + message.Level;
                    string msg = System.String.Format("<{0}>{1} {2} {3}", priority, DateTime.Now.ToString("MMM dd HH:mm:ss"), "XRIT", message.Text);
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(msg);
                    udpClient.Send(bytes, bytes.Length);
                }
            }
        }

        private static void InitFacilityMap() {
            FacilityMap = new Dictionary<string, Facility>();

            FacilityMap["LOG_KERNEL"] = Facility.Kernel;
            FacilityMap["LOG_USER"]   = Facility.User;
            FacilityMap["LOG_MAIL"]   = Facility.Mail;
            FacilityMap["LOG_DAEMON"] = Facility.Daemon;
            FacilityMap["LOG_AUTH"]   = Facility.Auth;
            FacilityMap["LOG_SYSLOG"] = Facility.Syslog;
            FacilityMap["LOG_LPR"]    = Facility.Lpr;
            FacilityMap["LOG_NEWS"]   = Facility.News;
            FacilityMap["LOG_UUCP"]   = Facility.UUCP;
            FacilityMap["LOG_CRON"]   = Facility.Cron;
            FacilityMap["LOG_LOCAL0"] = Facility.Local0;
            FacilityMap["LOG_LOCAL1"] = Facility.Local1;
            FacilityMap["LOG_LOCAL2"] = Facility.Local2;
            FacilityMap["LOG_LOCAL3"] = Facility.Local3;
            FacilityMap["LOG_LOCAL4"] = Facility.Local4;
            FacilityMap["LOG_LOCAL5"] = Facility.Local5;
            FacilityMap["LOG_LOCAL6"] = Facility.Local6;
            FacilityMap["LOG_LOCAL7"] = Facility.Local7;
        }

    }
}

