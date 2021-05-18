using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExdTech.ImageServer.ImageInfoPersistence
{
    class ImageInfoContext : DbContext
    {
        public ImageInfoContext(DbContextOptions options) : base(options)
        {
            var test = Database.EnsureCreated();
        }

        public DbSet<ImageInfo> ImageInfos { get; set; }
        public DbSet<Licence> Licences { get; set; }
    }
}
