using ADIdentity.Context;

namespace ADIdentity.Exemplo.Models
{
    public class ApplicationDbContext : ADIdentityContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}