using Couchbase.N1QL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNet.Identity.Couchbase.Entities
{
    public class UserTable<TUser>
         where TUser : global::Microsoft.AspNet.Identity.EntityFramework.IdentityUser
    {
        private CouchbaseDatabase _database;

        /// <summary>
        /// Constructor that takes a MySQLDatabase instance 
        /// </summary>
        /// <param name="database"></param>
        public UserTable(CouchbaseDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Returns an TUser given the user's id
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <returns></returns>
        public TUser GetUserById(string userId)
        {
            return _database.GetUserById<TUser>(userId);
        }

        /// <summary>
        /// Returns a list of TUser instances given a user name
        /// </summary>
        /// <param name="userName">User's name</param>
        /// <returns></returns>
        public TUser GetUserByName(string userName)
        {
            var request = QueryRequest.Create(String.Format("SELECT * FROM {0} WHERE userName=$1", _database.BucketName))
                .AddPositionalParameter(userName)
                .ScanConsistency(ScanConsistency.RequestPlus);
            return _database.Select<TUser>(request);
        }

        public TUser GetUserByEmail(string email)
        {
            var request = QueryRequest.Create(String.Format("SELECT * FROM {0} WHERE email=$1", _database.BucketName))
                .AddPositionalParameter(email)
                .ScanConsistency(ScanConsistency.RequestPlus);
            return _database.Select<TUser>(request);
        }

        /// <summary>
        /// Sets the user's password hash
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="passwordHash"></param>
        /// <returns></returns>
        public int SetPasswordHash(TUser user, string passwordHash)
        {
            _database.SetPasswordHash(user, passwordHash);
            return 0;
        }
        public string SetSecurityStamp(TUser user, string stamp)
        {
            user.SecurityStamp = stamp;
            _database.UpdateUser(user);
            return user.SecurityStamp;
        }

        /// <summary>
        /// Inserts a new user in the Users table
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public int Insert(TUser user)
        {
            _database.InsertUser<TUser>(user);
            return 0;
        }

        ///// <summary>
        ///// Deletes a user from the Users table
        ///// </summary>
        ///// <param name="userId">The user's id</param>
        ///// <returns></returns>
        //private int Delete(T)
        //{
        //    _database.DeleteUser(userId);
        //    return 0;
        //}

        /// <summary>
        /// Deletes a user from the Users table
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public int Delete(TUser user)
        {
            _database.DeleteUser(user);
            return 0;
        }

        /// <summary>
        /// Updates a user in the Users table
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public int Update(TUser user)
        {
            _database.UpdateUser(user);
            return 0;
        }
    }
}
