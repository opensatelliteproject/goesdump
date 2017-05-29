/**
 * Created by Lucas Teske on 28/05/17.
 * @flow
 */
import 'classlist-polyfill';
import React, { Component } from 'react';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import { Grid, Row, Col } from 'react-flexbox-grid';
import Paper from 'material-ui/Paper';
import TextField from 'material-ui/TextField';
import Toggle from 'material-ui/Toggle';
import IconButton from 'material-ui/IconButton';
import SaveIcon from 'react-material-icons/icons/action/done';

const styles = {
  block: {
    maxWidth: 250,
  },
  checkbox: {
    marginBottom: 16,
  },
  colStyle: {
    display: 'flex',
    justifyContent: 'center',
    flexDirection: 'column',
  },
  valColStyle: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'flex-end',
  },
  rowStyle: {
    minHeight: 60,
    border: '1px dashed #EEEEEE',
  },
  valDivStyle: {
    height: '100%',
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'center',
    marginLeft: 'auto',
    marginRight: 'auto',
  }
};

class Configuration extends Component {

  constructor(props) {
    super(props);
    this.props.ospConn.on('configList', this.handleConfigReception.bind(this));
    this.state = {
      data: {},
    };

    this.cache = {};
    this.onChangeField = this.onChangeField.bind(this);
  }

  handleConfigReception(data) {
    console.log('Received config list');
    if (this.refresh) {
      this.setState({
        data,
      });
    }
  }

  componentDidMount() {
    this.refresh = true;
    this.props.setTitle('Configuration');
    if (this.props.ospConn.isConnected) {
      this.props.ospConn.listConfig();
    } else {
      this.props.ospConn.on('wsConnected', () => {
        this.props.ospConn.listConfig();
      });
    }
  }

  onChangeField(event, value) {
    const id = event.target.id;
    console.log(`Property ${id} changed to ${value}`);
    this.cache[id] = value;
    if (typeof value === 'boolean') {
      this.props.ospConn.updateConfig(id, value);
    }
  }

  onSaveClick(field) {
    const val = this.cache[field];
    if (val === undefined) {
      console.log(`Field ${field} not changed.`);
    } else {
      console.log(`Saving ${field}: ${val}`);
      this.props.ospConn.updateConfig(field, val.toString());
    }
  }

  componentWillUnmount () {
    this.refresh = false;
  }

  render() {
    let configs = [];
    const keys = Object.keys(this.state.data);
    for (let i = 0; i < keys.length; i++) {
      const key = keys[i];
      const entry = this.state.data[key];
      switch (entry.Type.toLowerCase()) {
        case 'int32':
          configs.push(
            <Row key={key} style={styles.rowStyle}>
              <Col xs={3} style={styles.colStyle}>
                <b>{entry.Name}:</b>
              </Col>
              <Col xs={2} style={styles.valColStyle}>
                <TextField
                  id={entry.Name}
                  defaultValue={entry.Value}
                  onChange={this.onChangeField}
                  type="number"
                />
              </Col>
              <Col xs={1}>
                <IconButton
                  tooltip="Save"
                  onClick={() => {this.onSaveClick(entry.Name);}}
                  style={{marginLeft: 15}}
                >
                  <SaveIcon/>
                </IconButton>
              </Col>
              <Col xs={4} style={styles.colStyle}>
                {entry.Description}
              </Col>
              <Col xs={2} style={styles.colStyle}>
                <i>{entry.DefaultValue.toString()}</i>
              </Col>
            </Row>
          );
          break;
        case 'string':
          configs.push(
            <Row key={key} style={styles.rowStyle}>
              <Col xs={3} style={styles.colStyle}>
                <b>{entry.Name}:</b>
              </Col>
              <Col xs={2} style={styles.valColStyle}>
                <TextField
                  id={entry.Name}
                  defaultValue={entry.Value}
                  onChange={this.onChangeField}
                />
              </Col>
              <Col xs={1}>
                <IconButton
                  tooltip="Save"
                  onClick={() => {this.onSaveClick(entry.Name);}}
                  style={{marginLeft: 15}}
                >
                  <SaveIcon/>
                </IconButton>
              </Col>
              <Col xs={4} style={styles.colStyle}>
                {entry.Description}
              </Col>
              <Col xs={2} style={styles.colStyle}>
                <i>{entry.DefaultValue.toString()}</i>
              </Col>
            </Row>
          );
          break;
        case 'boolean':
          configs.push(
            <Row key={key} style={styles.rowStyle}>
              <Col xs={3} style={styles.colStyle}>
                <b>{entry.Name}:</b>
              </Col>
              <Col xs={3} style={styles.valColStyle}>
                <div style={styles.valDivStyle}>
                  <Toggle
                    id={entry.Name}
                    defaultToggled={entry.Value} style={{width: 50}}
                    onToggle={this.onChangeField}
                  />
                </div>
              </Col>
              <Col xs={4} style={styles.colStyle}>
                {entry.Description}
              </Col>
              <Col xs={2} style={styles.colStyle}>
                <Toggle
                  toggled={entry.DefaultValue}
                  style={{width: 50}}
                />
              </Col>
            </Row>
          );
          break;
        default:
          console.log(`Configuration -- Unknown field type: ${entry.Type}`);
          break;
      }
    }
    return (
      <MuiThemeProvider>
        <div className="Configuration" style={{display: 'flex', justifyContent: 'center'}}>
          <Paper zDepth={2} style={{width: 1200, paddingTop: 25, paddingBottom: 15}}>
            <Grid fluid>
              <Row>
                <Col xs={2}>
                  <b>Name</b>
                </Col>
                <Col xs style={{textAlign: 'center'}}>
                  <b>Value</b>
                </Col>
                <Col xs={4}>
                  <b>Description</b>
                </Col>
                <Col xs={2}>
                  <b>Default Value</b>
                </Col>
              </Row>
              <Row>
                <Col xs>
                  <br/>
                </Col>
              </Row>
              {configs}
            </Grid>
          </Paper>
        </div>
      </MuiThemeProvider>
    );
  }
}

export default Configuration;
