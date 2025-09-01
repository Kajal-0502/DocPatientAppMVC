using DocPatientAppMVC.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
namespace DocPatientAppMVC.Controllers;
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    public HomeController(ApplicationDbContext db) { _db = db; }
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult History(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required");
            }

            var history = _db.Messages
                .Where(c => (c.FromUserId == userId || c.ToUserId == userId))
                .OrderBy(c => c.SentAtUtc)
                .Select(c => new
                {
                    c.FromUserId,
                    c.ToUserId,
                    c.Text,
                    SentAt = c.SentAtUtc.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToList();

            return Json(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }



}