/**
 * Created by Lucas Teske on 24/05/17.
 * @flow
 */
import React, { Component } from 'react';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import { Grid, Row, Col } from 'react-flexbox-grid';
import Chart from 'chart.js';
import moment from 'moment';

import {
  // Viterbi / RS Errors [3]
  blueGrey200,
  blueGrey700,

  // RS Errors [2]
  lightBlue200,
  lightBlue700,

  // RS Errors [1]
  lightGreen200,
  lightGreen700,

  // RS Errors [0]
  amber200,
  amber700,
} from 'material-ui/styles/colors'

const MAXVITPOINTS = 32;

class Charts extends Component {

  constructor(props) {
    super(props);
    this.viterbiData = {
      data: [],
      labels: [],
    };

    this.rsData = {
      rsData0: [],
      rsData1: [],
      rsData2: [],
      rsData3: [],
      labels:  [],
    };

    this.packets = {
      labels: [],
      data: [],
      packets: {}
    };

    this.refresh = false;
    this.props.ospConn.on('statistics', (data) => {
      this.handleStatistics(data);
    });
  }

  handleStatistics(data) {
    // Packets
    if (data.frameLock && data.virtualChannelID !== 63) {
      if (this.packets.packets[data.virtualChannelID] === undefined) {
        this.packets.packets[data.virtualChannelID] = {
          index: this.packets.labels.length,
        };
        this.packets.labels.push(data.virtualChannelID);
        this.packets.data.push(1);
      } else {
        const idx = this.packets.packets[data.virtualChannelID].index;
        this.packets.data[idx]++;
      }

      if (this.refresh) {
        this.packetsChart.update();
      }
    }

    // Viterbi
    if (this.viterbiData.labels.length > MAXVITPOINTS) {
      this.viterbiData.labels.splice(0, 1);
      this.viterbiData.data.splice(0, 1);
    }

    this.viterbiData.labels.push(moment().format('hh:mm:ss'));
    this.viterbiData.data.push(data.viterbiErrors);

    if (this.refresh) {
      this.vitChart.update();
    }
    this.viterbiData.currAvg = 0;
    this.viterbiData.currSample = 0;

    // RS Errors
    if (this.rsData.labels.length > MAXVITPOINTS) {
      this.rsData.labels.splice(0, 1);
      this.rsData.rsData0.splice(0, 1);
      this.rsData.rsData1.splice(0, 1);
      this.rsData.rsData2.splice(0, 1);
      this.rsData.rsData3.splice(0, 1);
    }

    this.rsData.labels.push(moment().format('hh:mm:ss'));
    this.rsData.rsData0.push(data.reedSolomon[0]);
    this.rsData.rsData1.push(data.reedSolomon[1]);
    this.rsData.rsData2.push(data.reedSolomon[2]);
    this.rsData.rsData3.push(data.reedSolomon[3]);

    if (this.refresh) {
      this.rsChart.update();
    }
  }

