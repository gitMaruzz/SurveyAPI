using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
[assembly: OwinStartup(typeof(NodianConnect.Startup))]
namespace apiSurvey
{
    public class Startup
    {   public void Configuration(IAppBuilder app)
        {

            var key = Encoding.UTF8.GetBytes("Wf8VzS0Vuv5Ql7Q4M41Pudzhv1AfMYjZ");

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = "services",
                    ValidAudience = "state",
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true
                }
            });
        }
    }
}