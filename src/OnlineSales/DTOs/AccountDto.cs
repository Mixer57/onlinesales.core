﻿// <copyright file="AccountDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;
using OnlineSales.Geography;

namespace OnlineSales.DTOs;

public class AccountCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? State { get; set; }

    public Continent? ContinentCode { get; set; }

    public Country? CountryCode { get; set; }

    [SwaggerExample<string>("Colombo")]
    public string? CityName { get; set; }

    [SwaggerExample<string>("https://example.com")]
    public string? SiteUrl { get; set; }

    [SwaggerExample<string>("https://example.com/logo.png")]
    public string? LogoUrl { get; set; }

    [SwaggerExample<string>("50K-100K")]
    public string? EmployeesRange { get; set; }

    public double? Revenue { get; set; }

    public string[]? Tags { get; set; }

    public Dictionary<string, string>? SocialMedia { get; set; }

    public string? Source { get; set; }

    [Optional]
    public string? Data { get; set; }
}

public class AccountDetailsInfo
{
    public string? Name { get; set; }

    public string? SiteUrl { get; set; }

    public string? LogoUrl { get; set; }

    public string? CityName { get; set; }

    public string? State { get; set; }

    public Country? CountryCode { get; set; }

    public string? EmployeesRange { get; set; }

    public double? Revenue { get; set; }

    public string[]? Tags { get; set; }

    public Dictionary<string, string>? SocialMedia { get; set; }

    public string? Source { get; set; }

    public string? Data { get; set; }
}

public class AccountUpdateDto
{
    public string? Name { get; set; }

    public string? SiteUrl { get; set; }

    public string? LogoUrl { get; set; }

    public string? City { get; set; }

    public string? StateCode { get; set; }

    public Country? CountryCode { get; set; }

    public string? EmployeesRange { get; set; }

    public double? Revenue { get; set; }

    public string[]? Tags { get; set; }

    public Dictionary<string, string>? SocialMedia { get; set; }

    public string? Source { get; set; }

    public string? Data { get; set; }
}

public class AccountDetailsDto : AccountCreateDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Ignore]
    public List<ContactDetailsDto>? Contacts { get; set; }

    [Ignore]
    public List<DomainDetailsDto>? Domains { get; set; }
}

public class AccountImportDto : BaseImportDto
{
    [Optional]
    [SwaggerUnique]
    public string Name { get; set; } = string.Empty;

    [Optional]
    public string? City { get; set; }

    [Optional]
    public string? StateCode { get; set; }

    [Optional]
    public Country? CountryCode { get; set; }

    [Optional]
    public string? SiteUrl { get; set; }

    [Optional]
    public string? LogoUrl { get; set; }

    [Optional]
    public string? EmployeesRange { get; set; }

    [Optional]
    public double? Revenue { get; set; }

    [Optional]
    public string[]? Tags { get; set; }

    [Optional]
    public Dictionary<string, string>? SocialMedia { get; set; }

    [Optional]
    public string? Data { get; set; }
}