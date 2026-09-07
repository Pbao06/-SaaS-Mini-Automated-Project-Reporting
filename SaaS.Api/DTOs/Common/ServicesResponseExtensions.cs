using Microsoft.AspNetCore.Mvc;
using SaaS.Api.Enum;

namespace SaaS.Api.DTOs.Common
{
    public static class ServicesResponseExtensions
    {
        private static int FromResultStatusToHTTP(ResultStatus resultStatus)
        {
            return resultStatus switch
            {
                ResultStatus.Ok => StatusCodes.Status200OK,
                ResultStatus.NotFound => StatusCodes.Status404NotFound,
                ResultStatus.ValidationError => StatusCodes.Status400BadRequest,
                ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,
                ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
                ResultStatus.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        public static IActionResult ToHTTPResponse(this ServicesResponse response)
        {
            var apiResponse = response.Success
                ? ApiResponse.SuccessResponse(response.Message)
                : ApiResponse.ErrorResponse(response.Message);

            return new ObjectResult(apiResponse)
            {
                StatusCode = FromResultStatusToHTTP(response.ResultStatus)
            };
        }

        public static IActionResult ToHTTPResponse<T>(this ServicesResponse<T> response)
        {
            ApiResponse<T> apiResponse = response.Success
                ? ApiResponse<T>.SuccessResponse(response.Data, response.Message)
                : ApiResponse<T>.ErrorResponse(response.Message);

            return new ObjectResult(apiResponse)
            {
                StatusCode = FromResultStatusToHTTP(response.ResultStatus)
            };
        }
    }
}
