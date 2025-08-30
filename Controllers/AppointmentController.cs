
using DocPatientAppMVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Linq;
namespace DocPatientAppMVC.Controllers; public class AppointmentController : Controller { private readonly ApplicationDbContext _db; public AppointmentController(ApplicationDbContext db) { _db = db; } [Authorize(Roles = "Patient")] public async System.Threading.Tasks.Task<IActionResult> Book() { var docs = await _db.DoctorMasters.ToListAsync(); ViewBag.Doctors = new SelectList(docs, "Id", "Name"); return View(); } [HttpPost, Authorize(Roles = "Patient")] public async System.Threading.Tasks.Task<IActionResult> Book(int DoctorId, System.DateTime localSlot) { var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; var pat = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId); var appt = new DocPatientAppMVC.Models.Appointment { DoctorId = DoctorId, PatientId = pat.Id, SlotUtc = System.DateTime.SpecifyKind(localSlot, System.DateTimeKind.Local).ToUniversalTime() }; _db.Appointments.Add(appt); await _db.SaveChangesAsync(); return RedirectToAction("Index","Patient"); } [Authorize(Roles="Doctor")] public async System.Threading.Tasks.Task<IActionResult> List() { var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; var doc = await _db.DoctorMasters.FirstOrDefaultAsync(d => d.Username == username); var appts = await _db.Appointments.Include(a=>a.Patient).Where(a=>a.DoctorId==doc.Id).ToListAsync(); return View(appts); } }
