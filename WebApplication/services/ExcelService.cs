using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;
using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Dapper;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using StackExchange.Redis;
using WebApplication.Models;
using WebApplication.services;


namespace WebApplication.services
{
    public class ExcelService : IExcelService
    {
        private readonly IConfiguration _configuration;

        public ExcelService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public bool UploadLeaderboardData(IFormFile file)
        {
            var fileName = file.FileName;
            fileName = Path.GetFileNameWithoutExtension(fileName);
            if (!DateTime.TryParse(fileName, out var date))
            {
               // return BadRequest("date was incorrect");
            }
            var xlWorkbook = new XLWorkbook(file.OpenReadStream());
            var users = new List<User>();
            foreach (var xlWorkbookWorksheet in xlWorkbook.Worksheets)
            {
                var range = xlWorkbookWorksheet.RangeUsed();
                
                foreach (var xlRangeRow in range.Rows())
                {
                    var userName = xlRangeRow.Cell(1).Value.ToString();
                    var userScoreString = xlRangeRow.Cell(2).Value.ToString();
                    if (!int.TryParse(userScoreString, out var score))
                    {
                        return false; //BadRequest("bad value");
                    }
                    
                    users.Add(new User()
                    {
                        UserName = userName,
                        Score = score,
                        UploadDate = date
                    });
                }
                
            }


            var connection = GetConnection();
            foreach (var user in users)
            {
                connection.Execute("DELETE FROM Users WHERE UploadDate = @UploadDate AND UserName = @UserName", new { UploadDate = date, UserName = user.UserName });
                connection.Execute("INSERT INTO Users (UserName, Score, UploadDate) Values (@UserName, @Score, @UploadDate)", new
                {
                    UserName = user.UserName,
                    Score = user.Score,
                    UploadDate = user.UploadDate
                });
            }

            return true;

        }

        public IEnumerable<User> GetLeaderboardByDay(DateTime dateTime)
        {
            
            var connection = GetConnection();
            return connection.Query<User>("SELECT * FROM Users WHERE UploadDate = @dateTime ORDER BY Score DESC ", new { dateTime = dateTime });
        }

        public IEnumerable<dynamic> GetLeaderboardByWeek(DateTime dateTime)
        {
            var connection = GetConnection();
            var monday = dateTime.AddDays(-(int)dateTime.DayOfWeek + (int)DayOfWeek.Monday);
            var sunday = dateTime.AddDays(-(int) dateTime.DayOfWeek + 7);
            return connection.Query<dynamic>("SELECT u.UserName, SUM(Score) total FROM Users u WHERE u.UploadDate BETWEEN @monday AND @sunday GROUP BY u.UserName ORDER BY total DESC", 
                new { monday = monday, sunday = sunday});
        }

        public IEnumerable<dynamic> GetLeaderboardByMonth(DateTime dateTime)
        {
            var connection = GetConnection();
            var month = dateTime.Month;
            var year = dateTime.Year;
            return connection.Query<dynamic>("SELECT u.UserName, SUM(Score) total FROM Users u WHERE EXTRACT (MONTH FROM u.UploadDate) = @month AND EXTRACT ( YEAR FROM u.UploadDate) = @year GROUP BY u.UserName ORDER BY total DESC",
                param:new { month = month, year = year });
        }

        public IEnumerable<User> GetAllData()
        {
            var connection = GetConnection();
            return connection.Query<User>("SELECT * FROM Users");        }

        public IEnumerable<dynamic> GetStats()
        {
            var connection = GetConnection();
            return connection.Query<dynamic>(
                "select sum(score)/count(distinct(UploadDate)) as daily_avg, sum(score)/count(distinct (date_part('week',UploadDate), date_part('year',UploadDate))) as weekly_avg, sum(score)/count(distinct (date_part('month',UploadDate), date_part('year',UploadDate))) as monthly_avg, (select max(score) score from (select sum(score) score from users group by UploadDate) subq1) as daily_max, (select max(score) score from (select sum(score) score from users group by date_part('week',UploadDate),date_part('year',UploadDate)) subq2) as weekly_max, (select max(score) score from (select sum(score) score from users group by date_part('month',UploadDate),date_part('year',UploadDate)) subq3) as monthly_max from Users ");        }

        public IEnumerable<dynamic> GetUserInfo(string username)
        {
            var connection = GetConnection();
            var now = DateTime.Now;
            var month = now.Month;
            var year = now.Year;
            return connection.Query<dynamic>("SELECT UserName, rank, score FROM (select t.UserName, sum(Score) score, Rank() over(order by SUM(t.Score) desc) rank FROM Users t WHERE EXTRACT (MONTH FROM UploadDate) = @month AND EXTRACT ( YEAR FROM UploadDate) = @year group by UserName) t WHERE UserName = @username", 
                new { month = month, year = year ,username = username});
        }
        
        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(connectionString: _configuration.GetConnectionString("Default"));
        }
    }
}