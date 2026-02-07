using System.Security.Cryptography.X509Certificates;
using System.Xml;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Domain.Entities;
using ElectronicInvoicing.Infrastructure.ExternalsModels;
using FirmaXadesNetCore;
using FirmaXadesNetCore.Crypto;
using FirmaXadesNetCore.Signature.Parameters;


namespace ElectronicInvoicing.Infrastructure.Services;

public class SignatureService:ISignatureService
{
    public async Task<(string SignedXml, string SecurityCode)> SignXmlAsync(string xmlContent, DigitalCertificateModel cert)
    {
        return await Task.Run(async () =>
        {
            using var certificate = new X509Certificate2(cert.Content, cert.Password, 
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

        
            var xadesService = new XadesService();
            var parameters = new SignatureParameters
            {
                SignaturePolicyInfo = null,
                SignaturePackaging = SignaturePackaging.ENVELOPED,
                DataFormat = new DataFormat { MimeType = "text/xml" },
                Signer = new Signer(certificate)
            };

  
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlContent));
            var signatureDocument = xadesService.Sign(stream, parameters);


            var nsmgr = new XmlNamespaceManager(signatureDocument.Document.NameTable);
            nsmgr.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");
            var digestNode = signatureDocument.Document.SelectSingleNode("//dsig:Digest", nsmgr);
            
            var securityCode = digestNode?.InnerText ?? string.Empty;
            
            return (signatureDocument.Document.OuterXml, securityCode);
        });
    }
}