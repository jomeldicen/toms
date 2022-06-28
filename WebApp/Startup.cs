using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using Hangfire;
using WebApp.Models;
using WebApp.Api.Admin;

[assembly: OwinStartup(typeof(WebApp.Startup))]

namespace WebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            ConfigureAuth(app);
                 
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");
            app.UseHangfireDashboard("/AppJobDashboard", new DashboardOptions() {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });

            CronJobController crn = new CronJobController();

            RecurringJob.AddOrUpdate(() => crn.GetProcessEmailQueueing(), "*/15 * * * *");
            RecurringJob.AddOrUpdate(() => crn.GetFetchAllSAPAccountWithTOAS(), "*/15 * * * *");
            RecurringJob.AddOrUpdate(() => crn.GetSendTurnoverDateToSAP(), "*/15 * * * *");

            app.UseHangfireServer();
        }
    }
}
