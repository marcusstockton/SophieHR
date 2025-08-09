using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Employee;
using SophieHR.Api.Services;
using System.Security.Claims;
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
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;

        public AccountController(JwtSettings jwtSettings, UserManager<ApplicationUser> userManager, ApplicationDbContext context, IEmailSender emailSender, ILogger<AccountController> logger)
        {
            this.jwtSettings = jwtSettings;
            _userManager = userManager;
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous, Produces(typeof(UserTokens)), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status400BadRequest),]
        public async Task<IActionResult> GetToken(UserLogins userLogins)
        {
            try
            {
                Log.Information($"{nameof(AccountController)} > {nameof(GetToken)} Finding user with username {userLogins.UserName}");
                var Token = new UserTokens();
                var user = await _userManager.FindByNameAsync(userLogins.UserName);
                if (user == null)
                {
                    _logger.LogWarning("{nameof} Someone is trying to log in with an account that doesn't exist: {username}", nameof(AccountController), userLogins.UserName);

                    return Problem(
                        type: $"https://httpstatuses.com/404",
                        title: "Invalid Username or password",
                        detail: "Invalid Username or password",
                        statusCode: StatusCodes.Status404NotFound);
                }
                _logger.LogInformation($"User found...getting roles.");
                var roles = await _userManager.GetRolesAsync(user);
                var _passwordHasher = new PasswordHasher<ApplicationUser>();
                if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userLogins.Password) == PasswordVerificationResult.Success)
                {
                    _logger.LogInformation($"{nameof(AccountController)} User authentication passed. Generating JWT Payload...");
                    var userExtra = await _context.Employees
                        .Where(x => x.Id == user.Id)
                        .Select(x => new { x.CompanyId, x.DepartmentId })
                        .FirstOrDefaultAsync();

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
                    return Problem(
                        type: $"https://httpstatuses.com/404",
                        title: "Invalid Username or password",
                        detail: "Invalid Username or password",
                        statusCode: StatusCodes.Status404NotFound);
                }
                _logger.LogInformation($"{nameof(AccountController)} Payload generated for {Token.UserName}...returning.");
                return Ok(Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{nameof} An Exception was thrown {message}", nameof(AccountController), ex.Message);
                //return BadRequest("Invalid Username or password");
                return Problem(
                    type: $"https://httpstatuses.com/500",
                    title: "Something went wrong...",
                    detail: string.Format("{0}", ex.Message),
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Registers a new User account
        /// </summary>
        /// <param name="userData"></param>
        /// <returns>JWT Auth token</returns>
        [HttpPost, Route("RegisterNewAdminUser"), Authorize(Roles = "Admin"), Produces(typeof(UserTokens))]
        public async Task<IActionResult> RegisterNewAdminUser(RegisterUserDto userData)
        {
            _logger.LogInformation($"{nameof(AccountController)} > {nameof(RegisterNewAdminUser)} Registering New Admin User");
            if (!ModelState.IsValid)
            {
                _logger.LogError($"{nameof(AccountController)} Invalid form data passed in.");
                //return BadRequest(userData);
                return Problem(
                    type: $"https://httpstatuses.com/400",
                    title: "Invalid data",
                    detail: string.Format("{0}", userData),
                    statusCode: StatusCodes.Status400BadRequest);
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
                //return BadRequest("A User with that email address already exists in the system.");
                return Problem(
                    type: $"https://httpstatuses.com/400",
                    title: "Existing User",
                    detail: "A User with that email address already exists in the system.",
                    statusCode: StatusCodes.Status400BadRequest);
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
                    //return BadRequest(result.Errors.Select(x => x.Description).ToList());
                    return Problem(
                    type: $"https://httpstatuses.com/400",
                    title: "Existing User",
                    detail: result.Errors.Select(x => x.Description).ToList().ToString(),
                    statusCode: StatusCodes.Status400BadRequest);
                }
            }
            catch (Exception ex)
            {
                //return BadRequest(ex.Message);
                return Problem(
                    type: $"https://httpstatuses.com/500",
                    title: "Something went wrong our end",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get List of UserAccounts
        /// </summary>
        /// <returns>List Of UserAccounts</returns>
        [HttpGet("GetListOfUsers")]
        [Produces(typeof(List<EmployeeListDto>))]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> GetListAsync()
        {
            var users = _context.Employees.AsEnumerable();
            if (User.IsInRole("Manager"))
            {
                // Only retrieve employees the manager can access:

                var user1 = _userManager.GetUserId(User);

                var us3 = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var sw1 = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var use = await _userManager.FindByEmailAsync(User.Identity.Name);

                var managerId = _userManager.GetUserId(User);
                //users = users.Where(x => x.Manager != null && x.Manager?.Id == managerId);
            }
            var results = users.Select(x => new EmployeeListDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                WorkEmailAddress = x.WorkEmailAddress,
                DepartmentId = x.Department.Id,
                CompanyId = x.CompanyId,
                Company = new Models.DTOs.Company.CompanyIdNameDto                 {
                    Id = x.Company.Id,
                    Name = x.Company.Name
                },
                Address = new EmployeeAddress
                {
                    Line1 = x.Address.Line1,
                    Line2 = x.Address.Line2,
                    Postcode = x.Address.Postcode
                },
                DateOfBirth = x.DateOfBirth,
                HolidayAllowance = x.HolidayAllowance,
                JobTitle = x.JobTitle,
                MiddleName = x.MiddleName,
                PersonalEmailAddress = x.PersonalEmailAddress,
                StartOfEmployment = x.StartOfEmployment,
                PersonalMobileNumber = x.PersonalMobileNumber,
                WorkMobileNumber = x.WorkMobileNumber,
                WorkPhoneNumber = x.WorkPhoneNumber
            }).ToList();
            return Ok(results);
        }

        [HttpGet("GetListOfManagers")] // ToDo - delete!
        [Produces(typeof(List<string>))]
        [AllowAnonymous]
        public async Task<IActionResult> GetListOfManagersAsync()
        {
            _logger.LogInformation($"{nameof(GetListOfManagersAsync)} Called");
            var managers = await _userManager.GetUsersInRoleAsync("Manager");

            var results = managers.Select(x => x.UserName).ToList();

            return Ok(results);
        }

        [HttpGet("GetListOfCompanyAdmin")] // ToDo - delete!
        [Produces(typeof(List<string>))]
        [AllowAnonymous]
        public async Task<IActionResult> GetListOfCompanyAdminsAsync()
        {
            _logger.LogInformation($"{nameof(GetListOfCompanyAdminsAsync)} Called");
            var companyAdmins = await _userManager.GetUsersInRoleAsync("CompanyAdmin");

            return Ok(companyAdmins.Select(x => x.UserName).ToList());
        }
    }
}