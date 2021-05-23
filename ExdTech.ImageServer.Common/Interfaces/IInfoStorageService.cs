using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ExdTech.ImageServer.Common
{
    public interface IInfoStorageService
    {
        Task<RetrievedInfo> GetInfo (Guid id);

        Task AddInfo (Guid id, UploadedInfo info, string username);
    }
}
