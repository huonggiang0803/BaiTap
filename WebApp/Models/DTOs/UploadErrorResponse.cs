namespace WebApp.Models.DTOs
{
    public class UploadErrorResponse
    {
        public string Message { get; set; }
        public List<string> Errors { get; set; }
    }
}
