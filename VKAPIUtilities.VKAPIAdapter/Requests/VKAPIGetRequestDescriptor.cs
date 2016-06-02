using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKAPIUtilities.VKAPIAdapter.Authorization;

namespace VKAPIUtilities.VKAPIAdapter.Requests
{
public class VKAPIGetRequestDescriptor
{
    // > Название вызываемого метода API
    public String MethodName { get; set; }
        
    // > Словарь параметров запроса
    public Dictionary<String,String> Parameters { get; set; }

    // > Формирование ссылки для запроса, не требующего авторизации
    public Uri GetUnauthorizedRequestUri()
    {
        var query = String.Join("&", Parameters.Select(item => String.Format("{0}={1}", item.Key, item.Value)));
        return new Uri(String.Format("https://api.vk.com/method/{0}?{1}", MethodName, query));
    }

    // > Формирование ссылки для запроса, который требует авторизации
    public Uri GetAuthorizedRequestUri(VKAPIAuthorizationResult authorizationResult)
    {
        var query = String.Join("&", Parameters.Select(item => String.Format("{0}={1}", item.Key, item.Value)));
        return new Uri(
            String.Format("https://api.vk.com/method/{0}?{1}&access_token={2}",
            MethodName,
            query,
            authorizationResult.AccessToken));
    }
    }
}
