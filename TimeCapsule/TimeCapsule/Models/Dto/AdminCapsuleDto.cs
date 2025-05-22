namespace TimeCapsule.Models.Dto
{
    public class AdminCapsuleDto
    {
        public int CapsuleId { get; set; }
        public string Title { get; set; }
        public string UserName { get; set; }
        public bool IsOpened { get; set; }
        public DateTime OpenDate { get; set; }
    }
}
