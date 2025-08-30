using System;
using System.ComponentModel.DataAnnotations;
namespace DocPatientAppMVC.ViewModels;
public class PatientRegisterVm 
{ 
    [Required] 
    public string PatientName { get; set; } 
    [Required]
    public DateTime Date { get; set; } 

    [Required]
    public string MobileNo { get; set; }
    public string Address { get; set; } 
    public string Complaint { get; set; }

    [Required] 
    public int DoctorId { get; set; } 
}