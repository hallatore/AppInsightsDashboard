import * as React from 'react';
import { Link } from 'react-router-dom'
import styled from 'styled-components';
import Chart from '../utils/Chart';

const ItemLink = styled(Link)<{status: ItemStatus}>`
    display: block;
    padding: 10px;
    padding-bottom: 0;
    margin-right: 15px;
    margin-bottom: 15px;
    position: relative;
    background: #222;
    color: #555;
    text-decoration: none;
    flex-grow: 1;
    min-width: 170px;
        
    @media(max-width: 500px) {
        min-width: auto;
        max-width: 50%;
    }

    &:hover {
        background: #333;
        color: white;
        cursor: pointer;
    }

    &:last-child {
        margin-right: 0;
        
        @media(max-width: 1024px) {
            margin-right: 10px;
        }
    }

    color: ${props => props.status === ItemStatus.Disabled ? '#333' : null};
    color: ${props => props.status === ItemStatus.Warning ? 'yellow' : null};
    color: ${props => props.status === ItemStatus.Error ? 'white' : null};
    animation: ${props => props.status === ItemStatus.Error ? 'BlinkAnimation 2s infinite' : null};

    @keyframes BlinkAnimation {
        0%, 33% {
            background-color: red;
            color: white;
        }
    }
`;

const ItemTitle = styled.h3`
    font-size: 18px;
    font-weight: 400;
    padding: 0;
    margin: 0;
    overflow: hidden;
    white-space: nowrap;
    position: absolute;
    top: 5px;
    left: 0;
    right: 0;
    text-align: center;
`;

const Value = styled.div`
    font-size: 48px;
    line-height: 1;
    white-space: nowrap;
    position: absolute;
    bottom: 5px;
    left: 0;
    right: 0;
    text-align: center;
    text-shadow: 0px 1px 3px #222;
`;

const ItemChart = styled(Chart) < { status: ItemStatus } >`
    opacity: 0.4;
    margin-top: 20px;
    opacity: ${props => props.status === ItemStatus.Warning ? 1 : null};
    opacity: ${props => props.status === ItemStatus.Error ? 1 : null};
    width: 100%;
    height: 50px;
`;

const ValuePostfix = styled.span`
    font-size: 18px;
    font-weight: 300;
    padding-left: 3px;
`;

const GroupTitle = styled.div`
    position: absolute;
    top: -13px;
    color: #555;
    font-size: 10px;
    text-transform: uppercase;
`;

const GroupTitleGray = styled(GroupTitle)`    
    color: #222;
`;

export interface DashboardItem {
    name: string;
    postfix: string;
}

enum ItemStatus {
    Normal,
    Disabled,
    Warning,
    Error
}

interface Props {
    dashboardId: string;
    item: DashboardItem;
    groupName: string;
    groupIndex: number;
    itemIndex: number;
}

interface State {
    isLoading: boolean;
    value: string;
    chartValues: number[];
    chartMax: number;
    status: ItemStatus;
    intervalId?: NodeJS.Timeout;
}

export default class Item extends React.Component<Props, State> {
    public constructor(props: Props) {
        super(props);

        this.state = {
            isLoading: false,
            value: '',
            chartValues: [],
            chartMax: 0,
            status: ItemStatus.Normal
        };
    }

    componentDidMount() {
        this.ensureDataFetched();
        const intervalId = setInterval(this.ensureDataFetched.bind(this), 120000 + (30000 * Math.random()));
        this.setState({ intervalId: intervalId });
    }

    componentWillUnmount() {
        if (this.state.intervalId != null) {
            clearInterval(this.state.intervalId);
        }
    }

    render() {
        const { dashboardId, item, groupIndex, groupName, itemIndex } = this.props;
        const { value, status, chartValues, chartMax } = this.state;

        return (
            <ItemLink status={status} to={`/${dashboardId}/Item/${groupIndex}/${itemIndex}`}>
                {itemIndex == 0 ? <GroupTitle>{groupName}</GroupTitle> : <GroupTitleGray>{groupName}</GroupTitleGray>}
                <ItemChart width="150" height="50" status={status} color="#555" chartValues={chartValues} chartMax={chartMax}/>
                <ItemTitle>{item.name}</ItemTitle>
                <Value>
                    {value}<ValuePostfix>{item.postfix}</ValuePostfix>
                </Value>
            </ItemLink>
        );
    }

    private ensureDataFetched() {
        const { dashboardId, groupIndex, itemIndex } = this.props;
        this.setState({ isLoading: true });

        fetch(`/api/Dashboard/${dashboardId}/Overview/${groupIndex}/${itemIndex}`)
            .then(response => response.json() as Promise<any>)
            .then(data => {
                this.setState({
                    value: data.value,
                    chartValues: data.chartValues,
                    chartMax: data.chartMax,
                    status: data.status,
                    isLoading: false
                });
            })
            .catch(error => this.setState({
                isLoading: false,
                value: '',
                chartValues: [],
                chartMax: 0,
                status: ItemStatus.Normal
            }));
    }
}