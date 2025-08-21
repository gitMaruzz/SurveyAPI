using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace apiSurvey.Logic
{
    public class ConfigHelper
    {
        // =====================================================
        // MÉTODOS BASE PARA ACCEDER A LA CONFIGURACIÓN
        // =====================================================

        /// <summary>
        /// Obtiene un valor de appSettings
        /// </summary>
        public static string GetAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// Obtiene un valor de appSettings con valor por defecto
        /// </summary>
        public static string GetAppSetting(string key, string defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        /// <summary>
        /// Obtiene un valor entero de appSettings
        /// </summary>
        public static int GetAppSettingInt(string key, int defaultValue = 0)
        {
            var value = GetAppSetting(key);
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// Obtiene un valor booleano de appSettings
        /// </summary>
        public static bool GetAppSettingBool(string key, bool defaultValue = false)
        {
            var value = GetAppSetting(key);
            return bool.TryParse(value, out bool result) ? result : defaultValue;
        }

        /// <summary>
        /// Obtiene una cadena de conexión por nombre
        /// </summary>
        public static string GetConnectionString(string name)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name];
            return connectionString?.ConnectionString;
        }

        // =====================================================
        // PROPIEDADES ESPECÍFICAS PARA JWT
        // =====================================================

        public static string JwtSecretKey => GetAppSetting("JwtSecretKey");
        public static string JwtIssuer => GetAppSetting("JwtIssuer", "services");
        public static string JwtAudience => GetAppSetting("JwtAudience", "state");
        public static int JwtExpirationHours => GetAppSettingInt("JwtExpirationHours", 24);

        // =====================================================
        // PROPIEDADES PARA CORS
        // =====================================================

        public static string CorsOrigins => GetAppSetting("CorsOrigins", "http://localhost:5173");

        // =====================================================
        // PROPIEDADES PARA LA APLICACIÓN
        // =====================================================

        public static int DefaultSucursalId => GetAppSettingInt("DefaultSucursalId", 1);
        public static bool EnableDebugMode => GetAppSettingBool("EnableDebugMode", false);
        public static string ApiBaseUrl => GetAppSetting("ApiBaseUrl");
        public static string FrontendUrl => GetAppSetting("FrontendUrl");

        // =====================================================
        // CONNECTION STRINGS
        // =====================================================

        public static string EncuestasConnectionString => GetConnectionString("EncuestasContext");

        // =====================================================
        // MÉTODO PARA VALIDAR CONFIGURACIONES CRÍTICAS
        // =====================================================

        public static void ValidateConfiguration()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(JwtSecretKey))
                errors.Add("JwtSecretKey no está configurado");

            if (JwtSecretKey?.Length < 32)
                errors.Add("JwtSecretKey debe tener al menos 32 caracteres");

            if (string.IsNullOrEmpty(EncuestasConnectionString))
                errors.Add("Connection string 'EncuestasContext' no está configurado");

            if (errors.Any())
            {
                throw new InvalidOperationException(
                    "Errores de configuración:\n" + string.Join("\n", errors));
            }
        }
    }
}