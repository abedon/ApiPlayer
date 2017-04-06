using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AutoTest.Server.Startup))]
namespace AutoTest.Server
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
