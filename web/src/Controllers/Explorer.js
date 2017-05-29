/**
 * Created by Lucas Teske on 27/05/17.
 * @flow
 */
import 'classlist-polyfill';
import React, { Component } from 'react';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import { Link } from 'react-router-dom';
import moment from 'moment';

const hrsSuffix = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];

const logN = (v, b) => {
  return Math.log(v)/(b ? Math.log(b) : 1);
};

const BytesToString = (byteCount) => {

  if (byteCount === 0) {
    return "0" + hrsSuffix[0];
  }

  const bytes = Math.abs(byteCount);
  const place = Math.floor(logN(bytes, 1024));
  const num = Math.round(bytes / Math.pow(1024, place));
  return (Math.sign(byteCount) * num).toString() + hrsSuffix[place];
};

class Explorer extends Component {

  constructor(props) {
    super(props);
    this.props.ospConn.on('dirList', this.handleListReception.bind(this));
    this.state = {
      data: [],
      curPath: '',
      nextPath: this.props.location.pathname.replace('/explorer', '') ,
    };
  }

  handleListReception(data) {
    if (this.refresh) {
      this.setState({
        data,
        curPath: this.state.nextPath,
      });
      this.props.setTitle(`Explorer - ${this.state.nextPath}`);
    }
  }

  componentDidMount() {
    this.refresh = true;
    this.props.setTitle('Explorer');
    if (this.props.ospConn.isConnected) {
      this.switchFolder(this.state.nextPath);
    } else {
      this.props.ospConn.on('wsConnected', () => {
        this.switchFolder(this.state.nextPath);
      });
    }

    window.onpopstate = this.onBackButtonEvent.bind(this);
  }
  onBackButtonEvent(e) {
    e.preventDefault();
    this.switchFolder(this.props.location.pathname.replace('/explorer', ''));
  }

  componentWillUnmount () {
    this.refresh = false;
  }

  switchFolder(path) {
    console.log(`Switching folder to ${path}`);
    this.setState({
      nextPath: path,
    });
    this.props.ospConn.listDir(path);
  }

  render() {
    let files = [];
    for (let i = 0; i < this.state.data.length; i++) {
      const entry = this.state.data[i];
      files.push(
        <tr key={`${i}-${entry.Path}`}>
          <td><img alt={entry.IsFile ? "File Icon" : "Folder Icon"} src={entry.IsFile ? "/file.gif" : "/folder.gif"}/></td>
          <td>
            {
              !entry.IsFile ?
                (<Link to={`/explorer/${entry.Path}`} onClick={() => { this.switchFolder(`/${entry.Path}`)}}>{entry.Name}</Link>) :
                (<a target="_blank" rel="noopener noreferrer" href={`${this.props.serverUrl}/data/${entry.Path}`}>{entry.Name}</a>)
            }
          </td>
          <td>{entry.LastModified !== -1 ? moment(entry.LastModified*1000).toString() : '-'}</td>
          <td>{entry.Size === -1 ? '-' : BytesToString(entry.Size)}</td>
        </tr>
      );
    }
    return (
      <MuiThemeProvider>
        <div className="Explorer" style={{display: 'flex', justifyContent: 'center'}}>
          <table style={{minWidth: 800}}>
            <thead>
            <tr>
              <th>
                <img src="/blank.gif" alt="Blank Icon"/>
              </th>
              <th style={{textAlign: 'left', minWidth: 200}}>Name</th>
              <th style={{textAlign: 'left'}}>Last modified</th>
              <th style={{textAlign: 'left'}}>Size</th>
            </tr>
            </thead>
            <tbody>
            <tr>
              <td colSpan={4}><hr/></td>
            </tr>
            {files}
            </tbody>
          </table>
        </div>
      </MuiThemeProvider>
    );
  }
}

export default Explorer;
