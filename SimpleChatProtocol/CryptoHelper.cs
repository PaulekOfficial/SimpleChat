using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace SimpleChatProtocol;

public class CryptoHelper
{
    public const string SIGNATURE_ALGORITHM = "SHA256WithRSA";

    private static readonly UnicodeEncoding _encoder = new();

    public static X509Certificate2 GenerateSelfSignedCertificate(string subjectName, string issuerName,
        AsymmetricKeyParameter issuerPrivKey, int keyStrength = 2048)
    {
        //Generate random
        var randomizer = new CryptoApiRandomGenerator();
        var random = new SecureRandom(randomizer);

        //Cert generator
        var generator = new X509V3CertificateGenerator();
        generator.SetSignatureAlgorithm(SIGNATURE_ALGORITHM);

        //Serials
        var serial = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), random);
        generator.SetSerialNumber(serial);

        //Issuer and Subject Name
        var subjectDN = new X509Name(subjectName);
        var issuerDN = new X509Name(issuerName);
        generator.SetSubjectDN(subjectDN);
        generator.SetIssuerDN(issuerDN);

        //Vaild
        var notBefore = DateTime.UtcNow.Date;
        var notAfter = notBefore.AddMonths(1);
        generator.SetNotBefore(notBefore);
        generator.SetNotAfter(notAfter);

        //Subject Public Key
        AsymmetricCipherKeyPair asymmetricCipherKeyPair;
        var keyGeneratorParams = new KeyGenerationParameters(random, keyStrength);
        var keyPairGenerator = new RsaKeyPairGenerator();
        keyPairGenerator.Init(keyGeneratorParams);
        asymmetricCipherKeyPair = keyPairGenerator.GenerateKeyPair();

        generator.SetPublicKey(asymmetricCipherKeyPair.Public);

        //Sign cert
        var certificate = generator.Generate(issuerPrivKey, random);

        //Privatekey
        var keyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(asymmetricCipherKeyPair.Private);

        //Merge into cert v2
        var x509 = new X509Certificate2(certificate.GetEncoded());

        var seq = (Asn1Sequence)keyInfo.ParsePrivateKey();
        if (seq.Count != 9) throw new PemException("Malformed sequence in RSA private key");

        var rsa = new RsaPrivateKeyStructure(seq);
        var rsaParams = new RsaPrivateCrtKeyParameters(rsa.Modulus, rsa.PublicExponent,
            rsa.PrivateExponent, rsa.Prime1, rsa.Prime2, rsa.Exponent1, rsa.Exponent2, rsa.Coefficient);
        var parsedRsa = DotNetUtilities.ToRSA(rsaParams);

        return x509.CopyWithPrivateKey(parsedRsa);
    }

    public static AsymmetricKeyParameter GenerateCACertificate(string subjectName, int keyStrength = 2048)
    {
        //Generate random
        var randomizer = new CryptoApiRandomGenerator();
        var random = new SecureRandom(randomizer);

        //Cert generator
        var generator = new X509V3CertificateGenerator();
        generator.SetSignatureAlgorithm(SIGNATURE_ALGORITHM);

        //Serials
        var serial = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), random);
        generator.SetSerialNumber(serial);

        //Issuer and Subject Name
        var subjectDN = new X509Name(subjectName);
        generator.SetSubjectDN(subjectDN);
        generator.SetIssuerDN(subjectDN);

        //Vaild
        var notBefore = DateTime.UtcNow.Date;
        var notAfter = notBefore.AddMonths(1);
        generator.SetNotBefore(notBefore);
        generator.SetNotAfter(notAfter);

        //Subject Public Key
        AsymmetricCipherKeyPair asymmetricCipherKeyPair;
        var keyGeneratorParams = new KeyGenerationParameters(random, keyStrength);
        var keyPairGenerator = new RsaKeyPairGenerator();
        keyPairGenerator.Init(keyGeneratorParams);
        asymmetricCipherKeyPair = keyPairGenerator.GenerateKeyPair();

        generator.SetPublicKey(asymmetricCipherKeyPair.Public);

        //Sign cert
        var certificate = generator.Generate(asymmetricCipherKeyPair.Private, random);
        var x509 = new X509Certificate2(certificate.GetEncoded());

        //Add to cert to store
        //AddCertificateToStore(x509, StoreName.Root, StoreLocation.CurrentUser);

        return asymmetricCipherKeyPair.Private;
    }

    public static void AddCertificateToStore(X509Certificate2 cert, StoreName storeName,
        StoreLocation storeLocation)
    {
        try
        {
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);

            store.Close();
        }
        catch (Exception exception)
        {
            Console.WriteLine("Error while storing certificate");
        }
    }

    public static void SaveCertificateToFile(X509Certificate2 certificate, string filePath)
    {
        try
        {
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                var certData = certificate.Export(X509ContentType.Cert);
                fs.Write(certData, 0, certData.Length);
            }

            Console.WriteLine("Certyfikat został zapisany do pliku: " + filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Wystąpił błąd podczas zapisywania certyfikatu do pliku: " + ex.Message);
        }
    }

    public static string DecryptString(string data, X509Certificate2 privateCert)
    {
        var rsa = privateCert.GetRSAPrivateKey();
        var dataArray = data.Split(',');
        var dataByte = new byte[dataArray.Length];
        for (var i = 0; i < dataArray.Length; i++) dataByte[i] = Convert.ToByte(dataArray[i]);

        var decryptedByte = rsa.Decrypt(dataByte, RSAEncryptionPadding.OaepSHA256);
        return _encoder.GetString(decryptedByte);
    }

    public static string EncryptString(string content, X509Certificate2 publicCert)
    {
        //var crypto = (RSACryptoServiceProvider)publicCert.PublicKey.Key;
        var rsaPublicCert = publicCert.GetRSAPublicKey();
        // var crypto = new RSACryptoServiceProvider();
        // crypto.ImportCspBlob(publicCert.PublicKey.ExportSubjectPublicKeyInfo());

        var dataToEncrypt = _encoder.GetBytes(content);
        var encryptedByteArray = rsaPublicCert.Encrypt(dataToEncrypt, RSAEncryptionPadding.OaepSHA256).ToArray();
        var length = encryptedByteArray.Count();
        var item = 0;
        var sb = new StringBuilder();
        foreach (var x in encryptedByteArray)
        {
            item++;
            sb.Append(x);

            if (item < length)
                sb.Append(",");
        }

        return sb.ToString();
    }
}