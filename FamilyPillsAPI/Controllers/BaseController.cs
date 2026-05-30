using Microsoft.AspNetCore.Mvc;
using System;
using FamilyPillsAPI.Models;

namespace FamilyPillsAPI.Controllers
{
    /// <summary>
    /// Base controller class with common response methods
    /// </summary>
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// Return success response with data
        /// </summary>
        protected ActionResult<ApiResponse<T>> SuccessResponse<T>(T data, string message = "Operation successful")
        {
            return Ok(new ApiResponse<T>(message, data));
        }

        /// <summary>
        /// Return success response (201 Created)
        /// </summary>
        protected ActionResult<ApiResponse<T>> CreatedResponse<T>(T data, string message = "Resource created successfully")
        {
            return Created("", new ApiResponse<T>(message, data));
        }

        /// <summary>
        /// Return error response (400)
        /// </summary>
        protected ActionResult<ApiResponse<T>> BadRequestResponse<T>(string message, string errorCode, string field = null)
        {
            var error = new ErrorInfo(errorCode, message, field);
            return BadRequest(new ApiResponse<T>(message, error));
        }

        /// <summary>
        /// Return not found response (404)
        /// </summary>
        protected ActionResult<ApiResponse<T>> NotFoundResponse<T>(string message, string errorCode = "NOT_FOUND")
        {
            var error = new ErrorInfo(errorCode, message);
            return NotFound(new ApiResponse<T>(message, error));
        }

        /// <summary>
        /// Return unauthorized response (401)
        /// </summary>
        protected ActionResult<ApiResponse<T>> UnauthorizedResponse<T>(string message = "Unauthorized", string errorCode = "UNAUTHORIZED")
        {
            var error = new ErrorInfo(errorCode, message);
            return Unauthorized(new ApiResponse<T>(message, error));
        }

        /// <summary>
        /// Return server error response (500)
        /// </summary>
        protected ActionResult<ApiResponse<T>> ServerErrorResponse<T>(string message, string errorCode = "SERVER_ERROR", Exception ex = null)
        {
            var details = ex?.Message ?? message;
            var error = new ErrorInfo(errorCode, details);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<T>(message, error));
        }
    }
}
