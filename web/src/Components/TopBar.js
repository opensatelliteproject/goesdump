/**
 * Created by Lucas Teske on 28/05/17.
 * @flow
 */

import React, { Component } from 'react';
import Badge from 'material-ui/Badge';
import {
  AppBar,
}from 'material-ui';

import atom from '../atom.svg';
import '../App.css';
import BarElementRight from './BarElementRight';

class TopBar extends Component {

  constructor(props) {
    super(props);

    this.state = {
      consoleNotifications: 0,
      chartsNotifications: 0,
      dashboardNotifications: 0,
    };

    this.notifyCharts = this.notifyCharts.bind(this);
    this.notifyConsole = this.notifyConsole.bind(this);
    this.notifyDashboard = this.notifyDashboard.bind(this);

    this.props.ospConn.on('consoleMessage', ([ level, message ]) => {
      const currentPath = this.props.router.history.location.pathname;
      if ((level === 'ERROR' || level === 'WARN') && currentPath !== '/console') {
        this.notifyConsole();
      }
    });
  }

  notifyConsole() {
    this.setState({ consoleNotifications: this.state.consoleNotifications + 1 });
  }

  notifyDashboard() {
    this.setState({ dashboardNotifications: this.state.dashboardNotifications + 1 });
  }

  notifyCharts() {
    this.setState({ chartsNotifications: this.state.chartsNotifications + 1 });
  }

  clearConsoleNotification() {
    this.setState({ consoleNotifications: 0 });
  }

  clearDashboardNotification() {
    this.setState({ dashboardNotifications: 0 });
  }

  clearChartsNotification() {
    this.setState({ chartsNotifications: 0 });
  }

  componentDidMount() {
    this.refresh = true;
  }

  componentWillUnmount () {
    this.refresh = false;
  }

  render() {
    const totalNotifications = this.state.consoleNotifications + this.state.chartsNotifications + this.state.dashboardNotifications;
    return (
      <AppBar
        title={<span className="clickable">Open Satellite Project - {this.props.title}</span>}
        onLeftIconButtonTouchTap={this.props.toggleDrawer}
        onTitleTouchTap={this.props.toggleDrawer}
        iconElementLeft={
          <Badge
            badgeContent={totalNotifications}
            secondary={true}
            badgeStyle={{top: 0, right: 0,  display: totalNotifications > 0 ? 'flex' : 'none'}}
            style={{height: 60, padding: '0 0 0 0', marginRight: 20, top: 0 }}
          >
            <img src={atom} alt="Menu Icon" className="drawer-icon" width={64} height={64} />
          </Badge>
        }
        iconElementRight={<BarElementRight ospConn={this.props.ospConn}/>}
      />
    )
  }
}

export default TopBar;