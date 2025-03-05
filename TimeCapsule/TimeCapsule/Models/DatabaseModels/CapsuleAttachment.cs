using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TimeCapsule.Models.DatabaseModels
{
    public class CapsuleAttachment
    {
        public int Id { get; set; }
        public int CapsuleId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Link { get; set; }
        public AttachmentType Type { get; set; } 
        public Capsule Capsule { get; set; }
    }
    public enum AttachmentType
    {
        File,
        Link
    }
}
