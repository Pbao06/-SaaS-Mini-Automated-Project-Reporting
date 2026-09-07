
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SaaS.Api.Enum;
using static Google.Apis.Requests.BatchRequest;
namespace SaaS.Api.DTOs.Common
{
    public static class ServicesResponseExtensions
    {
        private static IActionResult FromResultStatusToHTTP(this ServicesResponse response)
        {
            return response.ResultStatus switch
            {
                ResultStatus.Ok =>
                    new OkObjectResult(response),

                ResultStatus.NotFound =>
                    new NotFoundObjectResult(response),

                ResultStatus.ValidationError =>
                    new BadRequestObjectResult(response),

                ResultStatus.Unauthorized =>
                    new UnauthorizedObjectResult(response),

                ResultStatus.Forbidden =>
                    new ObjectResult(response)
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    },

                ResultStatus.Conflict =>
                    new ConflictObjectResult(response),
                //Con lai 
                _ =>
                new ObjectResult(response)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
        public static IActionResult ToHTTPResponse(this ServicesResponse ServicesResponse)
        {
            //Chuyen doi sang tu ServicesResponse sang ApiResponse de controller tra lai response cho request den
            var response = ServicesResponse.Success ?
                  ApiResponse.SuccessResponse(ServicesResponse.Message)
                : ApiResponse.ErrorResponse(ServicesResponse.Message);
            //Tra ve ma Http tuong ung 
            return FromResultStatusToHTTP(ServicesResponse);
           
        }
        public static IActionResult ToHTTPResponse<T>(this ServicesResponse<T> ServicesResponse)
        {
            //Chuyen doi sang tu ServicesResponse sang ApiResponse de controller tra lai response cho request den
            var response = ServicesResponse.Success ?
                  ApiResponse<T>.SuccessResponse(ServicesResponse.Data, ServicesResponse.Message)
                : ApiResponse<T>.ErrorResponse(ServicesResponse.Message);
            //Tra ve ma Http tuong ung 
            return FromResultStatusToHTTP(ServicesResponse);
        }
    }
}
