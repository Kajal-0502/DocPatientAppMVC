
using DocPatientAppMVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace DocPatientAppMVC.Controllers; [Authorize(Roles = "Doctor")] public class DoctorController : Controller { private readonly ApplicationDbContext _db; public DoctorController(ApplicationDbContext db) { _db = db; } public async System.Threading.Tasks.Task<IActionResult> Index() { var username = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value; var doc = await _db.DoctorMasters.FirstOrDefaultAsync(d => d.Username == username); var patients = await _db.Patients.Where(p => p.DoctorId == doc.Id).ToListAsync(); return View(patients); } }
