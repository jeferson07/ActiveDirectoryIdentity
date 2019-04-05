using Microsoft.AspNet.Identity.EntityFramework;

namespace ADIdentity.Model
{
    public class ADIdentityUser : IdentityUser
    {
        //public virtual async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ADIdentityUser> manager)
        //{
        //    // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
        //    var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
        //    // Add custom user claims here
        //    return userIdentity;
        //}

    }
}
