using Expressions.Task3.E3SQueryProvider.Models.Request;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Expressions.Task3.E3SQueryProvider.Helpers
{
    public static class RequestGeneratorHelper
    {
        public static string GenerateFtsQueryRequestString(IEnumerable<string> queries, int start = 0, int limit = 10)
        {
            var ftsQueryRequest = new FtsQueryRequest
            {
                Statements = queries.Select(x => new Statement { Query = x }).ToList(),
                Start = start,
                Limit = limit
            };

            var ftsQueryRequestString = JsonConvert.SerializeObject(ftsQueryRequest);

            return ftsQueryRequestString;
        }
    }
}
