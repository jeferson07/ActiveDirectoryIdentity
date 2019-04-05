using ADIdentity.AD.Model;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace ADIdentity.AD
{
    public class ActiveDirectoryHelper
    {
        private readonly string Domain;
        private readonly string DC;

        public ActiveDirectoryHelper(string domain, string dc)
        {
            Domain = domain;
            DC = dc;
        }

        public bool AuthenticateAD(string username, string password)
        {
            using (var context = new PrincipalContext(ContextType.Domain, Domain))
            {
                return context.ValidateCredentials(username, password);
            }
        }

        /// <summary>
        /// Traz um usuário do AD com algumas informações.
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public ActiveDirectoryUser GetADUser(string search)
        {
            if (string.IsNullOrEmpty(search)) return null;

            using (var context = new PrincipalContext(ContextType.Domain, Domain))
            {
                var result = UserPrincipal.FindByIdentity(context, search);

                if (result == null)
                    return null;

                return new ActiveDirectoryUser
                {
                    Sid = result.Sid,
                    DisplayName = result.DisplayName,
                    Email = result.EmailAddress,
                    Mapped = true,
                    UserName = result.UserPrincipalName,
                    FirstName = result.GivenName,
                    MiddleName = result.MiddleName,
                    Surname = result.Surname,
                    // Initials = result.I
                    VoiceTelephoneNumber = result.VoiceTelephoneNumber
                };
            }
        }

        /// <summary>
        /// Traz um usuário do AD por login.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ActiveDirectoryUser GetADUserByLogin(string name)
        {
            if (name == null) return null;

            if (name.Contains("@")) return GetADUserByEmail(name);

            var filter = "(&(objectClass=user)(objectCategory=person)(samaccountname=" + name.Replace("DOMAIN\\", "") + "))";
            using (var context = new PrincipalContext(ContextType.Domain, Domain))
            {
                var result = InternalSearchSingle(filter);

                if (result == null) return null;

                result.Groups = UserPrincipal.FindByIdentity(context, name)
                                             .GetGroups()
                                             .Select(g => new ActiveDirectoryGroup { Sid = g.Sid.ToString(), Name = g.Name, Description = g.Description })
                                             .ToList();
                return result;
            };
        }

        public ActiveDirectoryUser GetADUserByEmail(string mail)
        {
            if (mail == null) return null;

            if (mail.Contains("DOMAIN\\")) return GetADUserByLogin(mail);

            var filter = "(&(objectClass=user)(objectCategory=person)(mail=" + mail + "))";
            using (var context = new PrincipalContext(ContextType.Domain, Domain))
            {
                var result = InternalSearchSingle(filter);
                /* result.Groups = UserPrincipal.FindByIdentity(context, name)
                                             .GetGroups()
                                             .Select(g => new ActiveDirectoryGroupViewModel { Sid = g.Sid, Name = g.Name, Description = g.Description })
                                             .ToList(); */
                return result;
            };
        }

        private ActiveDirectoryUser InternalSearchSingle(string filter)
        {
            return InternalSearch(filter).FirstOrDefault();
        }

        /// <summary>
        /// Método auxiliar para montar pesquisas do AD.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private IEnumerable<ActiveDirectoryUser> InternalSearch(string filter)
        {
            using (DirectoryEntry entry = new DirectoryEntry($"LDAP://{Domain}"))
            {
                DirectorySearcher search = new DirectorySearcher(entry);
                // search.Filter = "(&(objectClass=user)(l=" + name + "))";
                search.Filter = filter;
                search.PropertiesToLoad.AddRange(new string[] {"samaccountname", "mail", "usergroup", "department", "displayname", "cn", "givenName", "initials",
                    "sn", "homePostalAddress", "title", "company", "st", "l", "co", "postalcode", "telephoneNumber", "otherTelephone", "facsimileTelephoneNumber", "mail",
                    "extensionAttribute1", "extensionAttribute2", "extensionAttribute3", "extensionAttribute4", "extensionAttribute5", "extensionAttribute6",
                    "extensionAttribute7", "extensionAttribute8", "extensionAttribute9", "extensionAttribute10", "extensionAttribute11", "extensionAttribute12",
                    "whenChanged", "whenCreated", "thumbnailPhoto", "objectSid", "objectGUID", "login"}
                );

                foreach (SearchResult sResultSet in search.FindAll())
                {
                    var x = new System.Security.Principal.SecurityIdentifier((byte[])sResultSet.Properties["objectSid"][0], 0);

                    yield return new ActiveDirectoryUser
                    {
                        Sid = new System.Security.Principal.SecurityIdentifier((byte[])sResultSet.Properties["objectSid"][0], 0),
                        // Guid = GetProperty(sResultSet, "objectGUID"),
                        DisplayName = GetProperty(sResultSet, "displayname"),
                        Email = GetProperty(sResultSet, "mail"),
                        Mapped = true,
                        Login = GetProperty(sResultSet, "login"),
                        UserName = GetProperty(sResultSet, "samaccountname"),
                        FirstName = GetProperty(sResultSet, "givenName"),
                        Surname = GetProperty(sResultSet, "sn"),
                        Initials = GetProperty(sResultSet, "initials"),
                        VoiceTelephoneNumber = GetProperty(sResultSet, "telephoneNumber"),
                        JobTitle = GetProperty(sResultSet, "title"),
                        Department = GetProperty(sResultSet, "department"),
                    };
                }
            }
        }

        /// <summary>
        /// Método interno para tratamento de propriedade do AD.
        /// </summary>
        /// <param name="searchResult"></param>
        /// <param name="PropertyName"></param>
        /// <returns></returns>
        private string GetProperty(SearchResult searchResult, string PropertyName)
        {
            if (searchResult.Properties.Contains(PropertyName))
            {
                return searchResult.Properties[PropertyName][0].ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public IEnumerable<string> ValidateGroups(IEnumerable<string> userGroupsAd, IEnumerable<string> validGroups)
        {
            return userGroupsAd.Intersect(validGroups);
        }

    }
}
