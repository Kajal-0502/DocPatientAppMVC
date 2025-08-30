using System.ComponentModel.DataAnnotations; 
namespace DocPatientAppMVC.ViewModels; 
public class PatientLoginVm 
{ 
    [Required] 
    public string UserId { get; set; } = null!; 
    [Required] 
    public string Password { get; set; } = null!; 
}