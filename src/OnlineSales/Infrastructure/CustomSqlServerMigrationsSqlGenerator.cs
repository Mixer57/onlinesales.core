﻿// <copyright file="CustomSqlServerMigrationsSqlGenerator.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal;
using NpgsqlTypes;
using OnlineSales.Data;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;
using OnlineSales.Helpers;

namespace OnlineSales.Infrastructure;

public class CustomSqlServerMigrationsSqlGenerator : NpgsqlMigrationsSqlGenerator
{
#pragma warning disable EF1001 // Internal EF Core API usage.
    public CustomSqlServerMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, INpgsqlSingletonOptions npgsqlSingletonOptions/*, PgDbContext dbContext*/)
        : base(dependencies, npgsqlSingletonOptions)
    {
    }

    protected override void Generate(RenameColumnOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        base.Generate(operation, model, builder);

        var type = GetType(GetIEntityType(model!, operation.Table));

        if (type != null && IsChangeLogSupported(type))
        {
            var renameKeyInChangeLog = new SqlOperation()
            {
                Sql = $@"UPDATE change_log set Data = Data - '{ColumnNameToCamelCase(operation.Name)}' ||  jsonb_build_object('{ColumnNameToCamelCase(operation.NewName)}', Data -> '{ColumnNameToCamelCase(operation.Name)}') WHERE object_type = '{type.Name}'",
            };

            Generate(renameKeyInChangeLog, model, builder);
        }
    }

    protected override void Generate(AlterColumnOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        base.Generate(operation, model, builder);

        var type = GetType(GetIEntityType(model!, operation.Table));

        if (type != null && IsChangeLogSupported(type) && operation.OldColumn.ClrType != operation.ClrType)
        {
            throw new ChangeLogMigrationException("Changing colum type isn't supported. Please use SqlOperation for migrations.");
        }
    }

    protected override void Generate(DropTableOperation operation, IModel? model, MigrationCommandListBuilder builder, bool terminate = true)
    {
        base.Generate(operation, model, builder, terminate);

        var type = GetType(GetIEntityType(model!, operation.Name));

        if (type != null)
        {
            var deleteFromChangeLog = new SqlOperation()
            {
                Sql = $@"DELETE from change_log WHERE object_type = '{type.Name}'",
            };

            Generate(deleteFromChangeLog, model, builder);
        }
    }

    protected override void Generate(DropColumnOperation operation, IModel? model, MigrationCommandListBuilder builder, bool terminate = true)
    {
        base.Generate(operation, model, builder, terminate);

        var type = GetType(GetIEntityType(model!, operation.Table));

        if (type != null && IsChangeLogSupported(type))
        {
            var dropColumnFromChangeLog = new SqlOperation()
            {
                Sql = $@"UPDATE change_log set Data = Data - '{ColumnNameToCamelCase(operation.Name)}' WHERE object_type = '{type.Name}'",
            };

            Generate(dropColumnFromChangeLog, model, builder);
        }
    }

    protected override void Generate(AddColumnOperation operation, IModel? model, MigrationCommandListBuilder builder, bool terminate = true)
    {
        base.Generate(operation, model, builder, terminate);

        var type = GetType(GetIEntityType(model!, operation.Table));

        if (type != null && IsChangeLogSupported(type))
        {
            if (operation.DefaultValueSql != null)
            {
                throw new ChangeLogMigrationException("Default Value Sql construction isn't supported. Please use SqlOperation for migrations.");
            }

            string strValue;

            if (!operation.IsNullable && operation.DefaultValue == null)
            {
                throw new ChangeLogMigrationException($"Cannot create default Value for column {operation.Name} in table {operation.Table}");
            }

            if (operation.DefaultValue == null)
            {
                strValue = "null";
            }
            else
            {
                if (operation.ClrType == typeof(string))
                {
                    strValue = $"'{operation.DefaultValue}'";
                }
                else if (operation.ClrType == typeof(DateTime))
                {
                    if (operation.DefaultValue is DateTime)
                    {
                        strValue = $"'{((DateTime)operation.DefaultValue).ToUniversalTime().ToString("O")}'";
                    }
                    else
                    {
                        throw new ChangeLogMigrationException($"Cannot convert default value to DateTime in column {operation.Name} in table {operation.Table}");
                    }                    
                }
                else
                {
                    strValue = $"{operation.DefaultValue}";
                }
            }

            var addKeyToChangeLog = new SqlOperation()
            {
                Sql = @$"UPDATE change_log set Data = change_log.Data || jsonb_build_object('{ColumnNameToCamelCase(operation.Name)}', {strValue}) WHERE object_type = '{type.Name}'",
            };

            Generate(addKeyToChangeLog, model, builder);
        }
    }

    protected override void Generate(InsertDataOperation operation, IModel? model, MigrationCommandListBuilder builder, bool terminate = true)
    {
        base.Generate(operation, model, builder, terminate);

        var type = GetType(GetIEntityType(model!, operation.Table));

        if (type != null && IsChangeLogSupported(type))
        {
            var insertDataCount = operation.Values!.GetLength(0);
            var insertChangeLogData = new SqlOperation()
            {
                Sql = @$"CREATE OR REPLACE FUNCTION pg_temp.key_underscore_to_camel_case(s text)
                        RETURNS json
                        IMMUTABLE
                        LANGUAGE sql
                        AS $$
                        SELECT to_json(substring(s, 1, 1) || substring(replace(initcap(replace(s, '_', ' ')), ' ', ''), 2));
                        $$;

                        CREATE OR REPLACE FUNCTION pg_temp.json_underscore_to_camel_case(data json)
                        RETURNS json
                        IMMUTABLE
                        LANGUAGE sql
                        AS $$
                        SELECT ('{{'||string_agg(key_underscore_to_camel_case(key)||':'||value, ',')||'}}')::json
                        FROM json_each(data)
                        $$;

                        insert into change_log (object_type, object_id, entity_state, data, created_at)
                        select '{type.Name}', {operation.Table}.id, {(int)EntityState.Added}, json_underscore_to_camel_case(row_to_json({operation.Table})), now()
                        from {operation.Table} where {operation.Table}.id in (select id from {operation.Table} order by id desc limit {insertDataCount})",
            };

            Generate(insertChangeLogData, model, builder);
        }
    }

    private static IEntityType GetIEntityType(IModel model, string tableName)
    {
        var ets = model.GetEntityTypes();
        return ets.First(et => et.GetTableName() == tableName);
    }

    private static Type? GetType(IEntityType etype)
    {
        return Assembly.GetEntryAssembly()!.GetType(etype.Name);
    }

    private static string ColumnNameToCamelCase(string columnName)
    {
        var res = string.Join(string.Empty, columnName.Split('_').Select(s => s = s.Length > 0 ? char.ToUpper(s[0]) + s.Substring(1) : s));
        res = char.ToLower(res[0]) + res.Substring(1);
        return res;
    }

    private static bool IsChangeLogSupported(Type type)
    {
        return type.GetCustomAttributes<SupportsChangeLogAttribute>().Any();
    }
}
