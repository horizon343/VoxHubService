using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoxHubService.DB.Models;

namespace VoxHubService.DB.Configurations;

public sealed class ModelEntityConfiguration : IEntityTypeConfiguration<ModelEntity>
{
    public void Configure(EntityTypeBuilder<ModelEntity> e)
    {
        e.ToTable("models");
        e.HasKey(x => x.Id);

        e.Property(x => x.Name).IsRequired();
    }
}