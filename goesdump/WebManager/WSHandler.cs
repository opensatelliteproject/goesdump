using System;
using WebSocketSharp.Server;
using WebSocketSharp;

namespace OpenSatelliteProject {
    public class WSHandler: WebSocketBehavior {
        protected override void OnMessage(MessageEventArgs e) {
            var msg = e.Data == "BALUS"
                ? "I've been balused already..."
                : "I'm not available now.";

            Send(msg);
        }

    }
}

