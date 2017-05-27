/**
 * Created by Lucas Teske on 27/05/17.
 * @flow
 */
import React, { Component } from 'react';
import '../Components/Led.css';

import GPSNotFixed from 'react-material-icons/icons/device/gps-not-fixed';
import GPSFixed from 'react-material-icons/icons/device/gps-fixed';
import PortableWifiOff from 'react-material-icons/icons/communication/portable-wifi-off';
import WifiTethering from 'react-material-icons/icons/device/wifi-tethering';
import Cached from 'react-material-icons/icons/action/cached';

import Wifi0Icon from 'react-material-icons/icons/device/signal-wifi-0-bar';
import Wifi1Icon from 'react-material-icons/icons/device/signal-wifi-1-bar';
import Wifi2Icon from 'react-material-icons/icons/device/signal-wifi-2-bar';
import Wifi3Icon from 'react-material-icons/icons/device/signal-wifi-3-bar';
import Wifi4Icon from 'react-material-icons/icons/device/signal-wifi-4-bar';
import ReactTooltip from 'react-tooltip'

import '../App.css';

const iconStyle = {
  marginRight: 15,
};

const divStyle = {
  display: 'flex',
  alignItems: 'center',
  height: 50,
  marginRight: 15,
};

class BarElementRight extends Component {

  constructor(props) {
    super(props);

    this.state = {
      signalQuality: 0,
      frameLock: false,
      websocket: this.props.ospConn.isConnected,
      satelliteBusy: false,
      loading: false,
    };

    this.props.ospConn.on('wsConnected', () => this.handleConnection(true));
    this.props.ospConn.on('wsDisconnected', () => this.handleConnection(false));
    this.props.ospConn.on('statistics', this.handleStatistics.bind(this));
    this.props.ospConn.on('loadStatus', this.handleLoading.bind(this));
  }

  handleLoading(status) {
    this.setState({
      loading: status,
    });
  }

  handleStatistics(data) {
    if (this.refresh) {
      console.log(data.frameLock);
      this.setState({
        frameLock: data.frameLock,
        satelliteBusy: data.virtualChannelID !== 63,
        signalQuality: data.signalQuality,
      });
    }
  }

  handleConnection(connected) {
    if (this.refresh) {
      this.setState({
        websocket: connected,
        frameLock: connected ? this.state.frameLock : false,
        satelliteBusy: connected ? this.state.satelliteBusy : false,
      });
    }
  }

  componentDidMount() {
    this.refresh = true;
  }

  componentWillUnmount () {
    this.refresh = false;
  }

  render() {
    let signalLevel = [<Wifi0Icon style={iconStyle}/>];
    const signal = this.state.signalQuality;

    if (signal > 90) {
      signalLevel = [<Wifi4Icon style={iconStyle}/>];
    } else if (signal > 70) {
      signalLevel = [<Wifi3Icon style={iconStyle}/>];
    } else if (signal > 50) {
      signalLevel = [<Wifi2Icon style={iconStyle}/>];
    } else if (signal > 40) {
      signalLevel = [<Wifi1Icon style={iconStyle}/>];
    }

    return (
      <div>
        <div style={divStyle}>
          <p data-tip data-for="signalQuality">
          {signalLevel}
          </p>
          <p data-tip data-for="frameLockStatus">
          {
            this.state.frameLock ? (<GPSFixed style={iconStyle}/>) : (<GPSNotFixed style={iconStyle}/>)
          }
          </p>
          <p data-tip data-for="websocketStatus">
          {
            this.state.websocket ? (<WifiTethering style={iconStyle}/>) : (<PortableWifiOff style={iconStyle}/>)
          }
          </p>
          <p data-tip data-for="syncIcon">
          {
            this.state.loading ? (<Cached className="rotation-fast"/>) : (<Cached/>)
          }
          </p>
          <ReactTooltip id='signalQuality' aria-haspopup='true' place="bottom">
            <b>Signal Quality Icon</b><br/>
            This represents the signal quality
          </ReactTooltip>
          <ReactTooltip id='frameLockStatus' aria-haspopup='true' place="bottom">
            <b>FrameLock Icon</b><br/>
            This represents the frame lock status
          </ReactTooltip>
          <ReactTooltip id='websocketStatus' aria-haspopup='true' place="bottom">
            <b>WebSocket Icon</b><br/>
            This represents the connection with the server
          </ReactTooltip>
          <ReactTooltip id='syncIcon' aria-haspopup='true' place="bottom">
            <b>Syncing Icon</b><br/>
            This will rotate when something is loading.
          </ReactTooltip>
        </div>
      </div>
    )
  }
}

export default BarElementRight;