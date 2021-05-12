using System.Collections;
using System.Collections.Generic;

namespace WordPressPCL.Models
{
    /// <summary>
    /// Base class for query results
    /// </summary>
    public class QueryResult<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> items;

        public QueryResult(IEnumerable<T> items, int total, int totalPages) 
        {
            this.items = items;
            Total = total;
            TotalPages = totalPages;
        }

        public int Total { get; }

         public int TotalPages { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}