using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Employee;
using SophieHR.Api.Services;
using System.Web.Http.Description;
using ApiExplorerSettingsAttribute = Microsoft.AspNetCore.Mvc.ApiExplorerSettingsAttribute;

namespace SophieHR.Api.Controllers
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JwtSettings jwtSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;

        public AccountController(JwtSettings jwtSettings, UserManager<ApplicationUser> userManager, ApplicationDbContext context, IMapper mapper, IEmailSender emailSender, ILogger<AccountController> logger)
        {
            this.jwtSettings = jwtSettings;
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        [ResponseType(typeof(UserTokens))]
        public async Task<IActionResult> GetToken(UserLogins userLogins)
        {
            try
            {
                _logger.LogInformation($"{nameof(AccountController)} > {nameof(GetToken)} Finding user with username {userLogins.UserName}");
                var Token = new UserTokens();
                var user = await _userManager.FindByNameAsync(userLogins.UserName);
                if (user == null)
                {
                    _logger.LogWarning("{nameof} Someone is trying to log in with an account that doesn't exist: {username}", nameof(AccountController), userLogins.UserName);
                    return NotFound("Invalid Username or password");
                }
                _logger.LogInformation($"User found...getting roles.");
                var roles = await _userManager.GetRolesAsync(user);
                var _passwordHasher = new PasswordHasher<ApplicationUser>();
                if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userLogins.Password) == PasswordVerificationResult.Success)
                {
                    _logger.LogInformation($"{nameof(AccountController)} User authentication passed. Generating JWT Payload...");
                    var userExtra = await _context.Employees.Where(x => x.Id == user.Id).Select(x => new { CompanyId = x.CompanyId, DepartmentId = x.DepartmentId }).FirstOrDefaultAsync();

                    Token = JwtHelpers.JwtHelpers.GenTokenkey(new UserTokens()
                    {
                        Email = user.Email,
                        UserName = user.UserName,
                        Id = user.Id,
                        Role = roles.First(),
                        CompanyId = userExtra?.CompanyId,
                        DepartmentId = userExtra?.DepartmentId
                    }, jwtSettings);
                }
                else
                {
                    _logger.LogWarning("{nameof} Attempt made to log in to valid username {username} with bad password {password}", nameof(AccountController), user.UserName, userLogins.Password);
                    return BadRequest("Invalid Username or password");
                }
                _logger.LogInformation($"{nameof(AccountController)} Payload generated for {Token.UserName}...returning.");
                return Ok(Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{nameof} An Exception was thrown {message}", nameof(AccountController), ex.Message);
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
            _logger.LogInformation($"{nameof(AccountController)} > {nameof(RegisterNewAdminUser)} Registering New Admin User");
            if (!ModelState.IsValid)
            {
                _logger.LogError($"{nameof(AccountController)} Invalid form data passed in.");
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
                    await _emailSender.SendEmailAsync(user.Email, "New User Registered", "You have had a user registered with this email address. Gratz!");
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
        [ResponseType(typeof(List<EmployeeListDto>))]
        [Authorize(Roles = "Admin, Manager")]
        public IActionResult GetList()
        {
            var users = _context.Employees.AsEnumerable();
            if (User.IsInRole("Manager"))
            {
                // Only retrieve employees the manager can access:
                var managerId = _userManager.GetUserId(User);
                users = users.Where(x => x.Manager.Id == Guid.Parse(managerId));
            }

            return Ok(_mapper.Map<List<EmployeeListDto>>(users.ToList()));
        }

        [HttpGet("GetListOfManagers")] // ToDo - delete!
        [ResponseType(typeof(List<string>))]
        [AllowAnonymous]
        public async Task<IActionResult> GetListOfManagersAsync()
        {
            var managers = await _userManager.GetUsersInRoleAsync("Manager");

            return Ok(_mapper.Map<List<string>>(managers.Select(x => x.UserName).ToList()));
        }
    }
}