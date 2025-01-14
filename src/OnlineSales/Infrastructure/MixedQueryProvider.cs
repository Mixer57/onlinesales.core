﻿// <copyright file="MixedQueryProvider.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using Nest;
using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Infrastructure
{
    public class MixedQueryProvider<T> : IQueryProvider<T>
        where T : BaseEntityWithId
    {
        private readonly QueryModelBuilder<T> queryBuilder;

        private readonly ElasticClient elasticClient;

        private readonly string indexPrefix;

        private IQueryable<T> query;

        public MixedQueryProvider(QueryModelBuilder<T> queryBuilder, IQueryable<T> query, ElasticClient elasticClient, string indexPrefix/*, PgDbContext dbContext*/)
        {
            this.queryBuilder = queryBuilder;
            this.elasticClient = elasticClient;
            this.indexPrefix = indexPrefix;
            this.query = query;
        }

        public async Task<QueryResult<T>> GetResult()
        {
            var selectData = new QueryModelBuilder<T>.SelectCommandData();
            selectData.SelectedProperties.AddRange(queryBuilder.SelectData.SelectedProperties);
            selectData.IsSelect = queryBuilder.SelectData.IsSelect;
            var includeData = new List<PropertyInfo>();
            includeData.AddRange(queryBuilder.IncludeData);

            var idSelectData = new QueryModelBuilder<T>.SelectCommandData() { SelectedProperties = new List<PropertyInfo> { typeof(T).GetProperty("Id") ! }, IsSelect = true };

            queryBuilder.SelectData = idSelectData;
            queryBuilder.IncludeData.Clear();

            var esProvider = new ESQueryProvider<T>(elasticClient, queryBuilder, indexPrefix);
            var esResult = await esProvider.GetResult();
            List<int> ids = esResult.Records == null ? new List<int>() : esResult.Records.Select(r => r.Id).ToList();

            query = query.Where(e => ids.Contains(e.Id));

            queryBuilder.WhereData.Clear();
            queryBuilder.SearchData.Clear();
            queryBuilder.IncludeData = includeData;
            queryBuilder.SelectData = selectData;
            queryBuilder.Skip = 0;
            queryBuilder.Limit = ids.Count;

            var dbProvider = new DBQueryProvider<T>(query, queryBuilder);

            var dbResult = await dbProvider.GetResult();

            return new QueryResult<T>(dbResult.Records, esResult.TotalCount);
        }
    }
}
