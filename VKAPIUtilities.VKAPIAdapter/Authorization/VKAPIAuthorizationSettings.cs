using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace VKAPIUtilities.VKAPIAdapter.Authorization
{
    public class VKAPIAuthorizationSettings
    {
        // > Разрешения, передаваемые в параметре "scope"
        public VKAPIAuthorizationPermissions ApplicationPermissions { get; set; }

        // > Идентификатор приложения ВКонтакте
        public String ApplicationIdentity { get; set; }

        // > Флаг на постоянное переспрашивание авторизации
        public Boolean RevocationEnabled { get; set; }

        // > Используемая версия API 
        public String APIVersion { get; set; }

        // > Создание строки запроса на основе параметров
        public Uri GetAuthorizationUri()
        {
            // + Инициализация строки запроса на авторизацию
            var authorizationUriBuilder = new UriBuilder("https://oauth.vk.com/authorize");
            var queryBuilder = HttpUtility.ParseQueryString(String.Empty);
            // ++ Присваивание неизменяющихся параметров
            queryBuilder["display"] = "popup";
            queryBuilder["response_type"] = "token";
            queryBuilder["redirect_uri"] = "https://oauth.vk.com/blank.html";
            // -- Присваивание неизменяющихся параметров
            // ++ Присваивание параметров, переданных в конфигурации
            queryBuilder["v"] = APIVersion;
            queryBuilder["client_id"] = ApplicationIdentity;
            queryBuilder["scope"] = ApplicationPermissions.ToString();
            if (RevocationEnabled) queryBuilder["revoke"] = "1";
            // -- Присваивание параметров, переданных в конфигурации
            authorizationUriBuilder.Query = queryBuilder.ToString();
            // - Инициализация строки запроса на авторизацию
            return authorizationUriBuilder.Uri;
        }
    }
}
