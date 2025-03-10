using Microsoft.AspNetCore.Identity;

namespace TimeCapsule.Models.DatabaseModels
{
    public class CapsuleAnswer
    {
        public int Id { get; set; }
        public int CapsuleQuestionId { get; set; }
        public int CapsuleId { get; set; } 
        public string AnswerText { get; set; }

        public Capsule Capsule { get; set; }
        public CapsuleQuestion CapsuleQuestion { get; set; }
    }
}