  componentDidMount() {
    this.refresh = true;
    this.props.setTitle('Charts');

    // Viterbi Chart
    const vitCtx = this.refs.viterbiCnv.getContext('2d');
    this.vitChart = new Chart(vitCtx, {
      type: 'line',
      data: {
        labels: this.viterbiData.labels,
        datasets: [
          {
            label: 'Viterbi Corrections',
            data: this.viterbiData.data,
            pointStyle: 'line',
            fill: true,
            lineTension: 0.2,
            backgroundColor: blueGrey200,
            borderColor: blueGrey700,
            borderCapStyle: 'butt',
            borderDash: [],
            borderDashOffset: 0.0,
            borderJoinStyle: 'miter',
            pointBorderColor: blueGrey700,
            pointBackgroundColor: '#fff',
            pointBorderWidth: 1,
            pointRadius: 1,
            pointHitRadius: 10,
          }
        ]
      },
      options: {
        tooltips: {enabled: false},
        hover: {mode: null},
        responsive: true,
        legend: {
          position: 'top',
        },
        title: {
          display: true,
          text: 'Viterbi Corrections'
        }
      }
    });

    // Packets Chart
    const packetsCtx = this.refs.packetsCnv.getContext('2d');
    this.packetsChart = new Chart(packetsCtx, {
      type: 'bar',
      data: {
        labels: this.packets.labels,
        datasets: [
          {
            label: 'Receive Packets',
            data: this.packets.data,
            backgroundColor: blueGrey200,
          }
        ]
      },
      options: {
        tooltips: {enabled: false},
        hover: {mode: null},
        responsive: true,
        legend: {
          position: 'top',
        },
        title: {
          display: true,
          text: 'Received Packets'
        }
      }
    });

    // RS Charts
    const rsCtx = this.refs.rsCnv.getContext('2d');
    this.rsChart = new Chart(rsCtx, {
      type: 'line',
      data: {
        labels: this.rsData.labels,
        datasets: [
          {
            label: 'CH0',
            data: this.rsData.rsData0,
            pointStyle: 'line',
            fill: true,
            lineTension: 0.2,
            backgroundColor: amber200,
            borderColor: amber700,
            borderCapStyle: 'butt',
            borderDash: [],
            borderDashOffset: 0.0,
            borderJoinStyle: 'miter',
            pointBorderColor: amber700,
            pointBackgroundColor: '#fff',
            pointBorderWidth: 1,
            pointRadius: 1,
            pointHitRadius: 10,
          },
          {
            label: 'CH1',
            data: this.rsData.rsData1,
            pointStyle: 'line',
            fill: true,
            lineTension: 0.2,
            backgroundColor: lightGreen200,
            borderColor: lightGreen700,
            borderCapStyle: 'butt',
            borderDash: [],
            borderDashOffset: 0.0,
            borderJoinStyle: 'miter',
            pointBorderColor: lightGreen700,
            pointBackgroundColor: '#fff',
            pointBorderWidth: 1,
            pointRadius: 1,
            pointHitRadius: 10,
          },
          {
            label: 'CH2',
            data: this.rsData.rsData2,
            pointStyle: 'line',
            fill: true,
            lineTension: 0.2,
            backgroundColor: lightBlue200,
            borderColor: lightBlue700,
            borderCapStyle: 'butt',
            borderDash: [],
            borderDashOffset: 0.0,
            borderJoinStyle: 'miter',
            pointBorderColor: lightBlue700,
            pointBackgroundColor: '#fff',
            pointBorderWidth: 1,
            pointRadius: 1,
            pointHitRadius: 10,
          },
          {
            label: 'CH3',
            data: this.rsData.rsData3,
            pointStyle: 'line',
            fill: true,
            lineTension: 0.2,
            backgroundColor: blueGrey200,
            borderColor: blueGrey700,
            borderCapStyle: 'butt',
            borderDash: [],
            borderDashOffset: 0.0,
            borderJoinStyle: 'miter',
            pointBorderColor: blueGrey700,
            pointBackgroundColor: '#fff',
            pointBorderWidth: 1,
            pointRadius: 1,
            pointHitRadius: 10,
          }
        ]
      },
      options: {
        tooltips: {enabled: false},
        hover: {mode: null},
        responsive: true,
        legend: {
          position: 'top',
        },
        title: {
          display: true,
          text: 'Reed Solomon'
        }
      }
    });

  }

  componentWillUnmount () {
    this.refresh = false;
  }

  render() {
    return (
      <MuiThemeProvider>
        <div className="Charts">
          <Grid fluid>
            <Row>
              <Col xs style={{display: 'flex', justifyContent: 'center', marginBottom: 15}}>
                <div style={{display: 'flex', justifyContent: 'center', width: 500, height: 250 }}>
                  <canvas ref="viterbiCnv" width="500" height="250"/>
                </div>
              </Col>
              <Col xs style={{display: 'flex', justifyContent: 'center', marginBottom: 15}}>
                <div style={{display: 'flex', justifyContent: 'center', width: 500, height: 250 }}>
                  <canvas ref="rsCnv" width="500" height="250"/>
                </div>
              </Col>
            </Row>
            <Row>
              <Col xs style={{display: 'flex', justifyContent: 'center', marginBottom: 15}}>
                <div style={{display: 'flex', justifyContent: 'center', width: 500, height: 250 }}>
                  <canvas ref="packetsCnv" width="500" height="250"/>
                </div>
              </Col>
            </Row>
          </Grid>
        </div>
      </MuiThemeProvider>
    );
  }
}

export default Charts;
