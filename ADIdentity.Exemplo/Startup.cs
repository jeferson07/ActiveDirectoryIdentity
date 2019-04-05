using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ADIdentity.Exemplo.Startup))]
namespace ADIdentity.Exemplo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
