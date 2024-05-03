using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace APICatalogo.Services
{
    public interface ITokenService
    {
        JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration _config); // Método para gerar o token de acesso

        string GenerateRefreshToken(); // Método para gerar o token de atualização

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration _config); // Método para obter o principal do token expirado
    }
}
