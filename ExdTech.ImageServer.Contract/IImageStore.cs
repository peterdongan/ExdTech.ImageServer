using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ExdTech.ImageServer.Contract
{
    public interface IImageStore
    {
        Task<RetrievedImage> GetImage (Guid id);

        Task<Guid> AddImage (byte[] data, string docType);
    }
}
