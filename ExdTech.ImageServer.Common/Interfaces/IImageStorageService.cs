using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ExdTech.ImageServer.Common
{
    public interface IImageStorageService
    {
        /// <summary>
        /// Throw a FIleNotFoundException if the file is not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<RetrievedImageFile> GetImageFile (Guid id);

        Task<Guid> AddImage (byte[] data, string docType);
    }
}
