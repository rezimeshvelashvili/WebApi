using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;

namespace WebApplication.services
{
    public interface IExcelService
    {
        public bool UploadLeaderboardData([FromForm(Name = "file")] IFormFile file);

        public IEnumerable<User> GetLeaderboardByDay(DateTime dateTime);

        public IEnumerable<dynamic> GetLeaderboardByWeek(DateTime dateTime);

        public IEnumerable<dynamic> GetLeaderboardByMonth(DateTime dateTime);

        public IEnumerable<User> GetAllData();

        public IEnumerable<dynamic> GetStats();

        public IEnumerable<dynamic> GetUserInfo(string username);
        
        
    }
}