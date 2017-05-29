/**
 * Created by Lucas Teske on 24/05/17.
 * @flow
 */
import React, { Component } from 'react';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import {Card, CardHeader, CardText} from 'material-ui/Card';
import moment from 'moment';
import { Line } from 'rc-progress';
import { Grid, Row, Col } from 'react-flexbox-grid';
import {
  lightGreenA200,
  limeA200,
  orangeA200,
  deepOrangeA200,
  redA200,
  blueGrey400,
} from 'material-ui/styles/colors'

import Constellation from '../Components/Constellation';

import '../App.css';
import '../Components/Led.css';

class Dashboard extends Component {

  constructor(props) {
    super(props);

    this.state = {
      statistics: {
        satelliteID: 0,
        virtualChannelID: 0,
        packetNumber: 0,
        totalBits: 0,
        viterbiErrors: 0,
        signalQuality: 0,
        syncCorrelation: 0,
        phaseCorrection: 0,
        reedSolomon: [0,0,0,0],
        syncWord: "00000000",
        frameLock: true,
        startTime: "2017-05-06T19:49:49-03:00",
        runningTime: "0:0:00.0000000",
      },
      frameLock: false,
      websocket: false,
      satelliteBusy: false,
    };

    this.constellation = null;
    this.refresh = false;
    this.props.ospConn.on('wsConnected', () => {
      if (this.refresh) {
        this.setState({websocket: true})
      }
    });
    this.props.ospConn.on('wsDisconnected', () => {
      if (this.refresh) {
        this.setState({
          websocket: false,
          frameLock: false,
          satelliteBusy: false,
        });
      }
    });
    this.props.ospConn.on('statistics', (data) => {
      if (this.refresh) {
        this.setState({
          statistics: data,
          frameLock: data.frameLock,
          satelliteBusy: data.virtualChannelID !== 63,
        });
      }
    });
    this.props.ospConn.on('constellation', (data) => {
      if (this.constellation !== null) {
        this.constellation.refreshCanvas(data);
      }
    });
  }

  componentDidMount() {
    this.constellation = this.refs.constellation;
    this.refresh = true;
    this.props.setTitle('Dashboard');
  }

  componentWillUnmount () {
    this.refresh = false;
  }

  _signalToColor(signal) {
    if (signal < 60) {
      return redA200;
    } else if (signal < 70) {
      return deepOrangeA200;
    } else if (signal < 80) {
      return orangeA200;
    } else if (signal < 90) {
      return limeA200;
    } else {
      return lightGreenA200;
    }
  }

  _ledClass(color) {
    let colorClass;

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
      default:
        colorClass = "led-blue";
        break;
    }

    return colorClass;
  }

  render() {
    const runTime = moment.duration(this.state.statistics.runningTime);
    return (
      <MuiThemeProvider>
        <div className="Dashboard">
          <Grid fluid>
            <Row>
              <Col xs style={{display: 'flex', justifyContent: 'center', marginBottom: 15}}>
                <Card style={{width: 340, height: 340}}>
                  <CardHeader
                    title="Statistics"
                    titleStyle={{fontSize: 20}}
                    titleColor={'#FFFFFF'}
                    className="card-header"
                  />
                  <CardText>
                    <ul className="statistics-ul">
                      <li className="statistics">
                        SC ID: <div className="statistics-val">{this.state.statistics.satelliteID}</div>
                      </li>
                      <li className="statistics">
                        VC ID: <div className="statistics-val">{this.state.statistics.virtualChannelID}</div>
                      </li>
                      <li className="statistics">
                        Packet Number: <div className="statistics-val">{this.state.statistics.packetNumber}</div>
                      </li>
                      <li className="statistics">
                        Viterbi Errors: <div className="statistics-val">{this.state.statistics.viterbiErrors} / {this.state.statistics.totalBits}</div>
                      </li>
                      <li className="statistics">
                        Signal Quality: <div className="statistics-val">{this.state.statistics.signalQuality}%</div>
                        <Line
                          percent={this.state.statistics.signalQuality}
                          strokeWidth="4" strokeColor={this._signalToColor(this.state.statistics.signalQuality)}
                          trailColor={blueGrey400}/>
                      </li>
                      <li className="statistics">
                        Sync Correlation: <div className="statistics-val">{this.state.statistics.syncCorrelation}</div>
                      </li>
                      <li className="statistics">
                        Phase Shift: <div className="statistics-val">{this.state.statistics.phaseCorrection}</div>
                      </li>
                      <li className="statistics">
                        Running Time: <div className="statistics-val">{runTime.as('days') | 0}d {runTime.get('hours')}h {runTime.get('minutes')}m {runTime.get('seconds')}s</div>
                      </li>
                      <li className="statistics">
                        Reed Solomon: <div className="statistics-val">{this.state.statistics.reedSolomon[0]} {this.state.statistics.reedSolomon[1]} {this.state.statistics.reedSolomon[2]} {this.state.statistics.reedSolomon[3]}</div>
                      </li>
                      <li className="statistics">
                        Sync Word: <div className="statistics-val">{this.state.statistics.syncWord}</div>
                      </li>
                    </ul>
                  </CardText>
                </Card>
              </Col>
              <Col xs style={{display: 'flex', justifyContent: 'center', marginBottom: 15}}>
                <Card style={{width: 340, height: 340}}>
                  <CardHeader
                    title="Constellation"
                    titleStyle={{fontSize: 20}}
                    titleColor={'#FFFFFF'}
                    className="card-header"
                  />
                  <br/>
                  <div style={{textAlign:'center'}}>
                    <Constellation ref="constellation"/>
                  </div>
                </Card>
              </Col>
              <Col xs style={{display: 'flex', justifyContent: 'center', marginBottom: 15}}>
                <Card style={{width: 340, height: 340}}>
                  <CardHeader
                    title="Status"
                    titleStyle={{fontSize: 20}}
                    titleColor={'#FFFFFF'}
                    className="card-header"
                  />
                  <br/>
                  <div style={{textAlign:'center'}}>
                    <div className="led-block">
                      <div className="led-block-item">
                        <div className="led-box">
                          <div className={this.state.frameLock ? this._ledClass('green') : this._ledClass('red')}/>
                          <p>Frame Lock</p>
                        </div>
                        <div className="led-box">
                          <div className={this.state.websocket ? this._ledClass('green') : this._ledClass('red')}/>
                          <p>WebSocket</p>
                        </div>
                        <div className="led-box">
                          <div className={this.state.satelliteBusy ? this._ledClass('green') : this._ledClass('red')}/>
                          <p>Satellite Busy</p>
                        </div>
                      </div>
                    </div>
                  </div>
                </Card>
              </Col>
            </Row>
          </Grid>
        </div>
      </MuiThemeProvider>
    );
  }
}

export default Dashboard;
