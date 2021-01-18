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

namespace WebApplication.Controllers
{
    [Produces("application/json")]
    [Route("/api")]
    public class LeaderboardController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private IRedisCacheService _redisCacheService;
        private readonly IExcelService _excelService;


        public LeaderboardController(IConfiguration configuration, IRedisCacheService redisCacheService, IExcelService excelService)
        {
            _configuration = configuration;
            _redisCacheService = redisCacheService;
            _excelService = excelService;
        }
        
        [HttpPost("UploadLeaderboardData")]
        public IActionResult UploadLeaderboardData([FromForm(Name = "file")] IFormFile file)
        {
            _excelService.UploadLeaderboardData(file);
            return Ok();
        }
        
        [HttpGet("GetLeaderboardByDay")]
        public IEnumerable<User> GetLeaderboardByDay(DateTime dateTime)
        {
            return _excelService.GetLeaderboardByDay(dateTime);
        }
        
        [HttpGet("GetLeaderboardByWeek")]
        public IEnumerable<dynamic> GetLeaderboardByWeek(DateTime dateTime)
        {
            return _excelService.GetLeaderboardByWeek(dateTime);
        }
        
        [HttpGet("GetLeaderboardByMonth")]
        public IEnumerable<dynamic> GetLeaderboardByMonth(DateTime dateTime)
        {
            return _excelService.GetLeaderboardByMonth(dateTime);
        }
        
        [HttpGet("GetAllData")]
        public IEnumerable<User> GetAllData()
        {
            return _excelService.GetAllData();
        }
        
        [HttpGet("GetStats")]
        public IEnumerable<dynamic> GetStats()
        { 
            return _excelService.GetStats();
        }   
        [HttpGet("GetUserInfo")]
        public IEnumerable<dynamic> GetUserInfo(string username)
        {
            return _excelService.GetUserInfo(username);
        }
    }
}