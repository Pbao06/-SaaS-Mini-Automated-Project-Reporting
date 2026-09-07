using SaaS.Api.Enum;

namespace SaaS.Api.DTOs.Common
{
    //Non-generic class tra ve ko data
    public class ServicesResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public ResultStatus ResultStatus;
        public static ServicesResponse SuccessResponse(
        string? message = null)
        => new()
        {
            Success = true,
            Message = message!,
            ResultStatus = ResultStatus.Ok
        };

        public static ServicesResponse ErrorResponse(
            string message,
            ResultStatus resultStatus)
            => new()
            {
                Success = false,
                Message = message,
                ResultStatus = resultStatus
            };
    }
    //Generic class tra ve co data
    public class ServicesResponse<T> : ServicesResponse
    {
        public T? Data { get; set; }
        public static ServicesResponse<T> SuccessResponse(T? data, string? message = null) => new()
        {
            Success = true,
            Data = data,
            Message = message!,
            ResultStatus = ResultStatus.Ok
        };
        public static new ServicesResponse<T> ErrorResponse(
           string message,
           ResultStatus resultStatus)
           => new()
           {
               Data = default,
               Success = false,
               Message = message,
               ResultStatus = resultStatus
           };
    }
}
