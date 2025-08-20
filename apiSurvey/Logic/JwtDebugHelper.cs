using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace apiSurvey.Logic
{
    public class JwtDebugHelper
    {
        /// <summary>
        /// Muestra todos los claims presentes en el token para debug
        /// </summary>
        public static void DebugClaims(ClaimsPrincipal principal)
        {
            System.Diagnostics.Debug.WriteLine("=== CLAIMS EN EL TOKEN ===");

            if (principal?.Claims == null)
            {
                System.Diagnostics.Debug.WriteLine("No hay claims en el principal");
                return;
            }

            foreach (var claim in principal.Claims)
            {
                System.Diagnostics.Debug.WriteLine($"Type: {claim.Type}");
                System.Diagnostics.Debug.WriteLine($"Value: {claim.Value}");
                System.Diagnostics.Debug.WriteLine($"ValueType: {claim.ValueType}");
                System.Diagnostics.Debug.WriteLine("---");
            }

            System.Diagnostics.Debug.WriteLine("=== FIN CLAIMS ===");
        }

        /// <summary>
        /// Obtiene el ID del usuario de forma segura probando múltiples claims
        /// </summary>
        public static int? GetUserIdSafe(ClaimsPrincipal principal)
        {
            if (principal?.Claims == null) return null;

            // Lista de posibles claims para user ID (en orden de preferencia)
            string[] possibleUserIdClaims = {
                JwtRegisteredClaimNames.Sub,
                ClaimTypes.NameIdentifier,
                "sub",
                "user_id",
                "userId",
                "nameid"
            };

            foreach (var claimType in possibleUserIdClaims)
            {
                var claim = principal.FindFirst(claimType);
                if (claim != null && !string.IsNullOrEmpty(claim.Value))
                {
                    if (int.TryParse(claim.Value, out int userId))
                    {
                        System.Diagnostics.Debug.WriteLine($"User ID encontrado en claim: {claimType} = {userId}");
                        return userId;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("No se encontró User ID en ningún claim conocido");
            return null;
        }

        /// <summary>
        /// Obtiene el rol del usuario de forma segura
        /// </summary>
        public static string GetUserRoleSafe(ClaimsPrincipal principal)
        {
            if (principal?.Claims == null) return null;

            string[] possibleRoleClaims = {
                "role",
                ClaimTypes.Role,
                "roles",
                JwtRegisteredClaimNames.Sub + "_role"
            };

            foreach (var claimType in possibleRoleClaims)
            {
                var claim = principal.FindFirst(claimType);
                if (claim != null && !string.IsNullOrEmpty(claim.Value))
                {
                    System.Diagnostics.Debug.WriteLine($"Rol encontrado en claim: {claimType} = {claim.Value}");
                    return claim.Value;
                }
            }

            return null;
        }
    }
}