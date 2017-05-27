import React, { Component } from 'react';
import injectTapEventPlugin from 'react-tap-event-plugin';
import { Route, BrowserRouter, Link } from 'react-router-dom';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import Badge from 'material-ui/Badge';
import {
  AppBar,
  Drawer,
  MenuItem,
}from 'material-ui';

import atom from './atom.svg';
import './App.css';
import Charts from './Controllers/Charts';
import Dashboard from './Controllers/Dashboard';
import Console from './Controllers/Console';
import Explorer from './Controllers/Explorer';
import OSPConnector from './OSP/Connector';

injectTapEventPlugin();
//const serverUrl = "http://" + window.location.host;
const serverUrl = 'http://localhost:8090';
const conn = new OSPConnector(serverUrl);
const MAXLINES = 15;

class App extends Component {

  constructor(props) {
    super(props);
    this.state = {
      sideMenuOpen: false,
      consoleNotifications: 0,
      chartsNotifications: 0,
      dashboardNotifications: 0,
      title: 'Dashboard',
    };

    this.notifyCharts = this.notifyCharts.bind(this);
    this.notifyConsole = this.notifyConsole.bind(this);
    this.notifyDashboard = this.notifyDashboard.bind(this);
    this.setTitle = this.setTitle.bind(this);
    this.getMessageCache = this.getMessageCache.bind(this);

    this.messageCache = [];

    conn.on('consoleMessage', ([ level, message ]) => {
      const currentPath = this.refs.router.history.location.pathname;
      if ((level === 'ERROR' || level === 'WARN') && currentPath !== '/console') {
        this.notifyConsole();
      }

      if (currentPath !== '/console') {
        this._messageCacheUpdate(level, message);
      }
    });
  }

  _messageCacheUpdate(level, message) {
    const now = new Date();
    let logFunc;
    let textColor;

    if (this.messageCache.length > MAXLINES) {
      this.messageCache.splice(0, 1);
    }

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
        textColor = "blue";
        logFunc = console.log;
        break;
    }

    this.messageCache.push({
      color: textColor,
      message: `${now.toLocaleTimeString()}/${String("       " + level).slice(-5)} ${message}`,
    });
  }

  getMessageCache() {
    return this.messageCache;
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

  setTitle(title) {
    this.setState({ title });
  }

  handleToggle = () => this.setState({ sideMenuOpen: !this.state.sideMenuOpen });
  closeDrawer = () => this.setState({ sideMenuOpen: false });

  render() {
    const totalNotifications = this.state.consoleNotifications + this.state.chartsNotifications + this.state.dashboardNotifications;
    return (
      <MuiThemeProvider>
        <div>
          <BrowserRouter ref="router">
            <div>
              <div id="pageHeader">
                <AppBar
                  title={<span className="clickable">Open Satellite Project - {this.state.title}</span>}
                  onLeftIconButtonTouchTap={this.handleToggle}
                  onTitleTouchTap={this.handleToggle}
                  iconElementLeft={
                    <Badge
                      badgeContent={totalNotifications}
                      secondary={true}
                      badgeStyle={{top: 0, right: 0,  display: totalNotifications > 0 ? 'flex' : 'none'}}
                      style={{height: 60, padding: '0 0 0 0', top: 0 }}
                    >
                      <img src={atom} className="drawer-icon" width={64} height={64} />
                    </Badge>
                  }
                />
                <Drawer
                  open={this.state.sideMenuOpen}
                  docked={false}
                  onRequestChange={this.closeDrawer}
                >
                  <Link to="/" style={{ textDecoration: 'none' }}>
                    <MenuItem onTouchTap={() => { this.closeDrawer(); this.clearDashboardNotification(); }}>Dashboard</MenuItem>
                  </Link>
                  <Link to="/charts">
                    <MenuItem onTouchTap={() => { this.closeDrawer(); this.clearChartsNotification(); }}>Charts</MenuItem>
                  </Link>
                  <Link to="/console">
                    <MenuItem onTouchTap={() => { this.closeDrawer(); this.clearConsoleNotification(); }}>Console</MenuItem>
                  </Link>
                  <Link to="/explorer">
                    <MenuItem onTouchTap={() => { this.closeDrawer(); }}>Explorer</MenuItem>
                  </Link>
                </Drawer>
              </div>
              <br/>
              <div id="pageContent" style={{display: 'flex', justifyContent: 'center'}}>
                <switch style={{width: '100%', marginLeft: 15, marginRight: 15}}>
                  <Route exact path="/" render={(props) => { return <Dashboard setTitle={this.setTitle} notify={this.notifyDashboard} ospConn={conn} {...props} /> }}/>
                  <Route path="/charts" render={(props) => { return <Charts setTitle={this.setTitle} notify={this.notifyCharts} ospConn={conn} {...props} />}}/>
                  <Route path="/console" render={(props) => { return <Console setTitle={this.setTitle} notify={this.notifyConsole} messageCache={this.getMessageCache} ospConn={conn} {...props} /> }}/>
                  <Route exact path="/explorer" render={(props) => { return <Explorer setTitle={this.setTitle} ospConn={conn} {...props} /> }}/>
                  <Route path="/explorer/:folder" render={(props) => { return <Explorer serverUrl={serverUrl} setTitle={this.setTitle} ospConn={conn} {...props} /> }}/>
                </switch>
              </div>
            </div>
          </BrowserRouter>
        </div>
      </MuiThemeProvider>
    );
  }
}

export default App;
