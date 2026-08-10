using EnterpriseEmployeeManagement.Application.DTOs.Authentication;
using EnterpriseEmployeeManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnterpriseEmployeeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthController(
            IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        // ============================================
        // REGISTER
        // ============================================

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterRequest request)
        {
            var result =
                await _authenticationService.RegisterAsync(request);

            return Ok(result);
        }

        // ============================================
        // LOGIN
        // ============================================

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginRequest request)
        {
            var result =
                await _authenticationService.LoginAsync(request);

            return Ok(result);
        }

        // ============================================
        // REFRESH TOKEN
        // ============================================

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken(
            RefreshTokenRequest request)
        {
            var result =
                await _authenticationService.RefreshTokenAsync(request);

            return Ok(result);
        }

        // ============================================
        // LOGOUT
        // ============================================

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var employeeIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(employeeIdClaim, out var employeeId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user."
                });
            }

            await _authenticationService.LogoutAsync(employeeId);

            return Ok(new
            {
                message = "Successfully logged out."
            });
        }

        // ============================================
        // PROTECTED PROFILE
        // ============================================

        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            return Ok(new
            {
                Message = "You are authenticated!",

                UserId =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value,

                Email =
                    User.FindFirst(ClaimTypes.Email)?.Value,

                Role =
                    User.FindFirst(ClaimTypes.Role)?.Value
            });
        }

        // ============================================
        // ADMIN ONLY
        // ============================================

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminOnly()
        {
            return Ok(new
            {
                Message =
                    "Welcome Admin! You have access to this endpoint.",

                UserId =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value,

                Email =
                    User.FindFirst(ClaimTypes.Email)?.Value,

                Role =
                    User.FindFirst(ClaimTypes.Role)?.Value
            });
        }

        // ============================================
        // USER ONLY
        // ============================================

        [Authorize(Roles = "User")]
        [HttpGet("user")]
        public IActionResult UserOnly()
        {
            return Ok(new
            {
                Message =
                    "Welcome User! You have access to this endpoint.",

                UserId =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value,

                Email =
                    User.FindFirst(ClaimTypes.Email)?.Value,

                Role =
                    User.FindFirst(ClaimTypes.Role)?.Value
            });
        }
    }
}