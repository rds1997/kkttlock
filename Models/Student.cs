namespace KKProject.Models
{
    public class Student
    {
        public Guid Id { get;set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int DurationInHours { get; set; }
        public DateTime StartTime { get; set; }
        public float TotalAmount { get; set; }
        public bool IsLocker { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
