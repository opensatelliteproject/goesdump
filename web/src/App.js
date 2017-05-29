import React, { Component } from 'react';
import injectTapEventPlugin from 'react-tap-event-plugin';
import { Route, BrowserRouter, Link } from 'react-router-dom';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import {
  Drawer,
  MenuItem,
  Paper,
}from 'material-ui';

import DashBoardIcon from 'react-material-icons/icons/action/dashboard';
import ChartsIcon from 'react-material-icons/icons/device/data-usage';
import ConsoleIcon from 'react-material-icons/icons/hardware/computer';
import ExplorerIcon from 'react-material-icons/icons/file/folder';
import ConfigurationsIcon from 'react-material-icons/icons/action/settings';

import './App.css';
import Charts from './Controllers/Charts';
import Dashboard from './Controllers/Dashboard';
import Console from './Controllers/Console';
import Explorer from './Controllers/Explorer';
import Configuration from './Controllers/Configuration';
import OSPConnector from './OSP/Connector';
import TopBar from './Components/TopBar';

injectTapEventPlugin();
const serverUrl = "http://" + window.location.host;
//const serverUrl = 'http://localhost:8090';
const conn = new OSPConnector(serverUrl);
const MAXLINES = 15;

class App extends Component {

  constructor(props) {
    super(props);
    this.state = {
      sideMenuOpen: false,
      title: 'Dashboard',
    };

    this.setTitle = this.setTitle.bind(this);
    this.getMessageCache = this.getMessageCache.bind(this);

    this.messageCache = [];

    conn.on('consoleMessage', ([ level, message ]) => {
      const currentPath = this.refs.router.history.location.pathname;
      if (currentPath !== '/console') {
        this._messageCacheUpdate(level, message);
      }
    });
  }

  _messageCacheUpdate(level, message) {
    const now = new Date();
    let textColor;

    if (this.messageCache.length > MAXLINES) {
      this.messageCache.splice(0, 1);
    }

    switch(level) {
      case "INFO":
        textColor = "blue";
        break;
      case "WARN":
        textColor = "yellow";
        break;
      case "ERROR":
        textColor = "red";
        break;
      case "DEBUG":
        textColor = "brown";
        break;
      default:
        textColor = "blue";
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

  setTitle = (title) => this.setState({ title });
  handleToggle = () => this.setState({ sideMenuOpen: !this.state.sideMenuOpen });
  closeDrawer = () => this.setState({ sideMenuOpen: false });

  clearConsoleNotification = () => this.refs.topBar.clearConsoleNotification();
  clearChartsNotification = () => this.refs.topBar.clearChartsNotification();
  clearDashboardNotification = () => this.refs.topBar.clearDashboardNotification();

  render() {
    return (
      <MuiThemeProvider>
        <div>
          <BrowserRouter ref="router">
            <div>
              <div id="pageHeader">
                <TopBar ref="topBar" ospConn={conn} title={this.state.title} router={this.refs.router} toggleDrawer={this.handleToggle}/>
                <Drawer
                  open={this.state.sideMenuOpen}
                  docked={false}
                  onRequestChange={this.closeDrawer}
                >
                  <div style={{display: 'flex', flexDirection: 'column'}}>
                    <Paper style={{display: 'flex', justifyContent: 'center', paddingTop: 25, paddingBottom: 25}} zDepth={1}>
                      <img src="/android-icon-72x72.png" alt="OSP Logo"/>
                    </Paper>
                    <Link to="/" style={{ textDecoration: 'none' }}>
                      <MenuItem onTouchTap={() => { this.closeDrawer(); this.clearDashboardNotification(); }}><DashBoardIcon className="menu-icon"/>Dashboard</MenuItem>
                    </Link>
                    <Link to="/charts">
                      <MenuItem onTouchTap={() => { this.closeDrawer(); this.clearChartsNotification(); }}><ChartsIcon className="menu-icon"/>Charts</MenuItem>
                    </Link>
                    <Link to="/console">
                      <MenuItem onTouchTap={() => { this.closeDrawer(); this.clearConsoleNotification(); }}><ConsoleIcon className="menu-icon"/>Console</MenuItem>
                    </Link>
                    <Link to="/explorer">
                      <MenuItem onTouchTap={() => { this.closeDrawer(); }}><ExplorerIcon className="menu-icon"/>Explorer</MenuItem>
                    </Link>
                    <Link to="/config">
                      <MenuItem onTouchTap={() => { this.closeDrawer(); }}><ConfigurationsIcon className="menu-icon"/>Configurations</MenuItem>
                    </Link>
                    <div className="footer">
                      Made by <a target="_blank" rel="noopener noreferrer" href="https://github.com/racerxdl">Lucas Teske</a><br/>
                      Available at: <a target="_blank" rel="noopener noreferrer" href="https://github.com/opensatelliteproject/">Github</a>
                    </div>
                  </div>
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
                  <Route path="/config" render={(props) => { return <Configuration serverUrl={serverUrl} setTitle={this.setTitle} ospConn={conn} {...props} /> }}/>
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
