using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKAPIUtilities.VKAPIAdapter.Authorization
{
    public class VKAPIAuthorizationResult
    {
        // > Идентификатор пользователя, от имени которого произошла авторизация
        public String UserIdentity { get; set; }
        // > Ключ, который возвращается при успешной авторизации
        public String AccessToken { get; set; }
    }
}
