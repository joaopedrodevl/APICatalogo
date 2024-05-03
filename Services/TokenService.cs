using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace APICatalogo.Services
{
    public class TokenService : ITokenService
    {
        public JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration _config)
        {
            var key = _config.GetSection("JWT").GetValue<string>("SecretKey") // Obtem a chave secreta do appsettings.json
                    ?? throw new InvalidOperationException("Invalid secret key");

            var privateKey  = Encoding.UTF8.GetBytes(key); // Converte a chave secreta para bytes

            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(privateKey), // Cria as credenciais de assinatura
                               SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor // Cria o token
            {
                Subject = new ClaimsIdentity(claims), // Adiciona as claims ao token
                Expires = DateTime.UtcNow.AddMinutes(_config.GetSection("JWT").GetValue<double>("TokenValidityInMinutes")), // Define o tempo de expiração do token
                Audience = _config.GetSection("JWT").GetValue<string>("ValidAudience"), // Define a audiência do token
                Issuer = _config.GetSection("JWT").GetValue<string>("ValidIssuer"), // Define o emissor do token
                SigningCredentials = signingCredentials // Adiciona as credenciais de assinatura ao token
            };

            var tokenHandler = new JwtSecurityTokenHandler(); // Cria o manipulador de token
            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor); // Cria o token

            return token;
        }

        public string GenerateRefreshToken()
        {
            var secureRandomBytes = new byte[128]; // Cria um array de bytes de 128 bytes
            using var randomNumberGenerator = RandomNumberGenerator.Create(); // Cria um gerador de números aleatórios
            randomNumberGenerator.GetBytes(secureRandomBytes); // Preenche o array de bytes com valores aleatórios
            var refreshToken = Convert.ToBase64String(secureRandomBytes); // Converte o array de bytes para uma string Base64
            return refreshToken;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration _config) // Método para obter as claims do token expirado
        {
            var secretKey = _config.GetSection("JWT").GetValue<string>("SecretKey") // Obtem a chave secreta do appsettings.json
                    ?? throw new InvalidOperationException("Invalid secret key");
            
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            
            return principal;
        }
    }
}
