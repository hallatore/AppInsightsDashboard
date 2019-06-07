import * as React from 'react';
import styled from 'styled-components';
import Item, { DashboardItem } from './Item';

const DashboardGroup = styled.li`
    display: block;
    color: #555;
    margin-bottom: 5px;
`;

const Title = styled.h2`
    font-size: 16px;
    font-weight: 400;
    margin: 0;
    padding: 0;
    padding: 2px 5px;
    display: flex;
    justify-content: flex-end;
    flex-direction: column;
`;

const Items = styled.div`
    list-style: none;
    padding: 0;
    margin: 0;
    display: flex;
    justify-content: space-between;
`;

export interface DashboardGroup {
    name: string;
    items: DashboardItem[];
}

interface Props {
    dashboardId: string;
    group: DashboardGroup;
    groupIndex: number;
    className?: string;
}

export default class Group extends React.Component<Props> {
    render() {
        const { dashboardId, group, groupIndex, className } = this.props;

        return (
            <DashboardGroup className={className}>
                <Title>{group.name}</Title>
                <Items>
                    {group.items.map((item, itemIndex) =>
                        <Item key={itemIndex} dashboardId={dashboardId} item={item} groupIndex={groupIndex} itemIndex={
itemIndex}/>
                    )}
                </Items>
            </DashboardGroup>
        );
    }
}