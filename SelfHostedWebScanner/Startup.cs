using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;

[assembly: OwinStartup(typeof(SelfHostedWebScanner.Startup))]

namespace SelfHostedWebScanner
{

    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
			var hubConfiguration = new HubConfiguration();
			hubConfiguration.EnableDetailedErrors = true;
			app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR(hubConfiguration);
        }
    }
}
