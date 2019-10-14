import * as React from 'react';
import styled from 'styled-components';
import Loader from '../utils/Loader';
import { number } from 'prop-types';

const QueryButtonContainer = styled.div`
    position: absolute;
    top: calc(50% - 10px);
    right: -5px;
    display: none;

    @media(max-width: 1024px) {
        display: block;
    }
`;

const QueryButton = styled.button`
    border: 0;
    height: 20px;
    width: 20px;
    padding: 0;
    text-align: center;
    font-family: 'Roboto', sans-serif;
    font-size: 18px;
    line-height: 0px;
    cursor: pointer;
    background: #222;
    color: #fff;
    margin-right: 7px;
    border-radius: 50%;
    width: 20px;
    border: 1px solid #777;

    &:hover {
        border-color: #fff;
        background: #fff;
        color: #000;
    }
`;

const Table = styled.table`
    font-size: 14px;
    line-height: 1.3;
    margin: 0 -15px;
    margin-bottom: -15px;
    width: calc(100% + 30px);
`;

const TableHead = styled.th`
    font-weight: normal;
    padding: 5px;
    text-align: right;

    &:first-child {
        padding-left: 15px;
        text-align: left;
    }

    &:last-child {
        padding-right: 15px;
    }

    @media(max-width: 1024px) {
        font-size: 9px;
    }
`;

const TableRow = styled.tr`    
    color: #999;

    &:nth-child(odd) {
        background: #1f1f1f;
    }

    &:hover {
        color: #fff;
        background: #151515;
    }

    &:hover ${QueryButtonContainer} {
        display: block;
    }
`;

const TableCell = styled.td`
    border-top: 1px solid #333;
    font-size: 11px;
    line-height: 140%;
    padding: 5px;
    text-align: right;
    position: relative;

    &:first-child {
        padding-left: 15px;
        text-align: left;
        word-break: break-all;
        white-space: pre-line;
        width: 100%;
    }

    &:last-child {
        padding-right: 15px;
    }
`;

interface Props {
    url: string;
    queryTimestamp: Number;
    addCallback: any;
}

interface State {
    isLoading: boolean;
    queryTimestamp: Number;
    items: any;
}

export default class AnalyzerTable extends React.Component<Props, State> {
    constructor(props: Props) {
        super(props);

        this.state = {
            isLoading: false,
            queryTimestamp: 0,
            items: null
        };
    }

    componentDidMount() {
        this.ensureDataFetched();
    }

    componentDidUpdate() {
        this.ensureDataFetched();
    }

    render() {
        const { addCallback } = this.props;
        const { items, isLoading } = this.state;

        if (items == null) {
            return null;
        }

        return (
            <React.Fragment>
                {isLoading && <Loader/>}
                <Table cellSpacing="0" style={{ opacity: isLoading ? 0.3 : 1 }}>
                    <thead>
                    <tr>
                        {items.table.columns.map(
                            (column: any, columnIndex: number) =>
                            <TableHead key={columnIndex}>{column.name}</TableHead>)}
                    </tr>
                    </thead>
                    <tbody>
                    {items.table.rows.map((row: any, rowIndex: number) =>
                        <TableRow key={rowIndex}>
                            {items.table.columns.map(
                                (column: any, columnIndex: number) =>
                                <TableCell key={columnIndex}>
                                    {columnIndex === 0 &&
                                        (row.length - items.table.columns.length) === 2 &&
                                        <QueryButtonContainer>
                                            <QueryButton title={row[row.length - 2]} onClick={() => addCallback(
                                                row[row.length - 2])}>+</QueryButton>
                                            <QueryButton title={row[row.length - 1]} onClick={() => addCallback(
                                                row[row.length - 1])}>-</QueryButton>
                                        </QueryButtonContainer>}
                                    {row[columnIndex]}
                                </TableCell>)}
                        </TableRow>)}
                    </tbody>
                </Table>
            </React.Fragment>
        );
    }

    private ensureDataFetched() {
        if (this.props.queryTimestamp === this.state.queryTimestamp) {
            return;
        }

        this.setState({
            isLoading: true,
            queryTimestamp: this.props.queryTimestamp
        });

        fetch(this.props.url)
            .then(response => response.json() as Promise<any>)
            .then(data => this.setState({ items: data, isLoading: false }))
            .catch(error => this.setState({ items: null, isLoading: false }));
    }
}