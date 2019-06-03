import * as React from 'react';
import styled from 'styled-components';
import { RouteComponentProps } from 'react-router';
import Group, { DashboardGroup } from './Group';

const itemSpacing = '25px';

const Groups = styled.ul`
    list-style: none;
    padding: 0;
    margin: 0;
    display: flex;
    flex-wrap: wrap;
    margin-right: -${itemSpacing};
`;

const GroupContainer = styled(Group)`
    margin-right: ${itemSpacing};
`;

type Props = RouteComponentProps<{ dashboardId: string }>;

interface State {
    groups: DashboardGroup[];
    isLoading: boolean;
}

export default class DashboardPage extends React.Component<Props, State> {
    public constructor(props: Props) {
        super(props);

        this.state = {
            isLoading: false,
            groups: []
        };
    }

    componentDidMount() {
        this.ensureDataFetched();
    }

    render() {
        const { groups } = this.state;

        return (
            <Groups>
                {groups.map((group, groupIndex) =>
                    <GroupContainer key={groupIndex} dashboardId={this.props.match.params.dashboardId} group={group
} groupIndex={groupIndex}/>
                )}
            </Groups>
        );
    }

    private ensureDataFetched() {
        this.setState({ isLoading: true });

        fetch(`/api/Dashboard/${this.props.match.params.dashboardId}/`)
            .then(response => response.json() as Promise<DashboardGroup[]>)
            .then(data => {
                this.setState({ groups: data, isLoading: false });
            });
    }
}