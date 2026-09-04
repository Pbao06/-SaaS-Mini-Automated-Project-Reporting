namespace SaaS.Api.DTOs.Common
{
    //Non-generic
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public static ApiResponse SuccessResponse(string? message = null) => new()
        {
            Success = true,
            Message = message!
        };
        public static ApiResponse ErrorResponse(string message) => new()
        {
            Success = false,
            Message = message,
        };
    }
    //Generic
    public class ApiResponse<T> : ApiResponse
    {
        public T? Data
        {
            get;
            set;
        }
        public static ApiResponse<T> SuccessResponse(T? data, string? message=null) => new()
        {
            Success = true,
            Data = data,
            Message = message!
        };
    }
}
