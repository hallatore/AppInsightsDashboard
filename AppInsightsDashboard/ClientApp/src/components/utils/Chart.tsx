import * as React from 'react';
import styled from 'styled-components';

const ChartCanvas = styled.canvas`
    display: block;
    width: auto;
`;

export default class Chart extends React.Component<{
    width: string,
    height: string,
    chartValues: number[],
    chartMax: number,
    style?: object | undefined,
    color?: string | undefined
}> {

    componentDidMount() {
        this.drawChart();
    }

    componentDidUpdate() {
        this.drawChart();
    }

    private drawChart() {
        const canvas = this.chartRef.current;
        const { chartValues, chartMax, color } = this.props;
        this.renderChart(canvas, chartValues, chartMax, color || '#ccc');
    }

    private renderChart(canvas: HTMLCanvasElement | null, chartValues: number[], chartMax: number, color: string) {
        if (canvas == null) {
            return;
        }

        if (canvas.getContext) {
            const ctx = canvas.getContext('2d');

            if (ctx == null) {
                return;
            }

            ctx.clearRect(0, 0, canvas.width, canvas.height);
            ctx.fillStyle = color;

            if (chartValues.length === 0) {
                return;
            }

            const spacing = 0.5;
            const width = canvas.width / chartValues.length;
            const height = canvas.height;

            for (let i = 0; i < chartValues.length; i++) {
                const itemHeight = height / chartMax * chartValues[i];
                ctx.fillRect(i * width, height - itemHeight, Math.max(width - spacing, spacing), itemHeight);
            }
        }
    }

    private chartRef = React.createRef<HTMLCanvasElement>();

    render() {
        return (<ChartCanvas {...this.props} ref={this.chartRef}/>);
    }
}