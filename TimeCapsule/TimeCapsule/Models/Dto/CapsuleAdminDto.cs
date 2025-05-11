using System;

namespace TimeCapsule.Models.Dto
{
    public class CapsuleAdminDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime OpeningDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
