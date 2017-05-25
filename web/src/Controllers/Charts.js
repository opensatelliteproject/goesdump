/**
 * Created by Lucas Teske on 24/05/17.
 * @flow
 */
import React, { Component } from 'react';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';

class Charts extends Component {

  constructor(props) {
    super(props);
  }

  componentDidMount() {
    this.constellation = this.refs.constellation;
    this.refresh = true;
    this.props.setTitle('Charts');
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
