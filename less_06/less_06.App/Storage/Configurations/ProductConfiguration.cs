using less_06.App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using less_06.App.Models;

namespace less_06.App.Storage.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name).IsRequired();
        builder.HasIndex(p => p.Name);
        builder.Property(p => p.Price).IsRequired();
        builder.Property(p => p.Description).IsRequired(false);
    }
}