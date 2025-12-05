using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace FCG.Payments.WebApi.Models
{
    [ExcludeFromCodeCoverage]
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; } = default!;
        public List<string> ErrorMessages { get; set; } = default!;

        public static ApiResponse<T> SuccesResponse(T data)
        {
            return new ApiResponse<T> { Success = true, Data = data };
        }

        public static ApiResponse<T> ErrorResponse(List<string> errorMessages, HttpStatusCode statusCode)
        {
            return new ApiResponse<T>
            {
                Success = false,
                ErrorMessages = errorMessages,
            };
        }
    }
}
