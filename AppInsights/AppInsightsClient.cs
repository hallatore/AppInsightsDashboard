using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AppInsights
{
    public static class AppInsightsClient
    {
        // https://dev.applicationinsights.io/
        private const string QueryUrl = "https://api.applicationinsights.io/v1/apps/{0}/query";

        private static readonly ConcurrentDictionary<string, HttpClient> HttpClients = new ConcurrentDictionary<string, HttpClient>();

        private static HttpClient GetHttpClient(string apiKey)
        {
            return HttpClients.GetOrAdd(
                apiKey,
                key =>
                {
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("x-api-key", key);
                    return client;
                });
        }

        public static async Task<TableResult> GetTableQuery(Guid appid, string apikey, string query)
        {
            return (await GetTablesQuery(appid, apikey, query)).First();
        }

        public static async Task<List<TableResult>> GetTablesQuery(Guid appid, string apikey, params string[] queries)
        {
            var httpClient = GetHttpClient(apikey);
            var url = string.Format(QueryUrl, appid);
            var jsonRequest = JsonConvert.SerializeObject(new { query = queries[0] });
            var response = await httpClient.PostAsync(url, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
            var responseJson = await response.Content.ReadAsStringAsync();
            var queryResult = JObject.Parse(responseJson);
            var result = new List<TableResult>();

            foreach (var table in queryResult["tables"])
            {
                var tableResult = new TableResult();
                var columns = table["columns"];
                var rows = table["rows"];

                foreach (var column in columns)
                {
                    var columnType = Enum.TryParse<QueryResultType>(column["type"].Value<string>(), true, out var queryResultType)
                        ? queryResultType
                        : QueryResultType.String;
                    tableResult.Columns.Add(new TableResultColumn(column["name"].Value<string>(), columnType));
                }

                foreach (var row in rows)
                {
                    var rowResult = new List<dynamic>();

                    for (var i = 0; i < row.Count(); i++)
                    {
                        switch (tableResult.Columns[i].Type)
                        {
                            default:
                            case QueryResultType.String:
                                rowResult.Add(row[i].Value<string>());
                                break;
                            case QueryResultType.Long:
                                rowResult.Add(row[i].Value<string>() != "NaN" ? row[i].Value<long>() : 0);
                                break;
                            case QueryResultType.Real:
                                rowResult.Add(row[i].Value<string>() != "NaN" ? row[i].Value<double>() : 0);
                                break;
                            case QueryResultType.Datetime:
                                rowResult.Add(row[i].Value<DateTime>());
                                break;
                        }
                    }

                    tableResult.Rows.Add(rowResult);
                }

                result.Add(tableResult);
            }

            return result;
        }
    }

    public class TableResult
    {
        public TableResult()
        {
            Columns = new List<TableResultColumn>();
            Rows = new List<List<dynamic>>();
        }

        public List<TableResultColumn> Columns { get; set; }
        public List<List<dynamic>> Rows { get; set; }
    }

    public class TableResultColumn
    {
        public TableResultColumn(string name, QueryResultType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; set; }
        public QueryResultType Type { get; }
    }

    public enum QueryResultType
    {
        String,
        Long,
        Real,
        Datetime
    }
}