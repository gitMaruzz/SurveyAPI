using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace apiSurvey.Controllers
{
    public class JwtAuthorizationAttribute : AuthorizationFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public JwtAuthorizationAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var authHeader = actionContext.Request.Headers.Authorization;

            if (authHeader == null || authHeader.Scheme != "Bearer")
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    System.Net.HttpStatusCode.Unauthorized,
                    new { message = "Token de autorización requerido" });
                return;
            }

            var token = authHeader.Parameter;

            Logic.TokenGenerator jwtService = new Logic.TokenGenerator();
            var principal = jwtService.ValidateToken(token);

            if (principal == null)
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    System.Net.HttpStatusCode.Unauthorized,
                    new { message = "Token inválido" });
                return;
            }

            // Verificar roles si se especificaron
            if (_allowedRoles != null && _allowedRoles.Length > 0)
            {
                var userRole = principal.FindFirst(ClaimTypes.Role)?.Value;
                if (!_allowedRoles.Contains(userRole))
                {
                    actionContext.Response = actionContext.Request.CreateResponse(
                        System.Net.HttpStatusCode.Forbidden,
                        new { message = "No tiene permisos para esta operación" });
                    return;
                }
            }

            // Guardar el principal en el contexto
            actionContext.RequestContext.Principal = principal;
        }
    }
}