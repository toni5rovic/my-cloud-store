using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyCloudStore.Service.DataLayer.Models
{
    public class File
    {
        [Key]
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string Path { get; set; }
        public byte[] HashValue { get; set; }
        public DateTime Created { get; set; }

        public Guid UserId { get; set; }
        public AppUser User { get; set; }
    }
}
