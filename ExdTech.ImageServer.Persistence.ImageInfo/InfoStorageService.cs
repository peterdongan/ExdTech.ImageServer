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
            optionsBuilder.UseSqlite("FileName=d:/home/site/wwwroot/content/imageinfo.db");
            //optionsBuilder.UseSqlite("FileName=imageinfo.db");
            _options = optionsBuilder.Options;
        }

        public async Task AddInfo(Guid id, UploadedInfo info, string username)
        {
            using (var db = new ImageInfoContext(_options))
            {
                
                var newInfo = new ImageInfo
                {
                    AddedBy = username,
                    Author = info.Author,
                    DateAdded = DateTime.UtcNow,
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

        public async Task<RetrievedInfo> GetInfo(Guid id)
        {
            using (var db = new ImageInfoContext(_options))
            {
                var info = await db.ImageInfos.FindAsync(id);

                if (info == null)
                {
                    return null;
                }

                var iinfo = new RetrievedInfo
                {
                    AddedBy = info.AddedBy,
                    Author = info.Author,
                    DateAddedUtc = info.DateAdded,
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
