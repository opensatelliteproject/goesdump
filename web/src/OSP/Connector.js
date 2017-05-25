/**
 * Created by Lucas Teske on 24/05/17.
 * @flow
 */

import { EventEmitter } from 'fbemitter';

export default class OSPConnector extends EventEmitter {

  constructor() {
    super();
    //this.serverUrl = "ws://" + window.location.host + "/mainws";
    this.serverUrl = "ws://localhost:8090/mainws";
    this.ws = new WebSocket(this.serverUrl);

    this.ws.onopen    = this.onOpen.bind(this);
    this.ws.onmessage = this.onMessage.bind(this);
    this.ws.onerror   = this.onError.bind(this);
    this.ws.onclose   = this.onClose.bind(this);
  }

  onOpen(event) {
    this.emit('consoleMessage', ["INFO", "Connected to Server"]);
    this.emit('wsConnected');
    this.ws.send(JSON.stringify({ "method": "getCacheMessages"}));
  }

  onMessage(event) {
    try {
      const data = JSON.parse(event.data);
      switch (data.DataType) {
        case "console":
          this.handleConsole(data);
          break;
        case "statisticsData":
          this.handleStatistics(data);
          break;
        case "constellationData":
          this.handleConstellation(data);
          break;
      }
    } catch (e) {
      console.log(`OSPConnector -- Error parsing message: ${e}`);
    }
  }

  onError(event) {
    this.emit('consoleMessage', ["ERROR", `There was an WebSocket error: ${event}`]);
    console.log(`OSPConnector -- Error: ${event}`);
  }

  onClose(event) {
    this.emit('consoleMessage', ["INFO", "Connection to the server has been closed."]);
    this.emit('wsDisconnected');
  }

  handleStatistics(data) {
    this.emit('statistics', data);
  }

  handleConstellation(data) {
    this.emit('constellation', data.data);
  }

  handleConsole(data) {
    this.emit('consoleMessage', [ data.level, data.message ]);
  }

  on(...args) {
    this.addListener(...args);
  }
}
