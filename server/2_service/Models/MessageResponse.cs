namespace _2_service.Models;

public class MessageResponse(string content, string sender)
{
    public string Content { get; set; } = content;
    public string Sender { get; set; } = sender;
}