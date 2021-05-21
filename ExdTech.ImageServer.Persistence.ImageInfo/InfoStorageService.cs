using ExdTech.ImageServer.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ExdTech.ImageServer.ImageInfoPersistence
{
    public class InfoStorageService : IInfoStorageService
    {
        private readonly DbContextOptions _options;

        public InfoStorageService()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            //optionsBuilder.UseSqlite("Data Source=d:/home/site/wwwroot/content/imageinfo.db");
            optionsBuilder.UseSqlite("FileName=imageinfo.db");
            _options = optionsBuilder.Options;
        }

        public async Task AddInfo(Guid id, Info info)
        {
            using (var db = new ImageInfoContext(_options))
            {
                var newInfo = new ImageInfo
                {
                    AddedBy = info.AddedBy,
                    Author = info.Author,
                    DateAdded = info.DateAdded,
                    Id = id,
                    LicenceId = (int)info.LicenceId,
                    Notes = info.Notes,
                    OriginalFileName = info.OriginalFileName,
                    Title = info.Title,
                    Source = info.Source
                };
                db.ImageInfos.Add(newInfo);
                await db.SaveChangesAsync();
            }


        }

        public async Task<Info> GetInfo(Guid id)
        {
            using (var db = new ImageInfoContext(_options))
            {
                var info = await db.ImageInfos.FindAsync(id);

                if (info == null)
                {
                    return null;
                }

                var iinfo = new Info
                {
                    AddedBy = info.AddedBy,
                    Author = info.Author,
                    DateAdded = info.DateAdded,
                    LicenceId = (LicenceType)info.LicenceId,
                    Notes = info.Notes,
                    OriginalFileName = info.OriginalFileName,
                    Title = info.Title,
                    Source = info.Source
                };
                return iinfo;
            }
        }
    }
}
