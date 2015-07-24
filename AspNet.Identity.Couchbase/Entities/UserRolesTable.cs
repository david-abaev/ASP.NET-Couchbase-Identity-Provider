using Couchbase.N1QL;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNet.Identity.Couchbase.Entities
{
    public class UserRolesTable<TUser>
          where TUser : global::Microsoft.AspNet.Identity.EntityFramework.IdentityUser
    {
        private CouchbaseDatabase _database;

        /// <summary>
        /// Constructor that takes a MySQLDatabase instance 
        /// </summary>
        /// <param name="database"></param>
        public UserRolesTable(CouchbaseDatabase database)
        {
            _database = database;
        }



        /// <summary>
        /// Returns a list of user's roles
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <returns></returns>
        public List<string> FindByUserId(TUser user)
        {
            List<string> rolesName = new List<string>();
            var request = QueryRequest.Create(String.Format("SELECT ARRAY item.roleId FOR item IN {0}.roles END AS roleId FROM {1} where id=$1", _database.BucketName, _database.BucketName))
                                       .AddPositionalParameter(user.Id)
                                       .ScanConsistency(ScanConsistency.RequestPlus);
            string[] rolesId = _database.Select<string[]>(request, "roleId");
            if (rolesId != null)
            {
                List<IdentityRole> roles = _database.GetAllRoles<IdentityRole>();
                rolesName = roles.Where(i => rolesId.Contains(i.Id)).Select(i => i.Name).ToList();
            }
            return rolesName;
        }

        /// <summary>
        /// Deletes all roles from a user in the UserRoles table
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <returns></returns>
        public int Delete(TUser user, string roleName)
        {
            List<IdentityRole> roles = _database.GetAllRoles<IdentityRole>();
            IdentityRole role = roles.FirstOrDefault(i => i.Name == roleName);
            if (role != null)
            {
                string roleId = role.Id;

                IdentityUserRole userRole = user.Roles.FirstOrDefault(i => i.RoleId == roleId);
                if (userRole != null)
                {
                    user.Roles.Remove(userRole);
                    _database.UpdateUser<TUser>(user);
                }
            }
            return 0;
        }

        /// <summary>
        /// Inserts a new role for a user in the UserRoles table
        /// </summary>
        /// <param name="user">The User</param>
        /// <param name="roleId">The Role's id</param>
        /// <returns></returns>
        public int Insert(TUser user, string roleId)
        {

            IdentityUserRole newRole = new IdentityUserRole
            {
                RoleId = roleId,
                UserId = user.Id
            };
            if (!user.Roles.Any(i => i.RoleId == roleId && i.UserId == user.Id))
            {
                user.Roles.Add(newRole);
                _database.UpdateUser(user);
            }
            return 0;
        }
    }
}
