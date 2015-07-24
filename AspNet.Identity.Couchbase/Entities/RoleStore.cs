using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNet.Identity.Couchbase.Entities
{
    public class RoleStore<TRole> : IQueryableRoleStore<TRole>
       where TRole : global::Microsoft.AspNet.Identity.EntityFramework.IdentityRole
    {

        private RoleTable roleTable;
        public CouchbaseDatabase Database { get; private set; }
        private List<TRole> _roles { get; set; }

        public IQueryable<TRole> Roles
        {
            get
            {
                return _roles.AsQueryable<TRole>();
            }
        }


        /// <summary>
        /// Default constructor that initializes a new MySQLDatabase
        /// instance using the Default Connection string
        /// </summary>


        /// <summary>
        /// Constructor that takes a MySQLDatabase as argument 
        /// </summary>
        /// <param name="database"></param>
        public RoleStore(CouchbaseDatabase database)
        {
            Database = database;
            roleTable = new RoleTable(database);
            RefreshRoleList();
        }

        public Task CreateAsync(TRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }
            if (!_roles.Any(i => i.Name == role.Name))
            {
                roleTable.Insert(role);
                RefreshRoleList();
            }
            return Task.FromResult<object>(null);
        }

        public Task DeleteAsync(TRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("user");
            }
            TRole _role = _roles.FirstOrDefault(i => i.Id == role.Id);
            if (_role != null)
            {
                _roles.Remove(_role);
            }

            return Task.FromResult<Object>(null);
        }

        public Task<TRole> FindByIdAsync(string roleId)
        {
            TRole result = _roles.FirstOrDefault(i => i.Id == roleId);

            return Task.FromResult<TRole>(result);
        }

        public Task<TRole> FindByNameAsync(string roleName)
        {
            TRole result = _roles.FirstOrDefault(i => i.Name == roleName);
            return Task.FromResult<TRole>(result);
        }

        public Task UpdateAsync(TRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("user");
            }
            TRole _role = _roles.FirstOrDefault(i => i.Id == role.Id);
            if (_role != null)
            {
                _role = role;
                roleTable.Update<TRole>(_roles);
                RefreshRoleList();
            }
            return Task.FromResult<Object>(null);
        }

        public void Dispose()
        {
            if (Database != null)
            {
                Database.Dispose();
                Database = null;
            }
        }

        private void RefreshRoleList()
        {
            _roles = roleTable.GetAllRoles<TRole>();
        }

    }
}
