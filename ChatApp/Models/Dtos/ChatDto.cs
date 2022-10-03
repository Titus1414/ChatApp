namespace ChatApp.Models.Dtos
{
    public class ChatDto
    {
        public int Id { get; set; }
        public string? Image { get; set; }
        public string? UserName { get; set; }
        public string? LastMessage { get; set; }
        public long? Date { get; set; }
        public int UnreadCount { get; set; }
        public int ChatCount { get; set; }
    }
}
