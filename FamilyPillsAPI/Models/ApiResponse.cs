using System;

namespace FamilyPillsAPI.Models
{
    /// <summary>
    /// Generic API Response wrapper for all endpoints
    /// </summary>
    public class ApiResponse<T>
    {
        public string Message { get; set; }
        public T? Data { get; set; }
        public ErrorInfo? Error { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ApiResponse(string message, T data)
        {
            Message = message;
            Data = data;
            Error = null;
        }

        public ApiResponse(string message, ErrorInfo error)
        {
            Message = message;
            Data = default;
            Error = error;
        }

        public ApiResponse(string message, T data, ErrorInfo error)
        {
            Message = message;
            Data = data;
            Error = error;
        }

        public bool IsSuccess => Error == null;
    }

    /// <summary>
    /// Error information details
    /// </summary>
    public class ErrorInfo
    {
        public string Code { get; set; }
        public string Details { get; set; }
        public string? Field { get; set; }

        public ErrorInfo(string code, string details)
        {
            Code = code;
            Details = details;
        }

        public ErrorInfo(string code, string details, string? field)
        {
            Code = code;
            Details = details;
            Field = field;
        }
    }
}
