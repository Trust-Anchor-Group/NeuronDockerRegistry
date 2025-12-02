using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Security;
using Waher.Security.Users;
using Waher.Service.IoTBroker.Legal.Identity;
using Waher.Service.IoTBroker.Legal.MFA;

namespace TAG.Networking.DockerRegistry.Model
{
    public static class DashboardPrivileges
    {
        public static readonly string Read = "DockerRegistry.Read";
        public static readonly string Create = "DockerRegistry.Create";
        public static readonly string Update = "DockerRegistry.Update";
        public static readonly string Delete = "DockerRegistry.Delete";

        public static readonly string All = "DockerRegistry";
        public static readonly string Admin = "Administrator";

        public static async Task<CaseInsensitiveString> GetUsersOrganizationName(IUser User)
        {
            if (User is QuickLoginUser QuickLoginUser)
            {
                if (QuickLoginUser.Properties.TryGetValue("ORGNAME", out object OrgnNmeObj) && OrgnNmeObj is string OrgName)
                    return OrgName;
            }
            else if (User is User AdminUser)
            {
                LegalIdentity Id = await Database.FindFirstIgnoreRest<LegalIdentity>(new FilterAnd(new FilterFieldEqualTo("Id", AdminUser.LegalId)));
                if (!(Id is null))
                {
                    CaseInsensitiveString OrgName = Id["ORGNAME"];
                    if (!CaseInsensitiveString.IsNullOrEmpty(OrgName))
                        return OrgName;
                }
            }

            return null;
        }
    }

    public interface IDashboardAuthorizable
    {
        public Task<bool> IsAuthorized(IUser User, string Privilege);
    }
}
