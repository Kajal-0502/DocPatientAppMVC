using System;

namespace DocPatientAppMVC.Models;
public class Patient 
{
    public int Id { get; set; }
    public string PatientName { get; set; }
    public DateTime Date { get; set; } 
    public string MobileNo { get; set; } 
    public string Address { get; set; }
    public string Complaint { get; set; }
    public int DoctorId { get; set; }
    public DoctorMaster? Doctor { get; set; }
    public string UserId { get; set; } = null!;
    public string PlainPassword { get; set; } = null!; 
}
