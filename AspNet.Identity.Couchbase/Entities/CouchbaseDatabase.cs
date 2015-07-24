using AspNet.Identity.Couchbase.Config;
using Couchbase;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.N1QL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace AspNet.Identity.Couchbase.Entities
{
    public class CouchbaseDatabase : IDisposable
    {
        private static List<string> _hostsURL;
        private static string _bucketName;
        private static Cluster _cluster;
        private static IBucket _bucket;

        public string BucketName { get { return _bucketName; } }


        public CouchbaseDatabase(List<string> hosts, string bucketName)
        {
            _hostsURL = hosts;
            _bucketName = bucketName;
            Initialize();
        }

        public static CouchbaseDatabase Create()
        {
            CouchbaseSettings conf = CouchbaseSettings.Instance;
            _bucketName = conf.BucketName;
            _hostsURL = (from Host s in conf.Hosts select s.HostAddress).ToList();
            Initialize();
            return new CouchbaseDatabase(_hostsURL, _bucketName);
        }

        private static void Initialize()
        {
            ClientConfiguration config = new ClientConfiguration()
            {
                Servers = _hostsURL.Select(i => new Uri(i)).ToList(),
            };

            _cluster = new Cluster(config);
            _bucket = _cluster.OpenBucket(_bucketName);
            CreateIndex(_bucket);

            IDocumentResult rolesdocument = _bucket.GetDocument<dynamic>("Roles");
            if (rolesdocument.Status == global::Couchbase.IO.ResponseStatus.KeyNotFound)
            {
                CreateRolesDocument<IdentityRole>();
            }
        }


        #region Claims
        public void InsertClaims<TUser>(TUser user, Claim claims) where TUser : global::Microsoft.AspNet.Identity.EntityFramework.IdentityUser
        {
            IdentityUserClaim _claim = new IdentityUserClaim();
            _claim.ClaimType = claims.Type;
            _claim.ClaimValue = claims.Value;
            _claim.UserId = user.Id;
            _claim.Id = user.Claims.Count + 1;
            user.Claims.Add(_claim);
            UpdateUser(user);
        }
        #endregion

        #region Logins
        public void InserUserLogin<TUser>(TUser user, UserLoginInfo login) where TUser : global::Microsoft.AspNet.Identity.EntityFramework.IdentityUser
        {

            IdentityUserLogin _login = new IdentityUserLogin()
            {
                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey,
                UserId = user.Id
            };
            user.Logins.Add(_login);
            UpdateUser(user);
        }

        #endregion

        #region RolesTable

        private static void CreateRolesDocument<TRole>() where TRole : global::Microsoft.AspNet.Identity.EntityFramework.IdentityRole
        {
            var doc = new Document<IdentityRoleListWrapper<TRole>>
            {
                Id = "Roles",
                Content = new IdentityRoleListWrapper<TRole>
                {
                    Roles = new List<TRole>()
                }
            };
            var result = _bucket.Insert<IdentityRoleListWrapper<TRole>>(doc);
        }
        public void UpdateRoles<TRole>(List<TRole> roles) where TRole : global::Microsoft.AspNet.Identity.EntityFramework.IdentityRole
        {
            IdentityRoleListWrapper<TRole> _roles = new IdentityRoleListWrapper<TRole>
            {
                Roles = roles
            };
            var doc = new Document<IdentityRoleListWrapper<TRole>>
            {
                Id = "Roles",
                Content = _roles
            };
            var status = _bucket.Replace<IdentityRoleListWrapper<TRole>>(doc);
        }

        public List<TRole> GetAllRoles<TRole>() where TRole : global::Microsoft.AspNet.Identity.EntityFramework.IdentityRole
        {

            IdentityRoleListWrapper<TRole> _roles = _bucket.Get<IdentityRoleListWrapper<TRole>>("Roles").Value;
            return _roles.Roles;
        }
        #endregion

        #region UserStore
        public TUser GetUserById<TUser>(string id) where TUser : global::Microsoft.AspNet.Identity.EntityFramework.IdentityUser
        {
            return _bucket.GetDocument<TUser>("User_" + id).Content as TUser;
        }
        public void InsertUser<TUser>(TUser user) where TUser : global::Microsoft.AspNet.Identity.EntityFramework.IdentityUser
        {
            var doc = new Document<TUser>
            {
                Id = "User_" + user.Id,
                Content = user
            };
            var result = _bucket.Insert<TUser>(doc);
        }
        public void SetPasswordHash<TUser>(TUser user, string passwordhash) where TUser : global::Microsoft.AspNet.Identity.EntityFramework.IdentityUser
        {
            user.PasswordHash = passwordhash;
            UpdateUser(user);
        }
        public void DeleteUser<TUser>(TUser user) where TUser : global::Microsoft.AspNet.Identity.EntityFramework.IdentityUser
        {
            var doc = new Document<TUser>
            {
                Id = "User_" + user.Id,
                Content = user
            };
            _bucket.Remove<TUser>(doc);
        }
        public void UpdateUser<TUser>(TUser user) where TUser : global::Microsoft.AspNet.Identity.EntityFramework.IdentityUser
        {
            var doc = new Document<TUser>
            {
                Id = "User_" + user.Id,
                Content = user
            };
            var status = _bucket.Replace<TUser>(doc);
        }
        #endregion


        public T Select<T>(IQueryRequest query, string searchParametr = null)
        {
            if (String.IsNullOrEmpty(searchParametr))
            {
                searchParametr = _bucketName;
            }
            
            var result = _bucket.Query<dynamic>(query);
            if (result.Rows.Count > 0)
            {
                var dynamicObject = result.Rows[0][searchParametr];
                dynamic json = JsonConvert.SerializeObject(dynamicObject);
                dynamic TObject = JsonConvert.DeserializeObject<T>(json);
                return TObject;
            }
            else
            {
                return default(T);
            }
        }

        public List<T> MultiSelect<T>(IQueryRequest query, string searchParametr = null)
        {
            if (String.IsNullOrEmpty(searchParametr))
            {
                searchParametr = _bucketName;
            }

            var response = _bucket.Query<dynamic>(query);
            List<T> result = new List<T>();
            if (response.Rows.Count > 0)
            {
                foreach (var row in response.Rows)
                {
                    var dynamicObject = row[searchParametr];
                    dynamic json = JsonConvert.SerializeObject(dynamicObject);
                    dynamic TObject = JsonConvert.DeserializeObject<T>(json);
                    result.Add(TObject);
                 
                }
                return result;
            }
            else
            {
                return default(List<T>);
            }
        }

        
        private static void CreateIndex(IBucket bucket)
        {
            IQueryResult<dynamic> result;
            var indexQuery = new QueryRequest().Statement("SELECT name FROM system:keyspaces");
            if (bucket.Query<dynamic>(indexQuery).Rows.Any(index => index.name == _bucketName))
            {

                /// <summary>
                /// If you need delete Primary index - uncomment line belows
                /// </summary>


                //var deletePrimaryIndexQuery = new QueryRequest().Statement(String.Format("DROP PRIMARY INDEX ON `{0}`", _bucketName));
                //result = bucket.Query<dynamic>(deletePrimaryIndexQuery);

                //var deleteIndexQuery = new QueryRequest().Statement(String.Format("DROP INDEX ON `{0}`", _bucketName));
                //result = bucket.Query<dynamic>(deleteIndexQuery);
                //Console.WriteLine("PRIMARY Index on {0} was deleted: {1}", _bucketName, result.Success);
            }

            var createPrimaryIndexQuery = new QueryRequest().Statement("CREATE PRIMARY INDEX ON " + _bucketName);
            result = bucket.Query<dynamic>(createPrimaryIndexQuery);

            var createIndexQuery = new QueryRequest().Statement("CREATE INDEX ON " + _bucketName);
            result = bucket.Query<dynamic>(createIndexQuery);
            Console.WriteLine("PRIMARY Index on {0} was created: {1}", _bucketName, result.Success);

        }
        public void Dispose()
        {
            _cluster.CloseBucket(_bucket);
            _bucket.Dispose();
            _bucket = null;

            _cluster.Dispose();
            _cluster = null;
        }
    }
}


