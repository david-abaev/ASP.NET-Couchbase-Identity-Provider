using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace AspNet.Identity.Couchbase.Config
{
    public sealed class CouchbaseSettings : ConfigurationSection
    {

        private static CouchbaseSettings instance = null;
        public static CouchbaseSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (CouchbaseSettings)WebConfigurationManager.GetSection("couchbaseSettings");
                }
                return instance;
            }
        }

        [ConfigurationProperty("bucketName")]
        public string BucketName
        {
            get { return base["bucketName"].ToString(); }
            set { base["bucketName"] = value; }
        }



        [ConfigurationProperty("Hosts")]
        public HostCollection Hosts
        {
            get { return (HostCollection)base["Hosts"]; }
            set { base["Hosts"] = value; }
        }

    }
    public class Host : ConfigurationElement
    {
        [ConfigurationProperty("key", IsRequired = true)]
        public string Key
        {
            get { return (string)this["key"]; }
            set { this["key"] = value; }
        }

        [ConfigurationProperty("hostAddress", IsRequired = true)]
        public string HostAddress
        {
            get { return (string)this["hostAddress"]; }
            set { this["hostAddress"] = value; }
        }
    }

    public class HostCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new Host();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Host)element).Key;
        }
    }
}
