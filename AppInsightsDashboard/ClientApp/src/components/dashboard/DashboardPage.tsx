import * as React from 'react';
import styled from 'styled-components';
import { RouteComponentProps } from 'react-router';
import Item, { DashboardItem } from './Item';

const Page = styled.div`
    overflow-x: hidden;
`;

const Items = styled.div`
    display: flex;
    justify-content: stretch;
    flex-wrap: wrap;
    margin-right: -10px;

    @media(max-width: 1024px) {
        padding: 10px;
    }
`;

const Spacer = styled.div`
    flex-grow: 100;

    @media(max-width: 500px) {
        display: none;
    }
`;

interface DashboardGroup {
    name: string;
    items: DashboardItem[];
}

type Props = RouteComponentProps<{ dashboardId: string }>;

interface State {
    groups: DashboardGroup[];
    isLoading: boolean;
    intervalId?: NodeJS.Timeout;
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
        const intervalId = setInterval(this.ensureDataFetched.bind(this), 10 * 60 * 1000);
        this.setState({ intervalId: intervalId });
    }

    componentWillUnmount() {
        if (this.state.intervalId != null) {
            clearInterval(this.state.intervalId);
        }
    }

    render() {
        const { groups } = this.state;

        return (
            <Page>
                <Items>
                    {groups.map((group, groupIndex) =>
                        group.items.map((item, itemIndex) =>
                            <Item
                                key={`${groupIndex}_${itemIndex}`}
                                dashboardId={this.props.match.params.dashboardId}
                                item={item}
                                groupName={group.name}
                                groupIndex={groupIndex}
                                itemIndex={itemIndex} />
                        )
                    )}
                    <Spacer />
                </Items>
            </Page>
        );
    }

    private ensureDataFetched() {
        this.setState({ isLoading: true });

        fetch(`/api/Dashboard/${this.props.match.params.dashboardId}/`)
            .then(response => response.json() as Promise<DashboardGroup[]>)
            .then(data => {
                this.setState({ groups: data, isLoading: false });
            })
            .catch(error => this.setState({
                isLoading: false,
                groups: []
            }));
    }
}