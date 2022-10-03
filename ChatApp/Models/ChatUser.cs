using System;
using System.Collections.Generic;

namespace ChatApp.Models
{
    public partial class ChatUser
    {
        public ChatUser()
        {
            UsersMessages = new HashSet<UsersMessage>();
        }

        public int Id { get; set; }
        public string? ConnId { get; set; }
        public int? Ufrom { get; set; }
        public int? Uto { get; set; }
        public string? UserName { get; set; }
        public string? LastMessage { get; set; }
        public bool? IsRead { get; set; }
        public string? Avatar { get; set; }
        public long? Date { get; set; }
        public bool? IsArchive { get; set; }
        public bool? IsBlock { get; set; }

        public virtual ICollection<UsersMessage> UsersMessages { get; set; }
    }
}
