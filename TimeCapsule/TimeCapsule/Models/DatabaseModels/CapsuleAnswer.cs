using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TimeCapsule.Models.DatabaseModels
{
    public class CapsuleAnswer
    {
        public int Id { get; set; }
        public int CapsuleId { get; set; }
        public string QuestionName { get; set; }  //ToDo: do każdego pytania dodać unikalną nazwę/identyfikator
        public string AnswerText { get; set; }
        public int UserId { get; set; } 
        public Capsule Capsule { get; set; }
        public IdentityUser User { get; set; }
    }
}
