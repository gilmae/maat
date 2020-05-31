using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using SV.Maat.IndieAuth.Models;
using SV.Maat.Syndications.Models;

namespace SV.Maat.IndieAuth
{
    public class TokenSigning
    {
        X509Certificate2 signingCert;
        CertificateStorage _certificates;
        public TokenSigning(IOptions<CertificateStorage> certificates)
        {
            _certificates = certificates.Value;
            signingCert = new X509Certificate2(
                File.ReadAllBytes(
                    Path.Combine(_certificates.CertificatesLocation, _certificates.Certificates["TokenSigning"].path)
                ),
                _certificates.Certificates["TokenSigning"].password
            );
        }

        public byte[] Encrypt(byte[] token)
        {
            using (RSA rsa = signingCert.GetRSAPublicKey())
            {
                return rsa.Encrypt(token, RSAEncryptionPadding.OaepSHA1);
            }
        }

        public string Encrypt(AccessToken token)
        {
            return Convert.ToBase64String(
                    Encrypt(
                        System.Text.Encoding.ASCII.GetBytes(
                               System.Text.Json.JsonSerializer.Serialize(token)
                        )));
        }

        public byte[] Decrypt(byte[] data)
        {
            using (RSA rsa = signingCert.GetRSAPrivateKey())
            {
                return rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA1);
            }
        }

        public AccessToken Decrypt(string data)
        {
            return System.Text.Json.JsonSerializer.Deserialize<AccessToken>(Decrypt(Convert.FromBase64String(data)));
        }

    }
}
