using ADIdentity.AD;
using ADIdentity.Model;
using ADIdentity.Model.Enums;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Security;

namespace ADIdentity.IdentityManager
{
    public class ADUserManager<TEntity> : UserManager<TEntity> where TEntity : ADIdentityUser, new()
    {
        public readonly ActiveDirectoryHelper _activeDirectoryHelper;
        public readonly IEnumerable<string> _validGroupsAD;

        public ADUserManager(IUserStore<TEntity> store, ActiveDirectoryHelper activeDirectoryHelper, IEnumerable<string> validGroupsAD)
            : base(store)
        {
            _activeDirectoryHelper = activeDirectoryHelper;
            _validGroupsAD = validGroupsAD;
        }

        //TODO? não é aync
        public override async Task<bool> CheckPasswordAsync(TEntity user, string password)
        {
            return _activeDirectoryHelper.AuthenticateAD(user.UserName, password);
        }

        public virtual async Task<CustomSignInStatus> CreateIfAllowed(string user, string password)
        {
            if (!_activeDirectoryHelper.AuthenticateAD(user, password))
                return CustomSignInStatus.Failure;

            var userAD = _activeDirectoryHelper.GetADUserByLogin(user);

            var groupsAllowed = _activeDirectoryHelper.ValidateGroups(userAD.Groups.Select(s => s.Name), _validGroupsAD);

            if (!groupsAllowed.Any())
                return CustomSignInStatus.WithoutPermission;

            var userApp = new TEntity { UserName = user, Email = userAD.Email };
            //TODO: Criar usuário sem senha
            var result = await CreateAsync(userApp, Membership.GeneratePassword(12, 3));

            return CustomSignInStatus.Success;
        }
    }
}
