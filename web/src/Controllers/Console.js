/**
 * Created by Lucas Teske on 25/05/17.
 * @flow
 */
import 'classlist-polyfill';
import React, { Component } from 'react';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import { Card, CardHeader, CardText } from 'material-ui/Card';

const MAXLINES = 15;

class Console extends Component {

  constructor(props) {
    super(props);
    this.props.ospConn.on('consoleMessage', this.handleConsoleMessage.bind(this));
    this.state = {
      messages: [],
    }
  }

  handleConsoleMessage(data) {
    if (this.refresh) {
      const msgs = this.state.messages;
      const now = new Date();
      let textColor;

      if (msgs.length > MAXLINES) {
        msgs.splice(0, 1);
      }

      switch (data[0]) {
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

      const message = `${now.toLocaleTimeString()}/${String("       " + data[0]).slice(-5)} ${data[1]}`;

      msgs.push({
        color: textColor,
        message,
      });

      this.setState({messages: msgs});
    }
  }

  componentDidMount() {
    this.constellation = this.refs.constellation;
    this.refresh = true;
    this.props.setTitle('Console');
    this.setState({
      messages: this.props.messageCache(),
    });
  }

  componentWillUnmount () {
    this.refresh = false;
  }

  render() {
    let messages = [];
    for (let i = 0; i < this.state.messages.length; i++) {
      const msg = this.state.messages[i];
      messages.push(
        <span key={`consoleMessage/${i}`} style={{ color: msg.color }}>{msg.message}<br/></span>
      );
    }

    return (
      <MuiThemeProvider>
        <div className="Console">
          <Card className="console-card">
            <CardHeader
              title="Console"
              titleStyle={{fontSize: 20}}
              titleColor={'#FFFFFF'}
              className="card-header"
            />
            <CardText className="console-text">
              <div className="console-area">
                {messages}
              </div>
            </CardText>
          </Card>
        </div>
      </MuiThemeProvider>
    );
  }
}

export default Console;
