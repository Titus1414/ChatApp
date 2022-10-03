using System;
using System.Collections.Generic;

namespace ChatApp.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public string? Fname { get; set; }
        public string? Mname { get; set; }
        public string? Lname { get; set; }
        public string? UserName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Passwrod { get; set; }
        public long? Date { get; set; }
        public bool? IsVerified { get; set; }
        public string? Image { get; set; }
        public bool? IsActive { get; set; }
    }
}
