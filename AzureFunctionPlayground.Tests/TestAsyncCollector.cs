using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace AzureFunctionPlayground.Tests
{
    public class TestAsyncCollector<T> : IAsyncCollector<T>
    {
        private readonly List<T> _addedItems = new List<T>();

        public IEnumerable<T> AddedItems => _addedItems;

        public Task AddAsync(T item, CancellationToken cancellationToken = new CancellationToken())
        {
            _addedItems.Add(item);

            return Task.CompletedTask;
        }

        public Task FlushAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }
    }
}