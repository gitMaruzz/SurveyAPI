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
        public TokenGenerator()
        {

        }
        public string GenerateTokenJwt(Models.Model.Usuario usuario)
        {
            //// Clave secreta (debes mantenerla segura)
            //var key = "Wf8VzS0Vuv5Ql7Q4M41Pudzhv1AfMYjZ";
            //var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            //var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            //// Claims que incluirá el token
            //var claims = new[]
            //{
            //        new Claim(JwtRegisteredClaimNames.Sub, username),
            //        new Claim("userId", userId),
            //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            //    };

            //// Crear el token
            //var token = new JwtSecurityToken(
            //    issuer: "services",             // opcional
            //    audience: "state",       // opcional
            //    claims: claims,
            //    expires: DateTime.Now.AddHours(24),
            //    signingCredentials: credentials
            //);

            //return new JwtSecurityTokenHandler().WriteToken(token);
            var tokenHandler = new JwtSecurityTokenHandler();
            var secret = "Wf8VzS0Vuv5Ql7Q4M41Pudzhv1AfMYjZ"; // tu string clave
            var key = Encoding.UTF8.GetBytes(secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Nombre),
                    new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                    new Claim("role", usuario.TipoUsuario),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.Now.AddHours(24),
                Issuer = "services",
                Audience = "state",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public  ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                // Clave secreta (debes mantenerla segura)
                var _secretKey = "Wf8VzS0Vuv5Ql7Q4M41Pudzhv1AfMYjZ";
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = "services",
                    ValidateAudience = true,
                    ValidAudience = "state",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}