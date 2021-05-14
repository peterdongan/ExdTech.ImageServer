using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ExdTech.ImageServer.Contract
{
    public interface IImageStore
    {
        /// <summary>
        /// Throw a FIleNotFoundException if the file is not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<RetrievedImage> GetImage (Guid id);

        Task<Guid> AddImage (byte[] data, string docType);
    }
}
