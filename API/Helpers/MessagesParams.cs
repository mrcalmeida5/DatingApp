
namespace API.Helpers
{
    public class MessagesParams : PaginationParams
    {
        public int UserId { get; set; }
        public string Container { get; set; } = "Unread";
    }
}