using Concord.Application.DTO.Identity;
using Concord.Application.Services.Account.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concord.API.Controllers.Accounts
{
    /// <summary>
    /// Controller for managing provider account operations including authentication, registration, and account management.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IProviderAccountService _providerAccountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IProviderAccountService providerAccountService,
            ILogger<AccountController> logger)
        {
            _providerAccountService = providerAccountService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the current authenticated user's information.
        /// </summary>
        /// <returns>Current user information including profile details and permissions.</returns>
        /// <response code="200">Returns the current user information successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="404">User not found.</response>
        /// <response code="500">Internal server error occurred.</response>
        [Authorize]
        [HttpGet("GetCurrentUser")]
        [ProducesResponseType(typeof(UserInfo), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UserInfo>> GetCurrentUser()
        {
            try
            {
                var userInfo = await _providerAccountService.GetCurrentUser(User);

                if (userInfo == null)
                {
                    return NotFound("User information not found.");
                }

                return Ok(userInfo);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized("Access denied.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving user information.");
            }
        }

        /// <summary>
        /// Authenticates a user and returns authentication tokens.
        /// </summary>
        /// <param name="loginDto">Login credentials containing username/email and password.</param>
        /// <returns>User information with authentication tokens.</returns>
        /// <response code="200">Login successful, returns user data with tokens.</response>
        /// <response code="400">Invalid login credentials or malformed request.</response>
        /// <response code="401">Authentication failed - invalid credentials.</response>
        /// <response code="423">Account is locked due to too many failed attempts.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPost("Login")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(423)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userDto = await _providerAccountService.Login(loginDto);

                if (userDto == null)
                {
                    return Unauthorized("Invalid credentials.");
                }

                return Ok(userDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized("Invalid credentials.");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("locked"))
            {
                return StatusCode(423, "Account is temporarily locked. Please try again later.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest("Invalid login data provided.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Registers a new provider account.
        /// </summary>
        /// <param name="registerDto">Registration data including user details and credentials.</param>
        /// <returns>Success status of the registration operation.</returns>
        /// <response code="200">Registration successful.</response>
        /// <response code="400">Invalid registration data or validation errors.</response>
        /// <response code="409">User already exists with the provided email.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPost("Register")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> Register([FromBody] ProviderRegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _providerAccountService.Regitser(registerDto);

                if (result)
                {
                    return Ok(true);
                }

                return BadRequest("Registration failed.");
            
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                return Conflict("An account with this email already exists.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest("Invalid registration data provided.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Refreshes authentication tokens using a valid refresh token.
        /// </summary>
        /// <param name="tokenApiDto">Token data containing access and refresh tokens.</param>
        /// <returns>New set of authentication tokens.</returns>
        /// <response code="200">Tokens refreshed successfully.</response>
        /// <response code="400">Invalid token data or malformed request.</response>
        /// <response code="401">Invalid or expired refresh token.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPost("Refresh")]
        [ProducesResponseType(typeof(TokenApiDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<TokenApiDto>> Refresh([FromBody] TokenApiDto tokenApiDto)
        {
            try
            {
                if (!ModelState.IsValid || tokenApiDto == null)
                {
                    return BadRequest("Invalid token data provided.");
                }

                var refreshedTokens = await _providerAccountService.Refresh(tokenApiDto);

                if (refreshedTokens == null)
                {
                    return Unauthorized("Invalid or expired refresh token.");
                }

                return Ok(refreshedTokens);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized("Invalid or expired refresh token.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest("Invalid token data provided.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred during token refresh.");
            }
        }

        /// <summary>
        /// Changes the password for the authenticated user.
        /// </summary>
        /// <param name="input">Password change data including current and new passwords.</param>
        /// <returns>Success status of the password change operation.</returns>
        /// <response code="200">Password changed successfully.</response>
        /// <response code="400">Invalid password data or validation errors.</response>
        /// <response code="401">User is not authenticated or current password is incorrect.</response>
        /// <response code="500">Internal server error occurred.</response>
        [Authorize]
        [HttpPost("ChangePassword")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> ChangePassword([FromBody] ChangePasswordDto input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _providerAccountService.ChangePassword(input);

                if (result)
                {
                    return Ok(true);
                }

                return BadRequest("Password change failed.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized("Current password is incorrect.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest("Invalid password data provided.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while changing password.");
            }
        }

        /// <summary>
        /// Initiates a password reset process by sending a reset email.
        /// </summary>
        /// <param name="email">Email address of the account to reset password for.</param>
        /// <returns>Success status of the password reset initiation.</returns>
        /// <response code="200">Password reset email sent successfully (or email not found but returns success for security).</response>
        /// <response code="400">Invalid email format.</response>
        /// <response code="429">Too many password reset requests. Please wait before trying again.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("ForgotPassword")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(429)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> ForgotPassword([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest("Invalid email format.");
                }

                var result = await _providerAccountService.ForgotPassword(email);

                return Ok(true);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("rate limit"))
            {
                return StatusCode(429, "Too many password reset requests. Please wait before trying again.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest("Invalid email provided.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing password reset request.");
            }
        }

        /// <summary>
        /// Completes the password reset process using a reset token.
        /// </summary>
        /// <param name="model">Password reset data including reset token and new password.</param>
        /// <returns>Success status of the password reset operation.</returns>
        /// <response code="200">Password reset successfully.</response>
        /// <response code="400">Invalid reset data, expired token, or validation errors.</response>
        /// <response code="404">Invalid or expired reset token.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPost("ResetPassword")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> ResetPassword([FromBody] ResetPasswordDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _providerAccountService.ResetPassword(model);

                if (result)
                {
                    return Ok(true);
                }

                return NotFound("Invalid or expired reset token.");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("expired"))
            {
                return NotFound("Reset token has expired.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest("Invalid reset data provided.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while resetting password.");
            }
        }

        /// <summary>
        /// Verifies a provider account (Admin only operation).
        /// </summary>
        /// <param name="tenantId">Unique identifier of the tenant/provider to verify.</param>
        /// <returns>Success status of the account verification operation.</returns>
        /// <response code="200">Account verified successfully.</response>
        /// <response code="400">Invalid tenant ID provided.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="404">Tenant account not found.</response>
        /// <response code="500">Internal server error occurred.</response>
        [Authorize("Admin")]
        [HttpPut("VerifyAccount")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> VerifyAccount([FromQuery] Guid tenantId)
        {
            try
            {
                if (tenantId == Guid.Empty)
                {
                    return BadRequest("Invalid tenant ID provided.");
                }

                var result = await _providerAccountService.VerifyAccount(tenantId);

                if (result)
                {
                    return Ok(true);
                }

                return NotFound("Tenant account not found.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid("Insufficient permissions to verify accounts.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest("Invalid verification data provided.");
            }
            catch (Exception ex)
            { 
                return StatusCode(500, "An error occurred while verifying account.");
            }
        }
    }
}
