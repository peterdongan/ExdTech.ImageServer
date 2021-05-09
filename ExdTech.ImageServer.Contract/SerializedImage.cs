using System;

namespace ExdTech.ImageServer.Contract
{
    /// <summary>
    /// Schema for POSTs to the API
    /// </summary>
    public class SerializedImage
    {
        public byte[] Data { get; set; }
    }
}
