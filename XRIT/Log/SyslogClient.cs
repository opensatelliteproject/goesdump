using System;
using System.Net;
using System.Net.Sockets;

namespace OpenSatelliteProject.Log {
    public class SyslogClient {
        private IPHostEntry ipHostInfo;
        private IPAddress ipAddress;
        private IPEndPoint ipLocalEndPoint;
        private UdpClient udpClient;

        public int Port { get; set; }
        public string SysLogServerIp { get; set; }
        public bool IsActive { get; set; }

        public SyslogClient() {
            if (!Tools.LLTools.IsLinux) {
                throw new ArgumentException("Syslog only works on Linux");
            }

            ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList[0];
            ipLocalEndPoint = new IPEndPoint(ipAddress, 0);
            udpClient= new UdpClient(ipLocalEndPoint);
            Port = 514;
            SysLogServerIp = "localhost";
        }

        public void Close() {
            if (IsActive) {
                udpClient.Close();
                IsActive = false;
            }
        }

        public void Send(Message message) {
            if (!IsActive) {
                udpClient.Connect(SysLogServerIp, Port);
                IsActive = true;
            }

            if (IsActive) {
                int priority = message.Facility * 8 + message.Level;
                string msg = System.String.Format("<{0}>{1} {2} {3}", priority, DateTime.Now.ToString("MMM dd HH:mm:ss"), "XRIT", message.Text);
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(msg);
                udpClient.Send(bytes, bytes.Length);
            }
        }
    }
}

