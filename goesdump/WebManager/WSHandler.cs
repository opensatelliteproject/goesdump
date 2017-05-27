using System;
using WebSocketSharp.Server;
using WebSocketSharp;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace OpenSatelliteProject {
    public class WSHandler: WebSocketBehavior {

        public DirectoryHandler dh { get; set; }

        protected override void OnMessage(MessageEventArgs e) {
            var d = e.Data;
            try {
                var json = (JObject)JsonConvert.DeserializeObject (d);
                if (json["type"] != null) {
                    var type = (string)json["type"];
                    switch (type) {
                        case "config":
                            var variable = (string)json["variable"];
                            var value = (string)json["value"];
                            UIConsole.Debug($"Received config change request of {variable} to {value}");
                            EventMaster.Post("configChange", new ConfigChangeEventData() { Name = variable, Value = value });
                        break;
                        case "dirlist":
                            var path = (string)json["path"];
                            UIConsole.Debug($"Received request for listing folder {path}");
                            if (dh != null) {
                                var list = dh.ListDir(path);
                                var dl = new DirList(list);
                                Send(dl.toJSON());
                            }
                        break;
                    }
                }
            } catch (Exception) {
                UIConsole.Debug ($"Received invalid message from ws client: {d}");
            }
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

