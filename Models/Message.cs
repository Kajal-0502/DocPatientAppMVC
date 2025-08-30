
namespace DocPatientAppMVC.Models;
public class Message 
{ 
    public int Id { get; set; }
    public string FromUserId { get; set; } 
    public string ToUserId { get; set; }
    public string Text { get; set; } 
    public System.DateTime SentAtUtc { get; set; }
}
