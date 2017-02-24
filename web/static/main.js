(function() {

  window.serverUrl = "ws://" + window.location.host + "/mainws";

  var consoleMessages = [];
  var maxConsoleMessages = 10;
  var lastLockState = false;
  var lastSatteliteBusy = false;
  var constellationCtx;
  var constellationCanvas;
  var constellationPoints = [];

  function init() {
    consoleWrite("INFO", "Starting websocket");

    setLedColor("led-frame-lock", "red");
    setLedColor("led-websocket", "red");
    setLedColor("led-satellite-busy", "red");

    var ws = new WebSocket(window.serverUrl);

    ws.onopen    = function (e) { onOpen(e);    }
    ws.onmessage = function (e) { onMessage(e); }
    ws.onerror   = function (e) { onError(e);   }
    ws.onclose   = function (e) { onClose(e);   }

    constellationCanvas = $("#constellation")[0];
    constellationCtx = constellationCanvas.getContext("2d");
    refreshConstellation();
  }

  function refreshConstellation() {
    var ctx = constellationCtx;
    var width = constellationCanvas.width;
    var height = constellationCanvas.height;
    ctx.clearRect(0, 0, width, height);
    // Draw grid
    ctx.strokeStyle = 'rgba(190, 190, 190, 1)';
    ctx.moveTo(0, height / 2);
    ctx.lineTo(width, height / 2);
    ctx.stroke();
    ctx.moveTo(width / 2, 0);
    ctx.lineTo(width / 2, height);
    ctx.stroke();

    // Draw points
    ctx.strokeStyle = 'rgba(0, 0, 0, 1)';
    for (var i=0; i<1024; i+=2) {
        // Comes flipped
        var Q = (constellationPoints[i + 0] * height) / 2;
        var I = (constellationPoints[i + 1] * width) / 2;

        var x = I + width / 2;
        var y = Q + height / 2;
        ctx.beginPath();
        ctx.arc(~~x, ~~y, 2, 0, 2 *Math.PI);
        ctx.stroke();
    }
  }

  function onOpen(event) {
    consoleWrite("INFO", "Connected to Server");
    setLedColor("led-websocket", "green");
  }

  function setLedColor(led, color) {
    var colorClass = "led-blue";
    switch (color) {
      case "blue":
        colorClass = "led-blue";
        break;
      case "yellow":
        colorClass = "led-yellow";
        break;
      case "green":
        colorClass = "led-green";
        break;
      case "red":
        colorClass = "led-red";
        break;
    }

    $("#" + led).attr('class', colorClass);
  }

  function onMessage(event) {
    var data = JSON.parse(event.data);
    switch (data.DataType) {
      case "console":
        handleConsole(data);
        break;
      case "statisticsData":
        handleStatistics(data);
        break;
      case "constellationData":
        handleConstellation(data);
        break;
    }
  }

  function onError(event) {
    consoleWrite("ERROR", "There was an WebSocket error: " + event);
    console.log("Error: ", event);
  }

  function onClose(event) {
    consoleWrite("INFO", "Connection to the server has been closed.");
    setLedColor("led-websocket", "red");
  }

  function handleStatistics(data) {
    $("#field_scid").html(data.satelliteID);
    $("#field_vcid").html(data.virtualChannelID);
    $("#field_packetnumber").html(data.packetNumber);
    $("#field_framebits").html(data.totalBits);
    $("#field_viterrors").html(data.viterbiErrors);
    $("#field_signalquality").html(data.signalQuality);
    $("#field_synccorrelation").html(data.syncCorrelation);
    $("#field_phasecorrection").html(data.phaseCorrection);
    $("#field_rserror0").html(data.reedSolomon[0]);
    $("#field_rserror1").html(data.reedSolomon[1]);
    $("#field_rserror2").html(data.reedSolomon[2]);
    $("#field_rserror3").html(data.reedSolomon[3]);
    $("#field_runningtime").html(data.runningTime);
    $("#field_syncword").html(data.syncWord);
    if (data.frameLock != lastLockState) {
      lastLockState = data.frameLock;
      setLedColor("led-frame-lock", lastLockState ? "green" : "red");
    }

    var satBusy = data.virtualChannelID != 63;
    if (satBusy != lastSatteliteBusy) {
      lastSatteliteBusy = satBusy;
      setLedColor("led-satellite-busy", lastSatteliteBusy ? "green" : "red");
    }
  }

  function handleConstellation(data) {
    constellationPoints = data.data;
    refreshConstellation();
  }

  function handleConsole(data) {
    consoleWrite(data.level, data.message);
  }

  function consoleClear() {
    $("#console-text").html("");
  }

  function consoleWrite(level, message) {
    var textColor = "blue";
    var logFunc = console.log;
    switch(level) {
      case "INFO":
        textColor = "blue";
        logFunc = console.log;
        break;
      case "WARN":
        textColor = "yellow";
        if (console.hasOwnProperty("warn")) {
          logFunc = console.warn;
        }
        break;
      case "ERROR":
        textColor = "red";
        if (console.hasOwnProperty("error")) {
          logFunc = console.error;
        }
        break;
      case "DEBUG":
        textColor = "brown";
        if (console.hasOwnProperty("debug")) {
          logFunc = console.debug;
        }
        break;
      default:
        level = "INFO";
        logFunc = console.log;
        break;
    }

    var now = new Date();

    message = now.toLocaleTimeString() + "/" + level + " " + message;

    logFunc(message);
    if (consoleMessages.length >= maxConsoleMessages) {
      consoleMessages.shift();
    }

    consoleMessages.push('<span style="color: ' + textColor + '">' + message + '</span><BR/>');
    refreshConsoleUI();
  }

  function refreshConsoleUI() {
    consoleClear();
    for (var i in consoleMessages) {
      $("#console-text").append(consoleMessages[i]);
    }
  }

  if (!String.prototype.format) {
    String.prototype.format = function() {
      var args = arguments;
      return this.replace(/{(\d+)}/g, function(match, number) {
        return typeof args[number] != 'undefined'
          ? args[number]
          : match
        ;
      });
    };
  }

  window.addEventListener ("load", init, false);
})();