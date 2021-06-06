using ExdTech.ImageServer.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;



namespace ExdTech.ImageServer.ImageInfoPersistence
{
    public class InfoStorageService : IInfoStorageService
    {
        private readonly DbContextOptions _options;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="useSqlServer">True by default. If false then SQLite is used.</param>
        public InfoStorageService (string connectionString, bool useSqlServer = true)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            if (useSqlServer)
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
            else
            {
                optionsBuilder.UseSqlite(connectionString);
            }
            _options = optionsBuilder.Options;
        }


        public async Task AddInfo(Guid id, UploadedInfo info, string username, DateTime dateTime)
        {
            using (var db = new ImageInfoContext(_options))
            {
                
                var newInfo = new ImageInfo
                {
                    AddedBy = username,
                    Author = info.Author,
                    DateAdded = dateTime,
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
