import * as React from 'react';
import styled from 'styled-components';
import { RouteComponentProps } from 'react-router';
import { Link } from 'react-router-dom'
import AnalyzerTable from './AnalyzerTable';
import Loader from '../utils/Loader';
import Chart, { ChartValue } from '../utils/GoogleLineChart';

const Container = styled.div`
    max-width: 1600px;
    color: #fff;
    margin: 0 auto;
    padding: 20px;
`;

const AreaContainer = styled.div`
    background: #222;
    padding: 15px;
    margin-bottom: 20px;
    position: relative;
    min-height: 26px;
`;

const SplitContainer = styled.div`
    display: flex;
`;

const MainSplitContainer = styled.div`
    width: 60%;
    margin-right: 20px;
`;

const SecondarySplitContainer = styled.div`
    flex: 1;
`;

const Header = styled.div`
    margin-bottom: 20px;
`;

const Title = styled.h1`
    font-weight: normal;
    margin: 40px 0 20px;
    font-size: 25px;
    line-height: 1;
`;

const DurationButton = styled.button < { duration: ItemDuration, currentDuration: ItemDuration } >
    `
    display: inline-block;
    margin-right: 2px;
    border: 0;
    padding: 7px 15px;
    font-family: 'Roboto', sans-serif;
    font-size: 14px;
    cursor: pointer;
    background: #555;
    color: #fff;

    &:hover {
        background: #777;
        color: #fff;

        background: ${props => props.duration === props.currentDuration ? '#fff' : null};
        color: ${props => props.duration === props.currentDuration ? '#000' : null};
    }

    background: ${props => props.duration === props.currentDuration ? '#fff' : null};
    color: ${props => props.duration === props.currentDuration ? '#000' : null};
`;

const QueryParts = styled.div`
    margin-bottom: 15px;
`;

const QueryPartButton = styled.button`
    display: block;
    width: 100%;
    text-align: left;
    margin-top: 2px;
    border: 0;
    padding: 7px 15px;
    word-break: break-all;
    cursor: pointer;
    background: #333;
    color: #fff;
    font-family: monospace;
    line-height: 1.3;
    font-size: 11px;

    &:hover {
        background: #777;
        color: #fff;
    }
`;

const Query = styled.div`
    white-space: pre-wrap;
    word-break: break-all;
    font-family: monospace;
    line-height: 1.3;
    font-size: 11px;
    color: #999;
`;

const ChartMax = styled.div`
    position: absolute;
    right: 15px;
    color: #999;
    font-size: 14px;
`;

const ItemChart = styled(Chart)`
    margin-top: 20px;
    border-bottom: 1px solid #555;
`;

const BrowseButton = styled.button`
    display: block;
    border: 0;
    padding: 15px;
    width: 200px;
    font-family: 'Roboto', sans-serif;
    font-size: 14px;
    cursor: pointer;
    background: #015cda;
    color: #fff;

    &:hover {
        background: #016afe;
        color: #fff;
    }
`;

const BackLink = styled(Link)`
    color: inherit;
    text-decoration: none;
`;

enum ItemDuration {
    OneHour = 0,
    SixHours = 1,
    TwelveHours = 2,
    OneDay = 3,
    ThreeDays = 4,
    SevenDays = 5,
    ThirtyDays = 6
}

interface State {
    isLoading: boolean;
    name: string,
    duration: ItemDuration;
    query: string;
    queryParts: string[];
    chartValues: ChartValue[];
    chartMax: number;
    count: number;
}

type Props = RouteComponentProps<{ dashboardId: string, groupIndex: string, itemIndex: string }>;

export default class ItemPage extends React.Component<Props, State> {
    constructor(props: Props) {
        super(props);

        const historyState = history.state || {};

        this.state = {
            isLoading: false,
            name: '',
            duration: historyState.duration || ItemDuration.OneHour,
            query: '',
            queryParts: historyState.queryParts || [],
            chartValues: [],
            chartMax: 0,
            count: 0
        };
    }

    componentDidMount() {
        this.ensureDataFetched();
    }

