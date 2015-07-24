using Couchbase.N1QL;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNet.Identity.Couchbase.Entities
{
    public class RoleTable
    {
        private CouchbaseDatabase _database { get; set; }


        /// <summary>
        /// Constructor that takes a MySQLDatabase instance 
        /// </summary>
        /// <param name="database"></param>
        public RoleTable(CouchbaseDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Deltes a role from the Roles table
        /// </summary>
        /// <param name="roleId">The role Id</param>
        /// <returns></returns>


        /// <summary>
        /// Inserts a new Role in the Roles table
        /// </summary>
        /// <param name="roleName">The role's name</param>
        /// <returns></returns>
        public int Insert<TRole>(TRole role) where TRole : IdentityRole
        {
            List<TRole> roles = GetAllRoles<TRole>();
            roles.Add(role);
            Update<TRole>(roles);
            return 0;
        }

        public string GetRoleIdByName(string name)
        {
            List<IdentityRole> roles = GetAllRoles<IdentityRole>();
            IdentityRole role = roles.FirstOrDefault(i => i.Name == name);
            if (role != null)
            {
                return role.Id;
            }
            return null;
        }

        public int Update<TRole>(List<TRole> roles) where TRole : IdentityRole
        {
            _database.UpdateRoles(roles);
            return 0;
        }

        public List<TRole> GetAllRoles<TRole>() where TRole : IdentityRole
        {
            return _database.GetAllRoles<TRole>();
        }
    }
}
