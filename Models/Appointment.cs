
namespace DocPatientAppMVC.Models;
public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    public int DoctorId { get; set; }
    public DoctorMaster? Doctor { get; set; }
    public System.DateTime SlotUtc { get; set; }
    public string Status { get; set; } = "Pending"; 
}
