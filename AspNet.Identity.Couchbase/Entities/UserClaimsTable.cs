using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AspNet.Identity.Couchbase.Entities
{
    public class UserClaimsTable<TUser>
          where TUser : global::Microsoft.AspNet.Identity.EntityFramework.IdentityUser
    {
        private CouchbaseDatabase _database;

        /// <summary>
        /// Constructor that takes a MySQLDatabase instance 
        /// </summary>
        /// <param name="database"></param>
        public UserClaimsTable(CouchbaseDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Returns a ClaimsIdentity instance given a userId
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <returns></returns>
        public ClaimsIdentity FindByUserId(TUser user)
        {
            ClaimsIdentity claims = new ClaimsIdentity();
            foreach (IdentityUserClaim claim in user.Claims)
            {
                Claim newClaim = new Claim(claim.ClaimType, claim.ClaimValue);
                claims.AddClaim(newClaim);
            }
            return claims;
        }

        /// <summary>
        /// Inserts a new claim in UserClaims table
        /// </summary>
        /// <param name="userClaim">User's claim to be added</param>
        /// <param name="userId">User's id</param>
        /// <returns></returns>
        public int Insert(Claim userClaim, TUser user)
        {
            _database.InsertClaims(user, userClaim);
            return 0;
        }

        /// <summary>
        /// Deletes a claim from a user 
        /// </summary>
        /// <param name="user">The user to have a claim deleted</param>
        /// <param name="claim">A claim to be deleted from user</param>
        /// <returns></returns>
        public int Delete(TUser user, Claim claim)
        {
            var _claim = user.Claims.FirstOrDefault(t => t.ClaimType == claim.Type && t.ClaimValue == claim.Value);
            if (_claim != null)
            {
                user.Claims.Remove(_claim);
                _database.UpdateUser(user);
            }
            return 0;
        }
    }
}
