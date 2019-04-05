using ADIdentity.Model;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace ADIdentity.Context
{
    public class ADIdentityContext<TEntity> : IdentityDbContext<TEntity> where TEntity : ADIdentityUser
    {
        public ADIdentityContext(string connectionString)
        : base(connectionString, throwIfV1Schema: false)
        {
        }

        public static ADIdentityContext<TEntity> Create(string connectionString)
        {
            return new ADIdentityContext<TEntity>(connectionString);
        }

        protected override void OnModelCreating(DbModelBuilder dbModelBuilder)
        {
            base.OnModelCreating(dbModelBuilder);
        }
    }
}