    render() {
        const { name, duration, query, queryParts, isLoading, chartValues, chartMax, count } = this.state;

        return (
            <Container>
                <Header>
                    <Title><BackLink to={`/${this.props.match.params.dashboardId}`}>Overview</BackLink> / {name}&nbsp;</Title>
                    <div>
                        <DurationButton duration={ItemDuration.OneHour} currentDuration={duration} onClick={() => this.updateDuration(ItemDuration.OneHour)}>1 hour</DurationButton>
                        <DurationButton duration={ItemDuration.SixHours} currentDuration={duration} onClick={() => this.updateDuration(ItemDuration.SixHours)}>6 hours</DurationButton>
                        <DurationButton duration={ItemDuration.TwelveHours} currentDuration={duration} onClick={() => this.updateDuration(ItemDuration.TwelveHours)}>12 hours</DurationButton>
                        <DurationButton duration={ItemDuration.OneDay} currentDuration={duration} onClick={() => this.updateDuration(ItemDuration.OneDay)}>1 day</DurationButton>
                        <DurationButton duration={ItemDuration.ThreeDays} currentDuration={duration} onClick={() => this.updateDuration(ItemDuration.ThreeDays)}>3 days</DurationButton>
                        <DurationButton duration={ItemDuration.SevenDays} currentDuration={duration} onClick={() => this.updateDuration(ItemDuration.SevenDays)}>7 days</DurationButton>
                        <DurationButton duration={ItemDuration.ThirtyDays} currentDuration={duration} onClick={() => this.updateDuration(ItemDuration.ThirtyDays)}>30 days</DurationButton>
                    </div>
                </Header>
                <SplitContainer>
                    <MainSplitContainer>
                        <AreaContainer>
                            {isLoading && <Loader />}
                            <ItemChart values={chartValues} max={chartMax} style={{ opacity: isLoading ? 0.3 : 1 }}/>
                        </AreaContainer>
                        <AreaContainer>
                            <AnalyzerTable url={this.getAnalyzerUrl('RequestsAnalyzer')} addCallback={(queryPart: string) => this.addCallback(queryPart)}/>
                        </AreaContainer>
                        <AreaContainer>
                            <AnalyzerTable url={this.getAnalyzerUrl('UrlAnalyzer')} addCallback={(queryPart: string) => this.addCallback(queryPart)}/>
                        </AreaContainer>
                    </MainSplitContainer>
                    <SecondarySplitContainer>
                        <AreaContainer>
                            {isLoading && <Loader />}
                            {queryParts.length > 0 && <QueryParts>
                                {queryParts.map((part, index) =>
                                    <QueryPartButton key={index} onClick={() => this.removeWhereQuery(part)}>{part}</QueryPartButton>)}
                            </QueryParts>}
                            <Query style={{ opacity: isLoading ? 0.3 : 1 }}>{query}</Query>
                        </AreaContainer>
                        <AreaContainer>
                            <AnalyzerTable url={this.getAnalyzerUrl('StatusCodesAnalyzer')} addCallback={(queryPart: string) => this.addCallback(queryPart)}/>
                        </AreaContainer>
                        <AreaContainer>
                            <AnalyzerTable url={this.getAnalyzerUrl('RequestExceptionsAnalyzer')} addCallback={(queryPart: string) => this.addCallback(queryPart)}/>
                        </AreaContainer>
                        <AreaContainer>
                            <AnalyzerTable url={this.getAnalyzerUrl('StacktraceAnalyzer')} addCallback={(queryPart: string) => this.addCallback(queryPart)}/>
                        </AreaContainer>
                        {false && count > 0 && !isLoading && <BrowseButton>{count} Operations</BrowseButton>}
                    </SecondarySplitContainer>
                </SplitContainer>
            </Container>
        );
    }

    private removeWhereQuery(queryPart: string) {
        const { queryParts } = this.state;
        const index = queryParts.findIndex((value) => value === queryPart);

        if (index === -1) {
            return;
        }

        queryParts.splice(index, 1);
        this.setState({ queryParts: queryParts }, () => this.ensureDataFetched());
    }

    private updateDuration(duration: ItemDuration) {
        this.setState({ duration: duration }, () => this.ensureDataFetched());
    }

    private ensureDataFetched() {
        this.saveState();
        this.setState({ isLoading: true });

        const dashboardId = this.props.match.params.dashboardId;
        const groupIndex = this.props.match.params.groupIndex;
        const itemIndex = this.props.match.params.itemIndex;
        const queryParts = this.state.queryParts.map(part => `&queryParts=${encodeURIComponent(part)}`).join('');

        fetch(`/api/Dashboard/${dashboardId}/Details/${groupIndex}/${itemIndex}?duration=${this.state.duration}${queryParts}`)
            .then(response => response.json() as Promise<any>)
            .then(data => {
                this.setState({
                    name: data.name,
                    chartValues: data.chartValues.map((item: { date: string; value: number; }) => { return { date: new Date(Date.parse(item.date)), value: item.value } }),
                    chartMax: data.chartMax,
                    query: data.query,
                    count: data.count,
                    isLoading: false
                });
            })
            .catch(error => this.setState({ isLoading: false }));
    }

    private saveState() {
        const state = {
            duration: this.state.duration,
            queryParts: this.state.queryParts
        };
        history.replaceState(state, document.title, location.href);
    }

    private getAnalyzerUrl(analyzerName: string): string {
        const dashboardId = this.props.match.params.dashboardId;
        const groupIndex = this.props.match.params.groupIndex;
        const itemIndex = this.props.match.params.itemIndex;
        const queryParts = this.state.queryParts.map(part => `&queryParts=${encodeURIComponent(part)}`).join('');
        return `/api/Dashboard/${dashboardId}/Analyzer/${groupIndex}/${itemIndex}/${analyzerName}?duration=${this.state.duration}${queryParts}`;
    }

    private addCallback(queryPart: string) {
        const { queryParts } = this.state;

        if (queryParts.findIndex((value) => value === queryPart) !== -1) {
            return;
        }

        queryParts.push(queryPart);
        this.setState({ queryParts: queryParts }, () => this.ensureDataFetched());
    }
}