using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace apiSurvey
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
           
            // Configuración y servicios de API web
            // Habilitar CORS
            try
            {
                // Validar configuración al inicio
                Logic.ConfigHelper.ValidateConfiguration();

                // Habilitar CORS con configuración desde Web.config
                var corsOrigins = Logic.ConfigHelper.CorsOrigins;
                // Habilitar CORS con configuración específica para React
                var cors = new EnableCorsAttribute(
                    origins: corsOrigins, // En producción especifica tu dominio: "http://localhost:3000,https://mi-app.com"
                    headers: "*", // Permitir todos los headers
                    methods: "*", // Permitir todos los métodos HTTP
                    exposedHeaders: "Authorization" // Exponer header de autorización
                )
                {
                    SupportsCredentials = true // Permitir cookies si las necesitas
                };
                config.EnableCors(cors);


                // Rutas de API web
                config.MapHttpAttributeRoutes();

                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );

                // Configurar JSON como formato por defecto
                config.Formatters.Remove(config.Formatters.XmlFormatter);
                
                // Configurar serialización JSON para evitar referencias circulares
                config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    PreserveReferencesHandling = PreserveReferencesHandling.None
                };
                System.Diagnostics.Debug.WriteLine("WebAPI configurado exitosamente con configuración desde Web.config");
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configurando WebAPI: {ex.Message}");
                throw;
            }
            
        }
    }
}
