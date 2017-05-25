/**
 * Created by Lucas Teske on 24/05/17.
 * @flow
 */
import React, { Component } from 'react';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import logo from '../logo.svg';

class Charts extends Component {

  constructor(props) {
    super(props);
  }

  render() {
    return (
      <MuiThemeProvider>
        <div className="Charts">
          CHARTS
          <h2>Charts</h2>
        </div>
      </MuiThemeProvider>
    );
  }
}

export default Charts;
