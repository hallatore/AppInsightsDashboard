AppInsightsDashboard is a dashboard for Application Insights.

## Requirements

.NET Core 3.0

## Usage

1. Create configs
2. Deploy application
3. Go to `<Dashboard url>/<dashboard id>`

## Local development

To run the application locally, you need both the react app and the backend to run.
Run the frontend by navigating to `AppInsightsDashboard\ClientApp` and run `npm i` and `npm start`. It should start on `localhost:3000`.
Run the backend afterwards by running it in visual studio or from the command line in the folder `AppInsightsDashboard` with `dotnet run`.

### Configration

You need to create atleast one config for your project.

[Read more about configuration](AppInsightsDashboard/Configs/README.md)

## Screenshots

![Dashboard](https://i.imgur.com/WCb8KcS.png)

![Details](https://imgur.com/RbA2dU0.png)
