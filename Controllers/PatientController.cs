
using DocPatientAppMVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
namespace DocPatientAppMVC.Controllers;
[Authorize(Roles = "Patient")]
public class PatientController : Controller 
{
    private readonly ApplicationDbContext _db;
    public PatientController(ApplicationDbContext db)
    { 
        _db = db;
    }
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value; 
        var pat = await _db.Patients.Include(p => p.Doctor).FirstOrDefaultAsync(p => p.UserId == userId);
        return View(pat); 
    } 
}
