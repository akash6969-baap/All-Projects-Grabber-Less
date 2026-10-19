using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOADER_MAIN
{
    class Api
    {
        public class Data
        {
            public string id { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public object registered { get; set; }
            public object expired { get; set; }
            public object UID { get; set; }
            public object tlstatus { get; set; }
            public string product { get; set; }
            public string reset_limit { get; set; }
            public string type { get; set; }
            public string reseller { get; set; }
            public string credits { get; set; }
            public string token { get; set; }
            public object hwid { get; set; }
            public string appId { get; set; }
        }

        public class Root
        {
            public string status { get; set; }
            public string type { get; set; }
            public string error { get; set; }
            public Data data { get; set; }
        }
    }
}
