namespace Moneyman.Domain
{
    public enum StatusCode
    {
        Success = 200,
        NoContent = 204,
        BadRequest = 400,
        InternalError = 500,
        NotFound = 404
    }

    public class ApiResponse<T>
    {
        public bool Success { get{ return StatusCode == StatusCode.Success || StatusCode == StatusCode.NoContent; } }
        public StatusCode StatusCode {get; set;}
        public string Message { get; set; }
        public T Payload { get; set; }

        public ApiResponse(StatusCode statusCode, string message, T payload)
        {
            StatusCode = statusCode;
            Message = message;
            Payload = payload;
        }
    }

    public static class ApiResponse
    {
        public static ApiResponse<T> Success<T>(this T data, string message)
        {
            message ??= "Success";
            return new ApiResponse<T>(StatusCode.Success, message, data);
        }

        public static ApiResponse<T> ValidationError<T>(string message)
        {
            message  ??= "Validation Error";
            return new ApiResponse<T>(StatusCode.BadRequest, message, default);
        }

        public static ApiResponse<T> NotFound<T>(string message)
        {
            message ??= "Not Found";
            return new ApiResponse<T>(StatusCode.NotFound, message, default);
        }
        
        public static ApiResponse<T> NoContent<T>(string message = null)
        {
            message ??= "No Content";
            return new ApiResponse<T>(StatusCode.NoContent, message, default);
        }
    }

}