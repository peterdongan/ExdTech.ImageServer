using System;

namespace ExdTech.ImageServer.ImageInfoPersistence
{
    public class ImageInfo
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; }
        public string AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Source { get; set; }
        public string Notes { get; set; }
        public int? LicenceId { get; set; }
        
    }
}
