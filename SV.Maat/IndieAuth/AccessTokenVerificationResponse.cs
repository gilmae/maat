using System;
namespace SV.Maat.IndieAuth
{

        public class AccessTokenVerificationResponse
        {
            public string me { get; set; }

            public string client_id { get; set; }

            public string scope { get; set; }
        }
}
