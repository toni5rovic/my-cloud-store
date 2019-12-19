using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCloudStore.Service.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCloudStore.Service.DataLayer.Map
{
    public class FileMap : IEntityTypeConfiguration<File>
    {
        public void Configure(EntityTypeBuilder<File> builder)
        {
            builder.Property(file => file.Id);
            builder.Property(file => file.FileName);
            builder.Property(file => file.Path);
            builder.Property(file => file.HashValue);
            builder.Property(file => file.Created);
        }
    }
}
