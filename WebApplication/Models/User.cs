using System;

namespace WebApplication.Models
{
    public class User
    {
        public int Id { get; set; }
        
        public string UserName { get; set; }
        
        public int Score { get; set; }
        
        public DateTime UploadDate { get; set; }
    }
}