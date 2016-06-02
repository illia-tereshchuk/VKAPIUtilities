using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKAPIUtilities.VKAPIAdapter.Requests;
using System.Web.Handlers;
using Newtonsoft.Json.Linq;
using System.Windows;

namespace VKAPIUtilities.VKAPIAdapter.Authorization
{
    public class VKAPIAuthorizationPoint
    {
        private VKAPIAuthorizationSettings _authorizationSettings;

        public VKAPIAuthorizationPoint(VKAPIAuthorizationSettings authorizationSettings)
        {
            _authorizationSettings = authorizationSettings;
        }

        public VKAPIAuthorizationResult Authorize()
        {
            return new VKAPIAuthorizationWindow(_authorizationSettings).ShowDialog();
        }
    }
}
