using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNet.Identity.Couchbase
{
    internal class IdentityRoleListWrapper<TRole>
        where TRole : IdentityRole
    {
        public List<TRole> Roles { get; set; }

    }
}
