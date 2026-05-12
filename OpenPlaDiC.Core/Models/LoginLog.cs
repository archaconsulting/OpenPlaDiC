using System;

namespace OpenPlaDiC.Core.Models
{
    public class LoginLog
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public Guid? UserId { get; set; }
        public DateTime LoginDate { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
