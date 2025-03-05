using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TimeCapsule.Models.DatabaseModels
{
    public class CapsuleRecipient
    {
        public int Id { get; set; }
        public int CapsuleId { get; set; }
        public int RecipientUserId { get; set; }
        public bool EmailSent { get; set; } = false;
        public bool Accepted { get; set; } = false;
        public Capsule Capsule { get; set; }
        public IdentityUser RecipientUser { get; set; }

    }
}
