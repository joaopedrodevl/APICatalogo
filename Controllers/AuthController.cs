using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService; // Interface do servi�o de token
        private readonly UserManager<ApplicationUser> _userManager; // Interface do gerenciador de usu�rios
        private readonly RoleManager<IdentityRole> _roleManager; // Interface do gerenciador de roles
        private readonly IConfiguration _configuration; // Interface de configura��o
        private readonly ILogger<AuthController> _logger; // Interface de log

        public AuthController(ITokenService tokenService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }  

        [HttpPost]
        [Route("CreateRole")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            var roleExist = await _roleManager.RoleExistsAsync(roleName); // Verifica se a role existe.

            if (!roleExist)
            {
                var role = await _roleManager.CreateAsync(new IdentityRole(roleName)); // Cria a role.

                if (role.Succeeded)
                {
                    _logger.LogInformation(1, "Role created successfully!");
                    return Ok(new { status = "success", message = $"Role {roleName} created successfully!" });
                }

                _logger.LogError(1, "Role creation failed!");
                return StatusCode(StatusCodes.Status500InternalServerError, new { status = "error", message = "Role creation failed!" });
            }

            _logger.LogInformation(1, "Role already exists!");
            return StatusCode(StatusCodes.Status500InternalServerError, new { status = "error", message = "Role already exists!" });
        }

        [HttpPost]
        [Route("AddUserToRole")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> AddUserToRole(string email, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var result = await _userManager.AddToRoleAsync(user, roleName);

                if (result.Succeeded)
                {
                    _logger.LogInformation(1, $"User {user.Email} added to the {roleName} role");
                    return Ok(new {
                        status = "success",
                        message = $"User {user.Email} added to the {roleName} role"
                    });
                }

                return StatusCode(400, new {
                    status = "error",
                    message = $"Unable to add user {user.Email} to the {roleName} role"
                });
            }

            return BadRequest(new {error = "Unable to find user"});
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModelDTO modelDTO)
        {
            var user = await _userManager.FindByNameAsync(modelDTO.UserName!);
            
            if (user is not null && await _userManager.CheckPasswordAsync(user, modelDTO.Password!))
            {
                var roles = await _userManager.GetRolesAsync(user); // Obtem as roles do usu�rio

                var authClaims = new List<Claim> // Cria uma lista de claims.                
                {
                    new Claim(ClaimTypes.Name, user.UserName!), // Adiciona o nome do usu�rio.
                    new Claim(ClaimTypes.Email, user.Email!), // Adiciona o email do usu�rio.
                    new Claim("id", user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Adiciona um identificador �nico.
                };

                foreach (var role in roles) // Adiciona as roles do usu�rio.
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }

                var token = _tokenService.GenerateAccessToken(authClaims, _configuration);

                var refreshToken = _tokenService.GenerateRefreshToken(); // Gera um token de refresh.

                _ = int.TryParse(_configuration["Jwt:RefreshTokenValidityInMinutes"], out int refreshTokenValidityInMinutes); // Obtem o tempo de expira��o do token.

                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(refreshTokenValidityInMinutes); // Adiciona o tempo de expira��o do token de refresh.
                user.RefreshToken = refreshToken; // Adiciona o token de refresh.

                await _userManager.UpdateAsync(user); // Atualiza o usu�rio.

                return Ok(new
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo
                });
            }

            return Unauthorized();
        }
    
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModelDTO modelDTO)
        {
            var userExists = await _userManager.FindByNameAsync(modelDTO.UserName!);

            if (userExists is not null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "User already exists!" });
            }

            ApplicationUser user = new ApplicationUser()
            {
                Email = modelDTO.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = modelDTO.UserName
            };

            var result = await _userManager.CreateAsync(user, modelDTO.Password!);

            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            }

            return Ok(new { Status = "Success", Message = "User created successfully!" });
        }
    
        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenModelDTO tokenModelDTO)
        {
            if (tokenModelDTO is null)
            {
                return BadRequest(new { message = "Invalid client request" });
            }

            string? accessToken = tokenModelDTO.AccessToken ?? throw new ArgumentNullException(nameof(tokenModelDTO)); // Obtem o token de acesso.

            string? refreshToken = tokenModelDTO.RefreshToken ?? throw new ArgumentNullException(nameof(tokenModelDTO)); // Obtem o token de refresh.

            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken!, _configuration); // Obtem os dados do token de acesso.


            if (principal is null)
            {
                return BadRequest(new { message = "Invalid client request" });
            }

            string username = principal.Identity?.Name; // Obtem o nome do usu�rio.

            var user = await _userManager.FindByNameAsync(username!); // Obtem o usu�rio.


            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow) // Verifica se o usu�rio � nulo, se o token de refresh � inv�lido ou se o tempo de expira��o do token de refresh expirou.
            {
                return BadRequest(new { message = "Invalid client request" });
            }

            var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims, _configuration); // Gera um novo token de acesso.

            var newRefreshToken = _tokenService.GenerateRefreshToken(); // Gera um novo token de refresh.

            user.RefreshToken = newRefreshToken; // Adiciona o novo token de refresh.
            await _userManager.UpdateAsync(user); // Atualiza o usu�rio.

            return new ObjectResult(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken
            });
        }
    
        [Authorize(Policy = "ExclusivePolicyOnly")]
        [HttpPost]
        [Route("revoke/{username}")]
        public async Task<IActionResult> Revoke(string username)
        {
            // Verifica o role do usu�rio.
            // if (!User.IsInRole("Admin") && User.Identity?.Name != username)
            // {
            //     return Forbid();
            // }

            var user = await _userManager.FindByNameAsync(username); // Obtem o usu�rio.

            if (user is null)
            {
                return BadRequest(new { message = "Invalid client request" });
            }

            user.RefreshToken = null; // Remove o token de refresh.
            await _userManager.UpdateAsync(user); // Atualiza o usu�rio.

            return NoContent();
        }
    }
}
