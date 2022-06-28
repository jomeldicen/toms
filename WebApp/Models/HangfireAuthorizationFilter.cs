using Hangfire.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hangfire.Annotations;

namespace WebApp.Models
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            return HttpContext.Current.User.Identity.IsAuthenticated;
        }
    }
}