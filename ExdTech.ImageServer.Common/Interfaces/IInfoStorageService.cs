using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ExdTech.ImageServer.Common
{
    public interface IInfoStorageService
    {
        Task<Info> GetInfo (Guid id);

        Task AddInfo (Guid id, Info info);
    }
}
