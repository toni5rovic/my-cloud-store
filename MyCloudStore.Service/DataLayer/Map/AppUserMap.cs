using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCloudStore.Service.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCloudStore.Service.DataLayer.Map
{
    public class AppUserMap : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(user => user.FirstName);
            builder.Property(user => user.LastName);
            builder.Property(user => user.MaxKBs);
            builder.HasMany<File>(user => user.Files)
                .WithOne(file => file.User)
                .HasForeignKey(file => file.UserId);
        }
    }
}
