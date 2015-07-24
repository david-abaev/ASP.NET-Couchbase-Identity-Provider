using Couchbase.N1QL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNet.Identity.Couchbase.Entities
{
    public class UserLoginsTable<TUser>
         where TUser : global::Microsoft.AspNet.Identity.EntityFramework.IdentityUser
    {
        private CouchbaseDatabase _database;

        /// <summary>
        /// Constructor that takes a MySQLDatabase instance 
        /// </summary>
        /// <param name="database"></param>
        public UserLoginsTable(CouchbaseDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Deletes a login from a user in the UserLogins table
        /// </summary>
        /// <param name="user">User to have login deleted</param>
        /// <param name="login">Login to be deleted from user</param>
        /// <returns></returns>
        public int Delete(TUser user, UserLoginInfo login)
        {
            List<IdentityUserLogin> _logins = user.Logins.Where(t => t.ProviderKey == login.ProviderKey && t.LoginProvider == login.LoginProvider).ToList();
            foreach (IdentityUserLogin l in _logins)
            {
                user.Logins.Remove(l);
            }
            _database.UpdateUser(user);
            return 0;

        }

        /// <summary>
        /// Inserts a new login in the UserLogins table
        /// </summary>
        /// <param name="user">User to have new login added</param>
        /// <param name="login">Login to be added</param>
        /// <returns></returns>
        public int Insert(TUser user, UserLoginInfo login)
        {
            _database.InserUserLogin(user, login);
            return 0;
        }

        /// <summary>
        /// Return a userId given a user's login
        /// </summary>
        /// <param name="userLogin">The user's login info</param>
        /// <returns></returns>
        public string FindUserIdByLogin(UserLoginInfo userLogin)
        {
            var request = QueryRequest.Create(String.Format("SELECT id FROM {0} WHERE ANY item IN {1}.logins SATISFIES item.loginProvider=$1 AND item.providerKey=$2 END", _database.BucketName, _database.BucketName))
               .AddPositionalParameter(userLogin.LoginProvider)
               .AddPositionalParameter(userLogin.ProviderKey)
               .ScanConsistency(ScanConsistency.RequestPlus);
            return _database.Select<string>(request, "id");
        }


    }
}
