namespace Baigiamasis.DTOs.Common
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public int StatusCode { get; set; }

        // Add parameterless constructor for deserialization
        public ApiResponse() { }

        private ApiResponse(bool isSuccess, T data, string message, int statusCode)
        {
            IsSuccess = isSuccess;
            Data = data;
            Message = message;
            StatusCode = statusCode;
        }

        public static ApiResponse<T> Success(T data, string message = "") 
            => new ApiResponse<T>(true, data, message, 200);

        public static ApiResponse<T> Created(T data, string message = "")
            => new ApiResponse<T>(true, data, message, 201);

        public static ApiResponse<T> BadRequest(string message)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Data = default,
                StatusCode = 400
            };
        }

        public static ApiResponse<T> Unauthorized(string message)
            => new ApiResponse<T>(false, default, message, 401);

        public static ApiResponse<T> Forbidden(string message)
            => new ApiResponse<T>(false, default, message, 403);

        public static ApiResponse<T> NotFound(string message)
            => new ApiResponse<T>(false, default, message, 404);

        public static ApiResponse<T> Failure(string message, int statusCode = 500)
            => new ApiResponse<T>(false, default, message, statusCode);
    }
}
