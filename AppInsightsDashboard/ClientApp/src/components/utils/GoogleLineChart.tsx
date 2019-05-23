import * as React from "react";
import { Chart } from "react-google-charts";

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
            { type: "date" },
            { type: "number" },
            { role: "style", type: "string" }
        ];

        const areaStyle = "color: #999; fill-color: #333; stroke-width: 2; fill-opacity: 1";
        const rows = [];
        const nonNullData = this.props.values.filter(row => row.value !== null);

        for (let row of nonNullData) {
            const { date, value } = row;
            rows.push([date, value, areaStyle]);
        }

        const data = [columns, ...rows];

        return (
            <Chart
                chartType="AreaChart"
                data={data}
                options={{
                    hAxis: {
                        //format: "HH",
                        textStyle: { color: "#999" },
                        gridlines: { color: "transparent" }
                    },
                    vAxis: {
                        //format: "long",
                        textStyle: { color: "#999" },
                        gridlines: { color: "transparent" }
                    },
                    legend: "none",
                    height: 200,
                    chartArea: {
                        width: "100%",
                        height: "80%"
                    },
                    backgroundColor: "transparent",
                    colors: ["#fff"]
                }}/>
        );
    }
}