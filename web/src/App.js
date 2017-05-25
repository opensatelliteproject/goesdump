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
import OSPConnector from './OSP/Connector';

injectTapEventPlugin();

const conn = new OSPConnector();

conn.on('consoleMessage', ([ level, message ]) => {
  //console.log(`${level} - ${message}`);
});

class App extends Component {

  constructor(props) {
    super(props);
    this.state = {
      sideMenuOpen: false,
      notifications: 0,
    };
  }

  notificationsUpdate(notifications) {
    this.setState({ notifications });
  }

  handleToggle = () => this.setState({ sideMenuOpen: !this.state.sideMenuOpen });
  closeDrawer = () => this.setState({ sideMenuOpen: false });

  render() {
    return (
      <MuiThemeProvider>
        <div>
          <BrowserRouter>
            <div>
              <div id="pageHeader">
                <AppBar
                  title={<span className="clickable">Open Satellite Project</span>}
                  onLeftIconButtonTouchTap={this.handleToggle}
                  onTitleTouchTap={this.handleToggle}
                  iconElementLeft={
                    <Badge
                      badgeContent={this.state.notifications}
                      secondary={true}
                      badgeStyle={{top: 0, right: 0,  display: this.state.notifications > 0 ? 'flex' : 'none'}}
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
                    <MenuItem onTouchTap={this.closeDrawer}>Dashboard</MenuItem>
                  </Link>
                  <Link to="/charts">
                    <MenuItem onTouchTap={this.closeDrawer}>Charts</MenuItem>
                  </Link>
                </Drawer>
              </div>
              <br/>
              <div id="pageContent" style={{display: 'flex', justifyContent: 'center'}}>
                <switch>
                  <Route exact path="/" render={(props) => { return <Dashboard ospConn={conn} {...props} /> }}/>
                  <Route path="/charts" component={Charts}/>
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
