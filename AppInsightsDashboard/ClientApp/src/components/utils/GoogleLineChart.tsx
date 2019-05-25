import * as React from 'react';
import { Chart } from 'react-google-charts';

export interface ChartValue {
    date: Date;
    value: number;
}

export default class GoogleLineChart extends React.Component<{
    values: ChartValue[],
    max: number,
    style?: object | undefined,
    color?: string | undefined;
}> {
    render() {
        const columns = [
            { type: 'date' },
            { type: 'number' },
            { role: 'style', type: 'string' },
            { role: 'tooltip', type: 'string' }
        ];

        const areaStyle = 'color: #999; fill-color: #333; stroke-width: 2; fill-opacity: 1';
        const rows = [];
        const nonNullData = this.props.values.filter(row => row.value !== null);

        for (let row of nonNullData) {
            const { date, value } = row;
            rows.push([date, value, areaStyle, `${value} - ${date.toLocaleString('en-GB').substring(12, 17)}`]);
        }

        const data = [columns, ...rows];

        return (
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
                        width: '100%',
                        height: '80%'
                    },
                    backgroundColor: 'transparent',
                    colors: ['#fff']
                }}/>
        );
    }
}