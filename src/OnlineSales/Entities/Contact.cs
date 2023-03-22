﻿// <copyright file="Contact.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Nest;
using OnlineSales.DataAnnotations;
using OnlineSales.Geography;

namespace OnlineSales.Entities;

[Table("contact")]
[SupportsElastic]
[SupportsChangeLog]
[Index(nameof(Email), IsUnique = true)]
public class Contact : BaseEntity
{
    [Searchable]
    public string? LastName { get; set; }

    [Searchable]
    public string? FirstName { get; set; }

    [Searchable]
    [Required]
    public string Email { get; set; } = string.Empty;

    [Searchable]
    public Continent? ContinentCode { get; set; }

    [Searchable]
    public Country? CountryCode { get; set; }

    [Searchable]
    public string? CityName { get; set; }

    [Searchable]
    public string? Address1 { get; set; }

    [Searchable]
    public string? Address2 { get; set; }

    [Searchable]
    public string? State { get; set; }

    [Searchable]
    public string? Zip { get; set; }

    [Searchable]
    public string? Phone { get; set; }

    public int? Timezone { get; set; }

    [Searchable]
    public string? Language { get; set; }

    [Required]
    public int DomainId { get; set; }

    [Ignore]
    [JsonIgnore]
    [ForeignKey("DomainId")]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Domain? Domain { get; set; }

    public int? AccountId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    [JsonIgnore]
    [ForeignKey("AccountId")]
    public virtual Account? Account { get; set; }

    public int? UnsubscribeId { get; set; }

    [DeleteBehavior(DeleteBehavior.SetNull)]
    [JsonIgnore]
    [ForeignKey("UnsubscribeId")]
    public virtual Unsubscribe? Unsubscribe { get; set; }

    [Searchable]
    public string? Source { get; set; }
}