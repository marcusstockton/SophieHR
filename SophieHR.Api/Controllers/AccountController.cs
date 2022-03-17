using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.User;
using SophieHR.Api.Services;
using System.Web.Http.Description;

namespace SophieHR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JwtSettings jwtSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;

        public AccountController(JwtSettings jwtSettings, UserManager<ApplicationUser> userManager, ApplicationDbContext context, IMapper mapper, IEmailSender emailSender)
        {
            this.jwtSettings = jwtSettings;
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
            _emailSender = emailSender;
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
                if(user == null)
                {
                    return NotFound("Invalid Username or password");
                }
                var roles = await _userManager.GetRolesAsync(user);
                var _passwordHasher = new PasswordHasher<ApplicationUser>();
                if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userLogins.Password) == PasswordVerificationResult.Success)
                {
                    var companyId = await _context.Employees.Where(x => x.Id == user.Id).Select(x => x.CompanyId).FirstOrDefaultAsync();
                    Token = JwtHelpers.JwtHelpers.GenTokenkey(new UserTokens()
                    {
                        Email = user.Email,
                        UserName = user.UserName,
                        Id = user.Id,
                        Role = roles.First(),
                        CompanyId = companyId
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
                return BadRequest("Invalid Username or password");
            }
        }

        /// <summary>
        /// Registers a new User account
        /// </summary>
        /// <param name="userData"></param>
        /// <returns>JWT Auth token</returns>
        [HttpPost, Route("RegisterNewAdminUser"), Authorize(Roles = "Admin"), ResponseType(typeof(UserTokens))]
        public async Task<IActionResult> RegisterNewAdminUser(RegisterUserDto userData)
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
                UserName = userData.EmailAddress,
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
                    await _emailSender.SendEmailAsync( user.Email, "New User Registered", "You have had a user registered with this email address. Gratz!");
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
        [HttpGet("GetListOfUsers")]
        [ResponseType(typeof(List<UserDto>))]
        [Authorize(Roles = "Admin, Manager")]
        public IActionResult GetList()
        {
            var users = _userManager.Users.AsEnumerable();
            if (User.IsInRole("Manager"))
            {
                // Only retrieve employees the manager can access:
                
            }
            
            return Ok(_mapper.Map<List<UserDto>>(users.ToList()));
        }
    }
}