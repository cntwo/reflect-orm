using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace ReflectORM.Extensions
{
    /// <summary>
    /// This class exists as a way of quickly verifying whether a web location exists
    /// </summary>
    class UrlValidatorWebClient : WebClient
    {
        public bool HeadOnly { get; set; }
        
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest req = base.GetWebRequest(address);
            if (HeadOnly && req.Method == "GET")
            {
                req.Method = "HEAD";
            }
            return req;
        }
    }
}
