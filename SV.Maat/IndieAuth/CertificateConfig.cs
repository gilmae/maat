using System;
using System.Collections.Generic;

namespace SV.Maat.Syndications.Models
{
    public class Certificate
    {
        public string path { get; set; }
        public string password { get; set; }
    }

    public class CertificateStorage
    {
        public Dictionary<string, Certificate> Certificates { get; set; }
        public string CertificatesLocation { get; set; }
    }
}
