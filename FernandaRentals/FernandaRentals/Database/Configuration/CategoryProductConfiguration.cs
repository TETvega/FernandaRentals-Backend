﻿using FernandaRentals.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FernandaRentals.Database.Configuration
{
    public class CategoryProductConfiguration : IEntityTypeConfiguration<CategoryProductEntity>
    {
        public void Configure(EntityTypeBuilder<CategoryProductEntity> builder)
        {
            builder.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .HasPrincipalKey(e => e.Id);
            //.IsRequired();

            builder.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .HasPrincipalKey(e => e.Id);
              //  .IsRequired();
        }
    }
}
