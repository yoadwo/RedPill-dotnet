using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace RedPill_dotnet.Utils
{
    public class BasicAuthenticationAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Authorization == null)
            {
                actionContext.Response = actionContext.Request
                    .CreateResponse(System.Net.HttpStatusCode.Unauthorized);
            }
            else
            {
                string authenticationToken = actionContext.Request.Headers.Authorization.Parameter;
                string decodedAuthenticationToken = System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(authenticationToken));
                string[] docPasswordArray = decodedAuthenticationToken.Split(':');
                string docID = docPasswordArray[0];
                string password = docPasswordArray[1];

                if (DoctorSecurity.Login(docID, password))
                {
                    System.Threading.Thread.CurrentPrincipal = new GenericPrincipal(
                        new GenericIdentity(docID), null);
                }
                else
                {
                    actionContext.Response = actionContext.Request
                        .CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                }
            }
        }
    }
}