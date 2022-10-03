using ChatApp.Common;
using ChatApp.Hubs;
using ChatApp.Models;
using ChatApp.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ChatApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ChatAppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        //int UsId = Methods.UsId;
        public HomeController(ILogger<HomeController> logger, ChatAppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _logger = logger;
            _context = context;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            var UsId = HttpContext.Session.GetInt32("userId");
            if (UsId != null)
            {
                return View();
            }
            return RedirectToAction("Login", "Home");
        }
        public IActionResult Registeration()
        {

            return View();
        }
        [HttpPost]
        public IActionResult Registeration(User user)
        {
            try
            {
                string password = Methods.Encrypt(user.Passwrod);
                user.Passwrod = password;
                user.Date = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                user.IsActive = true;
                user.IsVerified = false;
                _context.Users.Add(user);
                _context.SaveChanges();

                ChatUser chatUser = new();
                chatUser.ConnId = "";
                chatUser.UserName = user.UserName;
                chatUser.IsArchive = false;
                chatUser.IsBlock = false;
                chatUser.IsRead = false;
                chatUser.Date = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                chatUser.Ufrom = _context.Users.Max(a => a.Id);
                chatUser.Avatar = "/imgs/avatar1.png";
                _context.ChatUsers.Add(chatUser);
                _context.SaveChanges();

                return RedirectToAction("Login", "Home");
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IActionResult Login()
        {

            return View();
        }
        [HttpPost]
        public IActionResult Login(string email, string pass)
        {
            string password = Methods.Encrypt(pass);
            var chek = _context.Users.Where(a => a.Email == email && a.Passwrod == password).FirstOrDefault();
            if (chek != null)
            {
                HttpContext.Session.SetInt32("userId", chek.Id);
                ViewBag.SessionName = chek.UserName;
                var dt = _context.ChatUsers.Where(a => a.Ufrom == chek.Id).FirstOrDefault();
                ViewBag.SessionImage = dt.Avatar;
                return RedirectToAction("Chat", "Home");
            }

            //HttpContext.Session.Remove("userId");
            //var UsId = HttpContext.Session.GetInt32("userId");
            return View();
        }
        public IActionResult Privacy()
        {
            var UsId = HttpContext.Session.GetInt32("userId");
            if (UsId != null)
            {
                return View();
            }
            return RedirectToAction("Login", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage(int chatid, int toid, string message)
        {
            var UsId = HttpContext.Session.GetInt32("userId");
            if (UsId != null)
            {

                var chk = await _context.ChatUsers.Where(a => a.Id == chatid).FirstOrDefaultAsync();
                if (chk != null)
                { 
                var chkOpposite = await _context.ChatUsers.Where(a => a.Uto == chk.Ufrom && a.Ufrom == chk.Uto).FirstOrDefaultAsync();
                    if (chkOpposite == null)
                    {
                        ChatUser chatUser = new();
                        var chekusers = await _context.ChatUsers.Where(a => a.Ufrom == chk.Uto).FirstOrDefaultAsync();
                        chatUser.ConnId = chekusers.ConnId;
                        var user = _context.Users.Where(a => a.Id == chekusers.Ufrom).FirstOrDefault();
                        chatUser.UserName = user.UserName;
                        chatUser.IsArchive = false;
                        chatUser.IsBlock = false;
                        chatUser.IsRead = false;
                        chatUser.Date = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        chatUser.Ufrom = chekusers.Ufrom;
                        chatUser.Uto = chk.Ufrom;
                        _context.ChatUsers.Add(chatUser);
                        _context.SaveChanges();
                    }
                }

                var msgusers = await _context.ChatUsers.Where(a => a.Id == chatid).FirstOrDefaultAsync();
                if (msgusers.IsBlock == true)
                {
                    return Content("Blocked");
                }

                UsersMessage usersMessage = new();
                usersMessage.ChatId = chatid;
                usersMessage.Message = message;
                usersMessage.UidFrom = msgusers.Ufrom;
                usersMessage.UidTo = msgusers.Uto;
                usersMessage.IsRead = false;
                usersMessage.Date = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var usr = await _context.ChatUsers.Where(a => a.Ufrom == msgusers.Ufrom).FirstOrDefaultAsync();
                var usrF = await _context.ChatUsers.Where(a => a.Ufrom == msgusers.Uto).FirstOrDefaultAsync();
                usersMessage.ConnFrom = usr.ConnId;
                usersMessage.ConnTo = usrF.ConnId;
                usersMessage.Type = "text";
                _context.Add(usersMessage);
                _context.SaveChanges();

                int id = _context.UsersMessages.Max(a => a.Id);

                await _hubContext.Clients.Clients(usr.ConnId).SendAsync(method: "ReceiveMessage", id, message, usrF.Avatar, usrF.UserName, usrF.Id);

                return Content("Success");
            }
            return RedirectToAction("Login", "Home");
        }
        public IActionResult InboxChat()
        {
            var UsId = HttpContext.Session.GetInt32("userId");

            var sdf = _context.ChatUsers.Where(a => a.IsArchive != true && a.IsBlock != true && a.Ufrom != UsId && a.Uto == UsId).ToList();
            List<ChatDto> chat = new();
            foreach (var item in sdf)
            {
                ChatDto dto = new();
                dto.Id = item.Id;
                dto.UserName = item.UserName;
                dto.Date = item.Date;
                dto.LastMessage = item.LastMessage;
                dto.Image = Methods.baseUrl + item.Avatar;
                var cnt = _context.UsersMessages.Where(a => a.IsRead != true && a.UidFrom == item.Uto && a.UidTo == item.Ufrom).ToList();
                dto.ChatCount = cnt.Count;

                chat.Add(dto);
            }

            ViewBag.Inbox = chat;

            return PartialView("~/Views/Home/_UsersChatModal.cshtml");
        }

        public IActionResult ArchiveChats()
        {
            var UsId = HttpContext.Session.GetInt32("userId");

            var sdfa = _context.ChatUsers.Where(a => a.IsArchive == true && a.IsBlock != true && a.Ufrom != UsId && a.Uto == UsId).ToList();
            List<ChatDto> chata = new();
            foreach (var item in sdfa)
            {
                ChatDto dto = new();
                dto.Id = item.Id;
                dto.UserName = item.UserName;
                dto.Date = item.Date;
                dto.LastMessage = item.LastMessage;
                dto.Image = Methods.baseUrl + item.Avatar;
                var cnt = _context.UsersMessages.Where(a => a.IsRead != true && a.UidFrom == item.Uto && a.UidTo == item.Ufrom).ToList();
                dto.ChatCount = cnt.Count;

                chata.Add(dto);
            }

            ViewBag.Archive = chata;

            return PartialView("~/Views/Home/_UsersChatArchive.cshtml");

        }
        public IActionResult BlockChat()
        {
            var UsId = HttpContext.Session.GetInt32("userId");

            var sdfas = _context.ChatUsers.Where(a => a.IsBlock == true && a.Ufrom != UsId && a.Uto == UsId).ToList();
            List<ChatDto> chatas = new();
            foreach (var item in sdfas)
            {
                ChatDto dto = new();
                dto.Id = item.Id;
                dto.UserName = item.UserName;
                dto.Date = item.Date;
                dto.LastMessage = item.LastMessage;
                dto.Image = Methods.baseUrl + item.Avatar;
                var cnt = _context.UsersMessages.Where(a => a.IsRead != true && a.UidFrom == item.Uto && a.UidTo == item.Ufrom).ToList();
                dto.ChatCount = cnt.Count;

                chatas.Add(dto);
            }

            ViewBag.Block = chatas;

            return PartialView("~/Views/Home/_UsersChatBlock.cshtml");

        }
        public IActionResult ArchiveChat(int id, string check)
        {
            var data = _context.ChatUsers.Where(a => a.Id == id).FirstOrDefault();
            if (data != null)
            {
                if (data.IsArchive == true)
                {
                    data.IsArchive = false;
                }
                else
                {
                    data.IsArchive = true;
                }
                _context.ChatUsers.Update(data);
                _context.SaveChanges();
                if (check == "Archive")
                {
                    return RedirectToAction("InboxChat", "Home");
                }
                else
                {
                    return RedirectToAction("ArchiveChats", "Home");
                }
            }
            return RedirectToAction("Chat", "Home");
        }
        public IActionResult BlockChats(int id, string check)
        {
            var data = _context.ChatUsers.Where(a => a.Id == id).FirstOrDefault();
            if (data != null)
            {
                if (data.IsBlock == true)
                {
                    data.IsBlock = false;
                }
                else
                {
                    data.IsBlock = true;
                }
                _context.ChatUsers.Update(data);
                _context.SaveChanges();
                if (check == "Block")
                {
                    return RedirectToAction("InboxChat", "Home");
                }
                else if (check == "BlockArchive")
                {
                    return RedirectToAction("ArchiveChats", "Home");
                }
                else
                {
                    return RedirectToAction("BlockChat", "Home");
                }
            }
            return RedirectToAction("Chat", "Home");
        }
        public IActionResult FocusMessage(int id)
        {
            var data = _context.UsersMessages.Where(a => a.Id == id).FirstOrDefault();
            if (data != null)
            {
                if (data.IsRead != true)
                {
                    data.IsRead = true;
                    _context.UsersMessages.Update(data);
                    _context.SaveChanges();
                }
            }
            return Content("Success");
        }
        public IActionResult ChatModal(int id)
        {
            var UsId = HttpContext.Session.GetInt32("userId");
            if (UsId != null)
            {
                List<MessageDto> lst = new();

                var sd = _context.ChatUsers.Where(a => a.Id == id).FirstOrDefault();
                var sds = _context.ChatUsers.Where(a => a.Ufrom == sd.Uto && a.Uto == sd.Ufrom).FirstOrDefault();
                if (sds == null)
                {
                    sds = _context.ChatUsers.Where(a => a.Uto == sd.Uto && a.Ufrom == sd.Ufrom).FirstOrDefault();
                }
                if (sds != null)
                {
                    var Msgs = _context.UsersMessages.Where(a => a.ChatId == id || a.ChatId == sds.Id).ToList();
                    foreach (var item in Msgs)
                    {
                        MessageDto dto = new();
                        //Session Id will be here by my side
                        if (item.UidFrom != UsId)
                        {
                            dto.IsMyMessage = true;
                            var user = _context.ChatUsers.Where(a => a.Ufrom == UsId).FirstOrDefault();
                            dto.Name = user.UserName;
                            dto.Image = Methods.baseUrl + user.Avatar;
                        }
                        else
                        {
                            dto.IsMyMessage = false;
                            var user = _context.ChatUsers.Where(a => a.Ufrom == item.UidTo).FirstOrDefault();
                            dto.Name = user.UserName;
                            dto.Image = Methods.baseUrl + user.Avatar;
                        }
                        dto.IsRead = item.IsRead;
                        dto.Id = item.Id;
                        dto.Text = item.Message;
                        DateTime dt = (new DateTime(1970, 1, 1)).AddMilliseconds(double.Parse(item.Date.ToString()));
                        var ds = dt.ToString("dd HH mm");
                        dto.Time = ds;
                        dto.ChatId = item.ChatId;
                        dto.ToId = item.UidTo;
                        lst.Add(dto);
                    }
                    //if (lst.Count == 0)
                    //{
                    //    MessageDto dto = new();
                    //    dto.ChatId = id;
                    //    var user = _context.Users.Where(a => a.Id == UsId).FirstOrDefault();
                    //    dto.Name = user.UserName;
                    //    var chatUsers = _context.ChatUsers.Where(a => a.Id == id).FirstOrDefault();
                    //    if (chatUsers.Uto != UsId)
                    //    {
                    //        dto.ToId = chatUsers.Uto;
                    //    }
                    //    else
                    //    {
                    //        dto.ToId = chatUsers.Ufrom;
                    //    }
                    //    dto.Image = user.Image;
                    //    lst.Add(dto);
                    //}
                }


                return PartialView("~/Views/Home/_ChatModal.cshtml", lst);
            }
            return RedirectToAction("Login", "Home");
        }
        public IActionResult Chat()
        {
            var UsId = HttpContext.Session.GetInt32("userId");
            if (UsId != null)
            {
                //Sessions id will be here by my side
                ViewBag.SessionValue = UsId;
                var sdf = _context.ChatUsers.Where(a => a.IsArchive != true && a.IsBlock != true && a.Ufrom != UsId && a.Uto == UsId).ToList();
                List<ChatDto> chat = new();
                foreach (var item in sdf)
                {
                    ChatDto dto = new();
                    dto.Id = item.Id;
                    dto.UserName = item.UserName;
                    dto.Date = item.Date;
                    dto.LastMessage = item.LastMessage;
                    dto.Image = Methods.baseUrl + item.Avatar;
                    var cnt = _context.UsersMessages.Where(a => a.IsRead != true && a.UidFrom == item.Uto && a.UidTo == item.Ufrom).ToList();
                    dto.ChatCount = cnt.Count;

                    chat.Add(dto);
                }

                ViewBag.Inbox = chat;

                var sdfa = _context.ChatUsers.Where(a => a.IsArchive == true && a.IsBlock != true && a.Ufrom != UsId && a.Uto == UsId).ToList();
                List<ChatDto> chata = new();
                foreach (var item in sdfa)
                {
                    ChatDto dto = new();
                    dto.Id = item.Id;
                    dto.UserName = item.UserName;
                    dto.Date = item.Date;
                    dto.LastMessage = item.LastMessage;
                    dto.Image = Methods.baseUrl + item.Avatar;
                    var cnt = _context.UsersMessages.Where(a => a.IsRead != true && a.UidFrom == item.Uto && a.UidTo == item.Ufrom).ToList();
                    dto.ChatCount = cnt.Count;

                    chata.Add(dto);
                }

                ViewBag.Archive = chata;

                var sdfas = _context.ChatUsers.Where(a => a.IsBlock == true && a.Ufrom != UsId && a.Uto == UsId).ToList();
                List<ChatDto> chatas = new();
                foreach (var item in sdfas)
                {
                    ChatDto dto = new();
                    dto.Id = item.Id;
                    dto.UserName = item.UserName;
                    dto.Date = item.Date;
                    dto.LastMessage = item.LastMessage;
                    dto.Image = Methods.baseUrl + item.Avatar;
                    var cnt = _context.UsersMessages.Where(a => a.IsRead != true && a.UidFrom == item.Uto && a.UidTo == item.Ufrom).ToList();
                    dto.ChatCount = cnt.Count;

                    chatas.Add(dto);
                }

                var chek = _context.Users.Where(a => a.Id == UsId).FirstOrDefault();
                ViewBag.Block = chatas;
                ViewBag.SessionName = chek.UserName;
                var dt = _context.ChatUsers.Where(a => a.Ufrom == chek.Id).FirstOrDefault();
                ViewBag.SessionImage = dt.Avatar;
                return View();
            }
            return RedirectToAction("Login", "Home");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}