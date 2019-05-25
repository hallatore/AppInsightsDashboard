using System.Collections.Generic;

namespace AppInsights
{
    public class StructuredQuery : List<string>
    {
        public StructuredQuery(IEnumerable<string> parts) : base(parts)
        {
        }
    }
}