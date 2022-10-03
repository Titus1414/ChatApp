namespace ChatApp.Models.Dtos
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public bool IsMyMessage { get; set; }
        public string? Image { get; set; }
        public string? Time { get; set; }
        public string? Name { get; set; }
        public bool? IsRead { get; set; }
        public int? ChatId { get; set; }
        public int? ToId { get; set; }
    }
}
