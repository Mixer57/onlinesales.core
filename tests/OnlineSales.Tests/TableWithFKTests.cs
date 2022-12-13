﻿// <copyright file="TableWithFKTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using Namotion.Reflection;
using Nest;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public abstract class TableWithFKTests<T, TC, TU> : SimpleTableTests<T, TC, TU>
    where T : BaseEntity
    where TC : new()
    where TU : new()
{
    protected TableWithFKTests(string url)
        : base(url)
    {
    }

    [Fact]
    public virtual async Task CreateItemWithNonExistedFKItemTest()
    {
        var testItem = new TC();
        await PostTest(itemsUrl, testItem, HttpStatusCode.InternalServerError);
    }

    [Fact]

    public async Task CascadeDeleteTest()
    {
        var fkItem = await CreateFKItem();

        var fkItemId = fkItem.Item1;

        int numberOfItems = 10;

        string[] itemsUrls = new string[numberOfItems];

        for (var i = 0; i < numberOfItems; ++i)
        {
            var testItem = await CreateItem(fkItemId);

            itemsUrls[i] = testItem.Item2;
        }

        await DeleteTest(fkItem.Item2);

        for (var i = 0; i < numberOfItems; ++i)
        {
            await GetTest<T>(itemsUrls[i], HttpStatusCode.NotFound);
        }             
    }

    protected abstract Task<(int, string)> CreateFKItem();

    protected override async Task<(TC, string)> CreateItem()
    {
        var fkItem = await CreateFKItem();

        var fkId = fkItem.Item1;

        return await CreateItem(fkId);
    }

    protected abstract Task<(TC, string)> CreateItem(int fkId);
}