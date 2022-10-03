using ChatApp.Common;
using ChatApp.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatAppDbContext _context;
        

        public ChatHub(ChatAppDbContext context)
        {
            _context = context;
        }
        public override async Task OnConnectedAsync()
        {
            try
            {
                var group = Context.GetHttpContext().Request.Query["token"].FirstOrDefault();

                var connectionId = Context.ConnectionId;

                var usr = _context.ChatUsers.Where(a => a.Ufrom == Convert.ToInt32(group)).FirstOrDefault();
                if (usr == null)
                {
                    ChatUser chatUser = new();
                    chatUser.ConnId = connectionId;
                    var user = _context.Users.Where(a => a.Id == Convert.ToInt32(group)).FirstOrDefault();
                    chatUser.UserName = user.UserName;
                    chatUser.IsArchive = false;
                    chatUser.IsBlock = false;
                    chatUser.IsRead = false;
                    chatUser.Date = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    chatUser.Ufrom = Convert.ToInt32(group);
                    _context.ChatUsers.Add(chatUser);
                    _context.SaveChanges();
                }
                else
                {
                    var lst = _context.ChatUsers.Where(a => a.Ufrom == Convert.ToInt32(group)).ToList();

                    foreach (var user in lst)
                    {
                        user.ConnId = connectionId;
                        _context.ChatUsers.Update(user);
                        _context.SaveChanges();
                    }

                    var msglst = _context.UsersMessages.Where(a => a.UidFrom == Convert.ToInt32(group)).ToList();
                    foreach (var item in msglst)
                    {
                        var cnid = _context.ChatUsers.Where(a => a.Ufrom == Convert.ToInt32(group)).FirstOrDefault();
                        item.ConnFrom = cnid.ConnId;
                        _context.UsersMessages.Update(item);
                        _context.SaveChanges();
                    }

                    var msglstA = _context.UsersMessages.Where(a => a.UidTo == Convert.ToInt32(group)).ToList();
                    foreach (var item in msglstA)
                    {
                        var cnid = _context.ChatUsers.Where(a => a.Ufrom == Convert.ToInt32(group)).FirstOrDefault();
                        item.ConnTo = cnid.ConnId;
                        _context.UsersMessages.Update(item);
                        _context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }
    }
}
