namespace Moneyman.Domain.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Payload { get; set; }

        public ApiResponse(bool success, string message, T payload)
        {
            Success = success;
            Message = message;
            Payload = payload;
        }
    }

}