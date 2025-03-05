using Microsoft.AspNetCore.Identity;

namespace TimeCapsule.Models.DatabaseModels
{
    public class Capsule
    {
        public int Id { get; set; }
        public CapsuleType Type { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public string Introduction { get; set; }
        public string Question1Answer { get; set; }  
        public string MessageContent { get; set; }
        public DateTime OpeningDate { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public IdentityUser CreatedByUser { get; set; }
        public Status Status { get; set; }


        public ICollection<CapsuleRecipient> CapsuleRecipients { get; set; } 
        public ICollection<CapsuleAnswer> CapsuleAnswers { get; set; } 
        public ICollection<CapsuleAttachment> CapsuleAttachments { get; set; }
    }
    public enum CapsuleType
    {
        Individual,
        Couple,
        Group
    }

    public enum Status
    {
        Created,
        Opened,
        Deleted
    }
}
