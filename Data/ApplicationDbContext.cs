
using DocPatientAppMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace DocPatientAppMVC.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<DoctorMaster> DoctorMasters { get; set; }
    public DbSet<Patient> Patients { get; set; } 
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Message> Messages { get; set; } 
}
