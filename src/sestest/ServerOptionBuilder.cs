using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.Batch.SoftwareEntitlement.Common;

namespace Microsoft.Azure.Batch.SoftwareEntitlement
{
    /// <summary>
    /// A factory object that tries to create a <see cref="ServerOptions"/> instance when given the 
    /// <see cref="ServerCommandLine"/> specified by the user.
    /// </summary>
    public class ServerOptionBuilder
    {
        // Reference to the server command line we wrap
        private readonly ServerCommandLine _commandLine;

        // Reference to a store in which we can search for certificates
        private readonly ICertificateStore _certificateStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerOptionBuilder"/> class
        /// </summary>
        /// <param name="commandLine">Options provided on the command line.</param>
        /// <param name="certificateStore">A store to check supplied certificates against.</param>
        public ServerOptionBuilder(
            ServerCommandLine commandLine,
            ICertificateStore certificateStore)
        {
            _commandLine = commandLine;
            _certificateStore = certificateStore;
        }

        /// <summary>
        /// Build an instance of <see cref="ServerOptions"/> from the information supplied on the 
        /// command line by the user
        /// </summary>
        /// <returns>Either a usable (and completely valid) <see cref="ServerOptions"/> or a set 
        /// of errors.</returns>
        public Errorable<ServerOptions> Build()
        {
            var result = Errorable.Success(new ServerOptions())
                .With(ServerUrl()).Map((opt, url) => opt.WithServerUrl(url))
                .With(ConnectionCertificate()).Map((opt, cert) => opt.WithConnectionCertificate(cert))
                .With(SigningCertificate()).Map((opt, cert) => opt.WithSigningCertificate(cert))
                .With(EncryptingCertificate()).Map((opt, cert) => opt.WithEncryptionCertificate(cert))
                .With(Audience()).Map((opt, audience) => opt.WithAudience(audience))
                .With(Issuer()).Map((opt, issuer) => opt.WithIssuer(issuer))
                .With(ExitAfterRequest()).Map((opt, exit) => opt.WithAutomaticExitAfterOneRequest(exit));

            return result;
        }

        /// <summary>
        /// Find the server URL for our hosting
        /// </summary>
        /// <returns>An <see cref="Errorable{Uri}"/> containing either the URL to use or any 
        /// relevant errors.</returns>
        private Errorable<Uri> ServerUrl()
        {
            // If the server URL is not specified, default it.
            var serverUrl = string.IsNullOrWhiteSpace(_commandLine.ServerUrl)
                ? ServerCommandLine.DefaultServerUrl
                : _commandLine.ServerUrl;

            try
            {
                var result = new Uri(serverUrl);
                if (!result.HasScheme("https"))
                {
                    return Errorable.Failure<Uri>("Server endpoint URL must specify https://");
                }

                return Errorable.Success(result);
            }
            catch (Exception e)
            {
                return Errorable.Failure<Uri>($"Invalid server endpoint URL specified ({e.Message})");
            }
        }

        /// <summary>
        /// Find the certificate to use for HTTPS connections
        /// </summary>
        /// <returns>Certificate, if found; error details otherwise.</returns>
        private Errorable<X509Certificate2> ConnectionCertificate()
        {
            if (string.IsNullOrEmpty(_commandLine.ConnectionCertificateThumbprint))
            {
                return Errorable.Failure<X509Certificate2>("A connection thumbprint is required.");
            }

            return FindCertificate("connection", _commandLine.ConnectionCertificateThumbprint);
        }

        /// <summary>
        /// Find the certificate to use for signing tokens
        /// </summary>
        /// <returns>Certificate, if found; error details otherwise.</returns>
        private Errorable<X509Certificate2> SigningCertificate()
        {
            if (string.IsNullOrEmpty(_commandLine.SigningCertificateThumbprint))
            {
                // No certificate requested, no need to look for one
                return Errorable.Success<X509Certificate2>(null);
            }

            return FindCertificate("signing", _commandLine.SigningCertificateThumbprint);
        }

        /// <summary>
        /// Find the certificate to use for encrypting tokens
        /// </summary>
        /// <returns>Certificate, if found; error details otherwise.</returns>
        private Errorable<X509Certificate2> EncryptingCertificate()
        {
            if (string.IsNullOrEmpty(_commandLine.EncryptionCertificateThumbprint))
            {
                // No certificate requested, no need to look for one
                return Errorable.Success<X509Certificate2>(null);
            }

            return FindCertificate("encrypting", _commandLine.EncryptionCertificateThumbprint);
        }

        /// <summary>
        /// Return the audience required 
        /// </summary>
        /// <returns>Audience from the commandline, if provided; default value otherwise.</returns>
        private Errorable<string> Audience()
        {
            if (string.IsNullOrEmpty(_commandLine.Audience))
            {
                return Errorable.Success(Claims.DefaultAudience);
            }

            return Errorable.Success(_commandLine.Audience);
        }

        /// <summary>
        /// Return the issuer required 
        /// </summary>
        /// <returns>Issuer from the commandline, if provided; default value otherwise.</returns>
        private Errorable<string> Issuer()
        {
            if (string.IsNullOrEmpty(_commandLine.Issuer))
            {
                return Errorable.Success(Claims.DefaultIssuer);
            }

            return Errorable.Success(_commandLine.Issuer);
        }

        /// <summary>
        /// Return whether the server should shut down after processing one request
        /// </summary>
        /// <returns></returns>
        private Errorable<bool> ExitAfterRequest()
        {
            return Errorable.Success(_commandLine.ExitAfterRequest);
        }

        /// <summary>
        /// Find a certificate for a specified purpose, given a thumbprint
        /// </summary>
        /// <param name="purpose">A use for which the certificate is needed (for human consumption).</param>
        /// <param name="thumbprint">Thumbprint of the required certificate.</param>
        /// <returns>The certificate, if found; an error message otherwise.</returns>
        private Errorable<X509Certificate2> FindCertificate(string purpose, string thumbprint)
        {
            if (string.IsNullOrWhiteSpace(thumbprint))
            {
                return Errorable.Failure<X509Certificate2>($"No thumbprint supplied; unable to find a {purpose} certificate.");
            }

            var certificateThumbprint = new CertificateThumbprint(thumbprint);
            return _certificateStore.FindByThumbprint(purpose, certificateThumbprint);
        }
    }
}
