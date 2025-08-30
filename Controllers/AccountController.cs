
using DocPatientAppMVC.Data;
using DocPatientAppMVC.Models;
using DocPatientAppMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace DocPatientAppMVC.Controllers;
public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;
    public AccountController(ApplicationDbContext db) { _db = db; }

    public IActionResult PatientRegister() 
    {
        var doctors = _db.DoctorMasters
        .Select(doc => new SelectListItem
        {
            Value = doc.Id.ToString(),
            Text = doc.Department + " - " + doc.Name
        })
        .ToList();

        ViewBag.Doctors = doctors;
        return View(); 
    }

    [HttpPost] 
    public async Task<IActionResult> PatientRegister(PatientRegisterVm model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Doctors = new SelectList(_db.DoctorMasters.ToList(), "Id", "Name","Department");
            return View(model); 
        }
        var userId = "PAT" + System.DateTime.Now.Ticks.ToString().Substring(10);
        var pwd = System.Guid.NewGuid().ToString().Replace("-", "").Substring(0,8);
        var pat = new Patient 
        {
            PatientName = model.PatientName, 
            Date = model.Date,
            MobileNo = model.MobileNo, 
            Address = model.Address,
            Complaint = model.Complaint, 
            DoctorId = model.DoctorId,
            UserId = userId,
            PlainPassword = pwd
        };
        _db.Patients.Add(pat);
        await _db.SaveChangesAsync();
        TempData["RegisterMessage"] = $"Registration successful. Your UserId: {userId} and Password: {pwd} (save it).";
        return RedirectToAction(nameof(PatientRegister));
    }

    public IActionResult PatientLogin() => View();
    [HttpPost] public async Task<IActionResult> PatientLogin(PatientLoginVm vm) {
        if (!ModelState.IsValid) return View(vm);
        var pat = _db.Patients.FirstOrDefault(p => p.UserId == vm.UserId && p.PlainPassword == vm.Password);
        if (pat == null) { ModelState.AddModelError(string.Empty, "Invalid credentials"); return View(vm); }
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, pat.UserId), new Claim(ClaimTypes.Name, pat.PatientName), new Claim(ClaimTypes.Role, "Patient") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new System.Security.Claims.ClaimsPrincipal(identity));
        return RedirectToAction("Index", "Patient");
    }

    public IActionResult DoctorLogin() => View();
    [HttpPost] public async Task<IActionResult> DoctorLogin(DoctorLoginVm vm) {
        if (!ModelState.IsValid) return View(vm);
        var doc = _db.DoctorMasters.FirstOrDefault(d => d.Username == vm.Username && d.Password == vm.Password);
        if (doc == null) { ModelState.AddModelError(string.Empty, "Invalid credentials"); return View(vm); }
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, doc.Username), new Claim(ClaimTypes.Name, doc.Name), new Claim(ClaimTypes.Role, "Doctor") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new System.Security.Claims.ClaimsPrincipal(identity));
        return RedirectToAction("Index", "Doctor");
    }

    public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); return RedirectToAction("Index", "Home"); }
}
