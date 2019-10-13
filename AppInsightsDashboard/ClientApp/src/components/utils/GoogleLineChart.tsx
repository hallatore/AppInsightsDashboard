import * as React from 'react';
import styled from 'styled-components';
import { Chart } from 'react-google-charts';

const ChartContainer = styled.div`
    position: relative;
`;

const TimelineSelectionVisualizer = styled.div`
    position: absolute;
    top: -15px;
    bottom: -15px;
    border: 1.4px solid #fff;
    border-width: 0 1.4px;
    background: rgba(255, 255, 255, 0.5);
    z-index: 1;    
    pointer-events: none;
`;

export interface ChartValue {
    date: Date;
    value: number;
}

interface Props {
    values: ChartValue[],
    max: number,
    style?: object | undefined,
    color?: string | undefined;
    onUpdateCustomDuration?: (from: Date, to: Date) => void | undefined;
}

interface State {
    selectedStartX: number;
    selectedEndX: number;
    isSelecting: boolean;
}

export default class GoogleLineChart extends React.Component<Props, State> {
    chartRef: React.RefObject<HTMLDivElement>;

    constructor(props: Props) {
        super(props);
        this.chartRef = React.createRef<HTMLDivElement>();

        this.state = {
            selectedStartX: 0,
            selectedEndX: 0,
            isSelecting: false
        };
    }

    private onMouseDown(e: any) {
        const clientRect = this.chartRef.current!.getBoundingClientRect();
        const x = e.clientX - clientRect.left;

        this.setState({
            selectedStartX: x,
            selectedEndX: x,
            isSelecting: true
        });
    }

    private isSettingSelectedEndX = false;

    private onMouseMove(e: any) {
        if (!this.state.isSelecting || this.props.onUpdateCustomDuration === undefined || this.isSettingSelectedEndX) {
            return;
        }

        this.isSettingSelectedEndX = true;
        const clientRect = this.chartRef.current!.getBoundingClientRect();
        const x = e.clientX - clientRect.left;
        
        this.setState({ selectedEndX: x });
        setTimeout(() => this.isSettingSelectedEndX = false, 1000 / 30);
    }

    private onMouseUp(e: any) {
        const clientRect = this.chartRef.current!.getBoundingClientRect();
        const selectedStartX = this.state.selectedStartX;
        const x = e.clientX - clientRect.left;

        this.setState({
            selectedStartX: 0,
            selectedEndX: 0,
            isSelecting: false
        });

        if (selectedStartX == x) {
            return;
        }

        const rows = this.getRows();
        const minX = Math.min(selectedStartX, x);
        const maxX = Math.max(selectedStartX, x);
        const minIndex = Math.max(Math.floor(minX / clientRect.width * rows.length), 0);
        const maxIndex = Math.min(Math.ceil(maxX / clientRect.width * rows.length), rows.length - 1);
        
        if (this.props.onUpdateCustomDuration) {
            this.props.onUpdateCustomDuration(rows[minIndex].date, rows[maxIndex].date);
        }
    }

    private onMouseLeave(e: any) {
        if (this.state.isSelecting) {
            this.setState({
                selectedStartX: 0,
                selectedEndX: 0,
                isSelecting: false
            });
        }        
    }

    private getRows() {
        return this.props.values.filter(row => row.value !== null);
    }
    
    render() {
        const columns = [
            { type: 'date' },
            { type: 'number' },
            { role: 'style', type: 'string' },
            { role: 'tooltip', type: 'string' }
        ];

        const areaStyle = 'color: #999; fill-color: #333; stroke-width: 2; fill-opacity: 1';
        const rows = [];
        const nonNullData = this.getRows();

        for (let row of nonNullData) {
            const { date, value } = row;
            rows.push([date, value, areaStyle, `${value} - ${date.toLocaleString('en-GB').substring(12, 17)}`]);
        }

        const data = [columns, ...rows];

        const timelineStyles = {            
            left: Math.min(this.state.selectedStartX, this.state.selectedEndX),
            width: Math.abs(this.state.selectedStartX - this.state.selectedEndX),
            display: this.state.selectedStartX === this.state.selectedEndX ? 'none' : 'block'
        }

        return (
            <ChartContainer 
                ref={this.chartRef} 
                onMouseDown={(e) => this.onMouseDown(e)} 
                onMouseMove={(e) => this.onMouseMove(e)} 
                onMouseUp={(e) => this.onMouseUp(e)} 
                onMouseLeave={(e) => this.onMouseLeave(e)}
                onMouseEnter={(e) => this.onMouseLeave(e)}
            >
                <TimelineSelectionVisualizer style={timelineStyles} />
                <Chart
                    chartType="AreaChart"
                    data={data}
                    options={{
                        tooltip: { isHtml: false },
                        hAxis: {
                            //format: "HH",
                            textStyle: { color: '#999' },
                            gridlines: {
                                color: 'transparent',
                                count: -1,
                                units: {
                                    days: { format: ['MMM dd'] },
                                    hours: { format: ['HH:mm'] },
                                    minutes: { format: ['HH:mm'] }
                                }
                            },
                            minorGridlines: {
                                units: {
                                    hours: { format: [''] },
                                    minutes: { format: [''] }
                                }
                            }
                        },
                        vAxis: {
                            //format: "long",
                            textStyle: { color: '#999' },
                            gridlines: { color: 'transparent' },
                            viewWindowMode: 'explicit',
                            viewWindow: {
                                max: this.props.max,
                                min: 0
                            }
                        },
                        legend: 'none',
                        height: 200,
                        chartArea: {
                            top: '0',
                            width: '100%',
                            height: '90%'
                        },
                        backgroundColor: 'transparent',
                        colors: ['#fff']
                    }}/
                >
            </ChartContainer>
        );
    }
}