using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.User;
using System.Web.Http.Description;

namespace SophieHR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JwtSettings jwtSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        public readonly IMapper _mapper;

        public AccountController(JwtSettings jwtSettings, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            this.jwtSettings = jwtSettings;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpPost]
        [AllowAnonymous]
        [ResponseType(typeof(UserTokens))]
        public async Task<IActionResult> GetToken(UserLogins userLogins)
        {
            try
            {
                var Token = new UserTokens();
                var user = await _userManager.FindByNameAsync(userLogins.UserName);
                var _passwordHasher = new PasswordHasher<ApplicationUser>();
                if (user != null && _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userLogins.Password) == PasswordVerificationResult.Success)
                {
                    Token = JwtHelpers.JwtHelpers.GenTokenkey(new UserTokens()
                    {
                        Email = user.Email,
                        UserName = user.UserName,
                        Id = user.Id,
                    }, jwtSettings);
                }
                else
                {
                    return BadRequest("Invalid Username or password");
                }
                return Ok(Token);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Registers a new User account
        /// </summary>
        /// <param name="userData"></param>
        /// <returns>JWT Auth token</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("RegisterNewUser")]
        [ResponseType(typeof(UserTokens))]
        public async Task<IActionResult> RegisterNewUser(RegisterUserDto userData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(userData);
            }
            var user = new ApplicationUser
            {
                Email = userData.EmailAddress,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                UserName = userData.EmailAddress
            };
            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser != null)
            {
                return BadRequest("A User with that email address already exists in the system.");
            }
            try
            {
                var result = await _userManager.CreateAsync(user, userData.Password);
                if (result == IdentityResult.Success)
                {
                    var token = JwtHelpers.JwtHelpers.GenTokenkey(new UserTokens()
                    {
                        Email = user.Email,
                        UserName = user.UserName,
                        Id = user.Id,
                    }, jwtSettings);

                    return Ok(token);
                }
                else
                {
                    return BadRequest(result.Errors.Select(x => x.Description).ToList());
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get List of UserAccounts
        /// </summary>
        /// <returns>List Of UserAccounts</returns>
        [HttpGet]
        [ResponseType(typeof(List<UserDto>))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetList()
        {
            var users = _userManager.Users.ToList();

            return Ok(_mapper.Map<List<UserDto>>(users));
        }
    }
}