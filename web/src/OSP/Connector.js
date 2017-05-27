/**
 * Created by Lucas Teske on 24/05/17.
 * @flow
 */

import { EventEmitter } from 'fbemitter';

export default class OSPConnector extends EventEmitter {

  constructor(serverUrl) {
    super();
    this.serverUrl = serverUrl.replace('http', 'ws').replace('https', 'wss') + '/mainws';
    this._initWS();
    this.isConnected = false;
  }

  _initWS() {
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
    this.isConnected = true;
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
        case "dirlist":
          this.handleDirList(data);
          break;
      }
    } catch (e) {
      console.log(`OSPConnector -- Error parsing message: ${e}`);
    }
  }

  onError(event) {
    this.emit('consoleMessage', ["ERROR", `There was an WebSocket error: ${JSON.stringify(event)}`]);
    console.log(`OSPConnector -- Error: ${event}`);
  }

  onClose(event) {
    this.emit('consoleMessage', ["INFO", "Connection to the server has been closed. Trying again in 2 seconds."]);
    this.emit('wsDisconnected');
    this.isConnected = false;
    setTimeout(() => {
      this._initWS();
    }, 2000);
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

  handleDirList(data) {
    this.emit('loadStatus', false);
    this.emit('dirList', data.Listing);
  }

  listDir(path) {
    console.log(`List dir path: ${path}`);
    this.emit('loadStatus', true);
    this.ws.send(JSON.stringify({
      type: 'dirlist',
      path,
    }));
  }

  on(...args) {
    this.addListener(...args);
  }
}
