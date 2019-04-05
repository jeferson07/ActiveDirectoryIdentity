using ADIdentity.AD;
using ADIdentity.Model;
using ADIdentity.Model.Enums;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ADIdentity.IdentityManager
{
    public class ADSignInManager<TEntity> : SignInManager<TEntity, string> where TEntity : ADIdentityUser, new()
    {
        protected readonly ActiveDirectoryHelper _activeDirectoryHelper;

        public ADSignInManager(ADUserManager<TEntity> userManager, IAuthenticationManager authenticationManager, ActiveDirectoryHelper activeDirectoryHelper)
            : base(userManager, authenticationManager)
        {
            _activeDirectoryHelper = activeDirectoryHelper;
        }

        //public override Task<ClaimsIdentity> CreateUserIdentityAsync(TEntity user)
        //{
        //    return user.GenerateUserIdentityAsync((ADUserManager<TEntity>)UserManager);
        //}

        //public static ADSignInManager<TEntity> Create(IdentityFactoryOptions<ADSignInManager<TEntity>> options, IOwinContext context, ActiveDirectoryHelper activeDirectoryHelper)
        //{
        //    return new ADSignInManager<TEntity>(context.GetUserManager<ADUserManager<TEntity>>(), context.Authentication, activeDirectoryHelper);
        //}

         //
        public override async Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            throw new NotImplementedException();
        }


        public virtual async Task<CustomSignInStatus> CustomPasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout, bool createIfNotExist = false)
        {
            if (this.UserManager == null)
                return CustomSignInStatus.Failure;

            var userAwaiter = this.UserManager.FindByNameAsync(userName);

            var tUser = await userAwaiter;
            if (tUser == null && !createIfNotExist)
                return CustomSignInStatus.Failure;
            else if (createIfNotExist)
            {
                var returnCreateUser = await ((ADUserManager<TEntity>)UserManager).CreateIfAllowed(userName, password);

                if (returnCreateUser != CustomSignInStatus.Success)
                    return returnCreateUser;

                tUser = await this.UserManager.FindByNameAsync(userName);
            }

            var cultureAwaiter1 = this.UserManager.IsLockedOutAsync(tUser.Id);
            if (await cultureAwaiter1)
                return CustomSignInStatus.LockedOut;

            var cultureAwaiter2 = this.UserManager.CheckPasswordAsync(tUser, password);
            if (!await cultureAwaiter2)
            {
                if (shouldLockout)
                {
                    var cultureAwaiter6 = this.UserManager.AccessFailedAsync(tUser.Id);
                    await cultureAwaiter6;
                    var cultureAwaiter4 = this.UserManager.IsLockedOutAsync(tUser.Id);
                    if (await cultureAwaiter4)
                        return CustomSignInStatus.LockedOut;

                }
                return CustomSignInStatus.Failure;
            }

            var userAD = _activeDirectoryHelper.GetADUserByLogin(userName);

            var groupsAllowed = _activeDirectoryHelper.ValidateGroups(userAD.Groups.Select(s => s.Name), ((ADUserManager<TEntity>)UserManager)._validGroupsAD);

            if (!groupsAllowed.Any())
                return CustomSignInStatus.WithoutPermission;

            var cultureAwaiter5 = this.UserManager.ResetAccessFailedCountAsync(tUser.Id);
            await cultureAwaiter5;
            var cultureAwaiter3 = this.SignInAsync(tUser, isPersistent, false);
            await cultureAwaiter3;

            //TODO: Verificar melhor forma de salvar os grupos do AD
            HttpContext.Current.Session["UserADGroup"] = groupsAllowed;
            return CustomSignInStatus.Success;
        }
    }
}
