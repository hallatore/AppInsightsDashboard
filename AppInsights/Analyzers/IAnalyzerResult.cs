namespace AppInsights.Analyzers
{
    public interface IAnalyzerResult
    {
        string Name { get; }
        AnalyzerResultType Type { get; }
        bool Success { get; }
    }

    public enum AnalyzerResultType
    {
        Table
    }

    public class TableAnalyzerResult : IAnalyzerResult
    {
        public TableAnalyzerResult(string name, bool success, TableResult table)
        {
            Name = name;
            Success = success;
            Type = AnalyzerResultType.Table;
            Table = table;
        }

        public TableResult Table { get; }
        public string Name { get; }
        public AnalyzerResultType Type { get; }
        public bool Success { get; }
    }
}