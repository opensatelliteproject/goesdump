using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace OpenSatelliteProject {
    public class UIConsole: Drawable, Updatable {

        private static readonly int MAX_MESSAGES = 10;
        private static readonly float LINE_SPACING = 2;

        public static UIConsole GlobalConsole;

        public bool LogConsole { get; set; }

        public Vector2 Position { get; set; }
        public SpriteFont Font { get; set; }

        private List<ConsoleMessage> messages;
        private Mutex messageMutex;

        static UIConsole() {
            GlobalConsole = new UIConsole();
        }

        public UIConsole() {
            messages = new List<ConsoleMessage>();
            LogConsole = true;
            messageMutex = new Mutex();
            Position = new Vector2(0, 0);
        }

        public float MaxHeight {
            get {
                return (Font.MeasureString("A").Y + LINE_SPACING) * (MAX_MESSAGES+1) + 5;
            }
        }

        #region Drawable implementation

        public void draw(SpriteBatch spriteBatch, GameTime gameTime) {
            float fontHeight = Font.MeasureString("A").Y;
            Vector2 curPos = new Vector2(Position.X, Position.Y);
            spriteBatch.DrawString(Font, "Console: ", curPos, Color.Black);
            curPos.Y += 5 + fontHeight;

            messageMutex.WaitOne();
            foreach (ConsoleMessage m in messages) {
                try {
                    spriteBatch.DrawString(Font, m.ToString(), curPos, ConsoleMessage.CMP2COLOR[m.Priority]);
                } catch (Exception e) {
                    Console.WriteLine(String.Format("Got exception {0} with: \n\n{1}", e, m.ToString()));
                }
                curPos.Y += fontHeight + LINE_SPACING;
            }
            messageMutex.ReleaseMutex();
        }

        #endregion

        #region Updatable implementation

        public void update(GameTime gameTime) {
            // Do Nothing
        }

        #endregion

        private void addMessage(ConsoleMessage message) {
            string[] lines = message.Message.Split('\n');
            int totalLines = messages.Count + lines.Length;
            while (totalLines > MAX_MESSAGES) {
                messages.RemoveAt(0);
                totalLines--;
            }
            if (lines.Length == 1) {
                message.Message = message.Message.Replace('\n', ' ').Replace('\r', ' ').Trim();
                if (message.Message.Length > 0) {
                    messages.Add(message);
                }
            } else {
                foreach (string l in lines.ToList()) {
                    ConsoleMessage m = new ConsoleMessage(message.Priority, l);
                    m.TimeStamp = message.TimeStamp;
                    m.Message = m.Message.Replace('\n', ' ').Replace('\r', ' ').Trim();;
                    if (m.Message.Length > 0) {
                        messages.Add(m);
                    }
                }
            }
        }

        public void Log(string message) {
            messageMutex.WaitOne();
            ConsoleMessage cm = new ConsoleMessage(ConsoleMessagePriority.INFO, message);
            addMessage(cm);
            if (LogConsole) {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(cm.ToString());
                Console.ForegroundColor = oldColor;
            }
            messageMutex.ReleaseMutex();
        }

        public void Warn(string message) {
            messageMutex.WaitOne();
            ConsoleMessage cm = new ConsoleMessage(ConsoleMessagePriority.WARN, message);
            addMessage(cm);
            if (LogConsole) {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(cm.ToString());
                Console.ForegroundColor = oldColor;
            }
            messageMutex.ReleaseMutex();
        }

        public void Error(string message) {
            messageMutex.WaitOne();
            ConsoleMessage cm = new ConsoleMessage(ConsoleMessagePriority.ERROR, message);
            addMessage(cm);
            if (LogConsole) {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(cm.ToString());
                Console.ForegroundColor = oldColor;
            }
            messageMutex.ReleaseMutex();
        }

        public void Debug(string message) {
            messageMutex.WaitOne();
            ConsoleMessage cm = new ConsoleMessage(ConsoleMessagePriority.DEBUG, message);
            addMessage(cm);
            if (LogConsole) {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(cm.ToString());
                Console.ForegroundColor = oldColor;
            }
            messageMutex.ReleaseMutex();
        }
    }
}

