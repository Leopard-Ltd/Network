#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Cmp
{
    using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Cmp;
    using BestHTTP.SecureProtocol.Org.BouncyCastle.Cms;
    using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;
    using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;
    using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;
    using BestHTTP.SecureProtocol.Org.BouncyCastle.X509;

    public class CertificateStatus
    {
        private static readonly DefaultSignatureAlgorithmIdentifierFinder sigAlgFinder = new();

        private readonly DefaultDigestAlgorithmIdentifierFinder digestAlgFinder;
        private readonly CertStatus                             certStatus;

        public CertificateStatus(DefaultDigestAlgorithmIdentifierFinder digestAlgFinder, CertStatus certStatus)
        {
            this.digestAlgFinder = digestAlgFinder;
            this.certStatus      = certStatus;
        }

        public PkiStatusInfo PkiStatusInfo => this.certStatus.StatusInfo;

        public BigInteger CertRequestId => this.certStatus.CertReqID.Value;

        public bool IsVerified(X509Certificate cert)
        {
            var digAlg = this.digestAlgFinder.find(sigAlgFinder.Find(cert.SigAlgName));
            if (null == digAlg)
                throw new CmpException("cannot find algorithm for digest from signature " + cert.SigAlgName);

            var digest = DigestUtilities.CalculateDigest(digAlg.Algorithm, cert.GetEncoded());

            return Arrays.ConstantTimeAreEqual(this.certStatus.CertHash.GetOctets(), digest);
        }
    }
}
#pragma warning restore
#endif