using System;
using OpenSatelliteProject.Log;
using System.Net.Sockets;
using OpenSatelliteProject.Tools;
using System.Threading;
using System.Text;
using System.Globalization;

namespace OpenSatelliteProject {
    public class UIConsole {

        public const string SYSLOGSERVERDBKEY = "syslogServerAddress";
        public const string SYSLOGFACILITYDBKEY = "syslogServerFacility";

        public static UIConsole GlobalConsole;
        static readonly SyslogClient syslog = new SyslogClient();

        readonly Mutex messageMutex;
        public bool LogConsole { get; set; }
        public delegate void ConsoleEvent(ConsoleMessage data);
        public event ConsoleEvent MessageAvailable;

        static string RemoveDiacritics(string text) {
          var normalizedString = text.Normalize(NormalizationForm.FormD);
          var stringBuilder = new StringBuilder();

          foreach (var c in normalizedString) {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark) {
              stringBuilder.Append(c);
            }
          }

          return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        static UIConsole() {
            Init();
        }

        public static void Init() {
            if (GlobalConsole == null) {
                GlobalConsole = new UIConsole();
            }
        }

        public UIConsole() {
            LogConsole = true;
            messageMutex = new Mutex();
            if (LLTools.IsLinux) {
                SyslogClient.SysLogServerIp = ConfigurationManager.Get(SYSLOGSERVERDBKEY, "127.0.0.1");
            }
        }
            
        public void Log(string message) {
            messageMutex.WaitOne();
            ConsoleMessage cm = new ConsoleMessage(ConsoleMessagePriority.INFO, message);
            if (LogConsole) {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(cm.ToString());
                Console.ForegroundColor = oldColor;
            }

            if (syslog != null) {
                try {
                    SyslogClient.Send(new Message(ConfigurationManager.Get(SYSLOGFACILITYDBKEY, "LOG_USER"), Level.Information, cm.Message));
                } catch (SocketException) {
                    // Syslog not configured, ignore.
                }
            }
            messageMutex.ReleaseMutex();
            MessageAvailable?.Invoke(cm);
        }

        public void Warn(string message) {
            messageMutex.WaitOne();
            ConsoleMessage cm = new ConsoleMessage(ConsoleMessagePriority.WARN, message);
            if (LogConsole) {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(cm.ToString());
                Console.ForegroundColor = oldColor;
            }

            if (syslog != null) {
                try {
                    SyslogClient.Send(new Message(ConfigurationManager.Get(SYSLOGFACILITYDBKEY, "LOG_USER"), Level.Warning, cm.Message));
                } catch (SocketException) {
                    // Syslog not configured, ignore.
                }
            }
            messageMutex.ReleaseMutex();
            MessageAvailable?.Invoke(cm);
        }

        public void Error(string message) {
            messageMutex.WaitOne();
            ConsoleMessage cm = new ConsoleMessage(ConsoleMessagePriority.ERROR, message);
            if (LogConsole) {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(cm.ToString());
                Console.ForegroundColor = oldColor;
            }

            if (syslog != null) {
                try {
                    SyslogClient.Send(new Message(ConfigurationManager.Get(SYSLOGFACILITYDBKEY, "LOG_USER"), Level.Error, cm.Message));
                } catch (SocketException) {
                    // Syslog not configured, ignore.
                }
            }
            messageMutex.ReleaseMutex();
            MessageAvailable?.Invoke(cm);
        }

        public void Debug(string message) {
            messageMutex.WaitOne();
            ConsoleMessage cm = new ConsoleMessage(ConsoleMessagePriority.DEBUG, message);
            if (LogConsole) {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(cm.ToString());
                Console.ForegroundColor = oldColor;
            }

            if (syslog != null) {
                try {
                    SyslogClient.Send(new Message(ConfigurationManager.Get(SYSLOGFACILITYDBKEY, "LOG_USER"), Level.Debug, cm.Message));
                } catch (SocketException) {
                    // Syslog not configured, ignore.
                }
            }
            messageMutex.ReleaseMutex();
            MessageAvailable?.Invoke(cm);
        }
    }
}

