using System;
using System.Collections.Generic;

namespace ChatApp.Models
{
    public partial class UsersMessage
    {
        public int Id { get; set; }
        public int? ChatId { get; set; }
        public string? ConnFrom { get; set; }
        public string? ConnTo { get; set; }
        public int? UidFrom { get; set; }
        public int? UidTo { get; set; }
        public string? Message { get; set; }
        public string? Type { get; set; }
        public bool? IsRead { get; set; }
        public long? Date { get; set; }

        public virtual ChatUser? Chat { get; set; }
    }
}
