using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MsExperience.Functions.StorageListener
{
    public static class TableExtensions
    {
        public static async Task<T> RetrieveEntityAsync<T>(this CloudTable table, string partitionKey, string rowKey) where T : ITableEntity
        {
            TableOperation retreive = TableOperation.Retrieve<T>(partitionKey, rowKey);
            TableResult res = await table.ExecuteAsync(retreive);
            T entity = (T)res.Result;
            return entity;
        }

        public static async Task<IEnumerable<T>> RetrieveEntitiesAsync<T>(this CloudTable table, string partitionKey) where T : ITableEntity, new()
        {
            return await table.QueryEntitiesAsync<T>(pk => pk.PartitionKey == partitionKey);
        }

        public static async Task<IEnumerable<T>> QueryEntitiesAsync<T>(this CloudTable table, Expression<Func<T, bool>> predicate = null) where T : ITableEntity, new()
        {
            TableQuery<T> query;
            if (predicate != null)
                query = new TableQuery<T>().Where(predicate.ToString());
            else
                query = new TableQuery<T>();

            var result = await table.ExecuteQuerySegmentedAsync(query, null);
            var entities = result.Select(e => e).ToList();
            while (result.ContinuationToken != null)
            {
                result = await table.ExecuteQuerySegmentedAsync(query, result.ContinuationToken);
                entities.AddRange(result.Select(e => e));
            }
            return entities;
        }

        public static async Task InsertOrMergeAsync<T>(this CloudTable table, T entity) where T : ITableEntity
        {
            TableOperation ope = TableOperation.InsertOrMerge(entity);
            await table.ExecuteAsync(ope);
        }

        public static async Task InsertOrReplaceAsync<T>(this CloudTable table, T entity) where T : ITableEntity
        {
            TableOperation ope = TableOperation.InsertOrReplace(entity);
            await table.ExecuteAsync(ope);
        }

        public static async Task MergeAsync<T>(this CloudTable table, T entity) where T : ITableEntity
        {
            TableOperation ope = TableOperation.Merge(entity);
            await table.ExecuteAsync(ope);
        }

        public static async Task InsertAsync<T>(this CloudTable table, T entity) where T : ITableEntity
        {
            TableOperation ope = TableOperation.Insert(entity);
            await table.ExecuteAsync(ope);
        }

        public static async Task DeleteAsync<T>(this CloudTable table, T entity) where T : ITableEntity
        {
            TableOperation ope = TableOperation.Delete(entity);
            await table.ExecuteAsync(ope);
        }

        public static async Task<IList<TableResult>> InsertOrReplaceBatchAsync<T>(this CloudTable table, IEnumerable<T> entities) where T : ITableEntity
        {
            if (entities == null)
                return null;

            var result = new List<TableResult>();
            var iterCount = 0;
            var entitiesList = entities as IList<T> ?? entities.ToList();
            var entitiesBatch = entitiesList.Skip(iterCount * 100).Take(100).ToList();
            var batchOperation = new TableBatchOperation();
            do
            {
                foreach (var entity in entitiesBatch)
                    batchOperation.InsertOrReplace(entity);

                result.AddRange(await table.ExecuteBatchAsync(batchOperation));
                iterCount++;
                entitiesBatch = entitiesList.Skip(iterCount * 100).Take(100).ToList();
                batchOperation.Clear();
            } while (entitiesBatch.Any());
            return result;
        }

        public static async Task<IList<TableResult>> DeleteBatchAsync<T>(this CloudTable table, IEnumerable<T> entities) where T : ITableEntity, new()
        {
            if (entities == null)
                return new List<TableResult>();

            var foundEntities = new List<T>();
            foreach (var entity in entities)
            {
                if (entity.RowKey != null)
                {
                    var found = await table.RetrieveEntityAsync<T>(entity.PartitionKey, entity.RowKey);
                    if (found != null)
                        foundEntities.Add(found);
                }
                else
                {
                    foundEntities.AddRange(await table.RetrieveEntitiesAsync<T>(entity.PartitionKey));
                }
            }

            return await table.DeleteExistingEntitiesBatchAsync(foundEntities);
        }

        public static async Task<IList<TableResult>> DeleteBatchAsync<T>(this CloudTable table, Expression<Func<T, bool>> predicate) where T : ITableEntity, new()
        {
            var foundEntities = (await QueryEntitiesAsync(table, predicate));

            return await table.DeleteExistingEntitiesBatchAsync(foundEntities);
        }

        public static async Task<IList<TableResult>> DeleteExistingEntitiesBatchAsync<T>(this CloudTable table, IEnumerable<T> entities) where T : ITableEntity, new()
        {
            if (entities == null || !entities.Any())
                return new List<TableResult>();

            var foundEntities = entities.GroupBy(e => e.PartitionKey);

            var result = new List<TableResult>();
            foreach (var entitiesByPK in foundEntities)
            {
                var iterCount = 0;
                var entitiesBatch = entitiesByPK.Skip(iterCount * 100).Take(100).ToList();
                var batchOperation = new TableBatchOperation();
                while (entitiesBatch.Any())
                {
                    foreach (var entity in entitiesBatch)
                    {
                        batchOperation.Delete(entity);
                    }

                    result.AddRange(await table.ExecuteBatchAsync(batchOperation));
                    iterCount++;
                    entitiesBatch = entitiesByPK.Skip(iterCount * 100).Take(100).ToList();
                    batchOperation.Clear();
                }
            }
            return result;
        }
    }
}
