namespace MvcClinic.DTOs
{
    public class ErrorViewDTO
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
