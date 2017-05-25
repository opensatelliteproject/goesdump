/**
 * Created by Lucas Teske on 24/05/17.
 * @flow
 */

import React, { Component } from 'react';

class Constellation extends Component {

  constructor(props) {
    super(props);
    this.points = [];
    this.refreshCanvas = this.refreshCanvas.bind(this);
  }

  componentDidMount() {
    this.canvas = this.refs.canvas;
    this.ctx = this.canvas.getContext("2d");
    this.refreshCanvas();
  }

  refreshCanvas(points) {
    if (points !== undefined) {
      this.points = points;
    }
    const size = this.props.size;
    const numPoints = this.points.length;
    this.ctx.clearRect(0, 0, size, size);
    // Draw grid
    this.ctx.strokeStyle = 'rgba(190, 190, 190, 1)';
    this.ctx.moveTo(0, size / 2);
    this.ctx.lineTo(size, size / 2);
    this.ctx.stroke();
    this.ctx.moveTo(size / 2, 0);
    this.ctx.lineTo(size / 2, size);
    this.ctx.stroke();

    // Draw points
    this.ctx.strokeStyle = 'rgba(0, 0, 0, 1)';
    for (let i = 0; i < numPoints; i += 2) {
      // Comes flipped
      const Q = (this.points[i] * size) / 2;
      const I = (this.points[i + 1] * size) / 2;

      const x = I + size / 2;
      const y = Q + size / 2;

      this.ctx.beginPath();
      this.ctx.arc(~~x, ~~y, 2, 0, 2 * Math.PI);
      this.ctx.stroke();
    }
  }

  render() {
    return (
      <canvas
        width={this.props.size}
        height={this.props.size}
        ref="canvas">
        Your browser does not support canvas.
      </canvas>
    )
  }
}

Constellation.defaultProps = {
  size: 240,
};

export default Constellation;