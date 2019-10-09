import * as React from 'react';
import { Route } from 'react-router';
import HomePage from './components/home/HomePage';
import DashboardPage from './components/dashboard/DashboardPage';
import ItemPage from './components/item/ItemPage';

export default () => (
    <React.Fragment>
        <Route exact path="/" component={HomePage}/>
        <Route exact path="/:dashboardId([0-9a-z-]{36})" component={DashboardPage}/>
        <Route exact path="/:dashboardId([0-9a-z-]{36})/Item/:groupIndex([0-9]+)/:itemIndex([0-9]+)" component={ItemPage}/>
    </React.Fragment>
);