using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munaqasat.Web.Domain.Entities;

namespace Munaqasat.Web.Infrastructure.Persistence.Configurations;

internal static class ModelBuilderExtensions
{
    public const string Money = "numeric(12,3)";

    public static void HasTimestamps<T>(this EntityTypeBuilder<T> b) where T : class, IHasTimestamps
    {
        b.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        b.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
    }

    public static void HasCreatedAt<T>(this EntityTypeBuilder<T> b) where T : class, IHasCreatedAt =>
        b.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
}
