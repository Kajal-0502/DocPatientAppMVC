using System.ComponentModel.DataAnnotations; 
namespace DocPatientAppMVC.ViewModels; 
public class DoctorLoginVm 
{ 
    [Required] 
    public string Username { get; set; } = null!; 
    [Required] 
    public string Password { get; set; } = null!; 
}