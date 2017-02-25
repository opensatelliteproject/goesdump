using System;
using WebSocketSharp.Server;
using WebSocketSharp;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace OpenSatelliteProject {
    public class WSHandler: WebSocketBehavior {
        protected override void OnMessage(MessageEventArgs e) {

        }

        protected override void OnOpen() {
            List<ConsoleMessage> messages = HeadlessMain.GetCachedMessages;
            for (int i = 0; i < messages.Count; i++) {
                ConsoleModel cm = new ConsoleModel(messages[i].Priority.ToString(), messages[i].Message);
                Send(cm.toJSON());
            }
        }

    }
}

