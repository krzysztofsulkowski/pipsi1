namespace TimeCapsule.Models.DatabaseModels
{
    public class CapsuleQuestion
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }

        public ICollection<CapsuleAnswer> CapsuleAnswers { get; set; }
    }
}
