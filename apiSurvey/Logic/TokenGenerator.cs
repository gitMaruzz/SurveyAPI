using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.IdentityModel.Tokens.Jwt;

namespace apiSurvey.Logic
{
    public class TokenGenerator
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationHours;

        public TokenGenerator()
        {
            try
            {
                // Cargar configuración desde Web.config
                _secretKey = ConfigHelper.JwtSecretKey;
                _issuer = ConfigHelper.JwtIssuer;
                _audience = ConfigHelper.JwtAudience;
                _expirationHours = ConfigHelper.JwtExpirationHours;

                // Validar configuración crítica
                ValidateJwtConfiguration();

                System.Diagnostics.Debug.WriteLine("TokenGenerator inicializado con configuración desde Web.config");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al inicializar TokenGenerator: {ex.Message}");
                throw new InvalidOperationException("Error de configuración JWT", ex);
            }
        }

        private void ValidateJwtConfiguration()
        {
            if (string.IsNullOrEmpty(_secretKey))
            {
                throw new InvalidOperationException("JwtSecretKey no está configurado en Web.config");
            }

            if (_secretKey.Length < 32)
            {
                throw new InvalidOperationException("JwtSecretKey debe tener al menos 32 caracteres");
            }

            if (string.IsNullOrEmpty(_issuer))
            {
                throw new InvalidOperationException("JwtIssuer no está configurado en Web.config");
            }

            if (string.IsNullOrEmpty(_audience))
            {
                throw new InvalidOperationException("JwtAudience no está configurado en Web.config");
            }

            if (_expirationHours <= 0)
            {
                throw new InvalidOperationException("JwtExpirationHours debe ser mayor a 0");
            }
        }
        public string GenerateTokenJwt(Models.Model.Usuario usuario)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                    new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Nombre),
                    new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                    new Claim("role", usuario.TipoUsuario),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat,
                            new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                            ClaimValueTypes.Integer64)
                }),
                    Expires = DateTime.Now.AddHours(_expirationHours),
                    Issuer = _issuer,
                    Audience = _audience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                System.Diagnostics.Debug.WriteLine($"Token JWT generado para usuario: {usuario.Email}");
                return tokenString;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generando token JWT: {ex.Message}");
                throw new InvalidOperationException("Error al generar token JWT", ex);
            }
            
        }

        public  ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true
                };

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                System.Diagnostics.Debug.WriteLine("Token JWT validado exitosamente");
                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                System.Diagnostics.Debug.WriteLine("Token JWT expirado");
                return null;
            }
            catch (SecurityTokenException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Token JWT inválido: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error validando token JWT: {ex.Message}");
                return null;
            }
        }

        // Método para obtener información del token sin validarlo completamente
        public JwtSecurityToken DecodeTokenWithoutValidation(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                return tokenHandler.ReadJwtToken(token);
            }
            catch
            {
                return null;
            }
        }
    }
}