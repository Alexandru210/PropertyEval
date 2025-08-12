namespace PropertyEval.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public User() { }

        public User(string name, string email)
        {
            Name = name;
            Email = email;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
