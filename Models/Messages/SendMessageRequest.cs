namespace Kabutar_WPF.Models.Messages
{
    public class SendMessageRequest
    {
        public long ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
