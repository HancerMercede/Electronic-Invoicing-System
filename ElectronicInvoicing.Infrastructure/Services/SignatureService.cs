using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Domain.Entities;


namespace ElectronicInvoicing.Infrastructure.Services;

public class SignatureService:ISignatureService
{
    public async Task<(string SignedXml, string SecurityCode)> SignXmlAsync(string xmlContent, DigitalCertificateModel cert)
    {
        return await Task.Run(() =>
        {
            using var certificate = new X509Certificate2(cert.Content, cert.Password, 
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
            
            
            var xmlDoc = new XmlDocument { PreserveWhitespace = false };
            xmlDoc.LoadXml(xmlContent);

            var signedXml = new SignedXml(xmlDoc)
            {
                SigningKey =  certificate.GetRSAPrivateKey(),
            };

            signedXml.SignedInfo?.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
            
           
            var reference = new Reference
            {
              Uri = "",
              DigestMethod = SignedXml.XmlDsigSHA256Url
            };
            
             reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
         
             signedXml.AddReference(reference);

            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificate));
            signedXml.KeyInfo = keyInfo;
            
            signedXml.ComputeSignature();


            var xmlDigitalSignature = signedXml.GetXml();


            xmlDoc.DocumentElement?.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));
            
            
            var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            var digestNode = xmlDoc.SelectSingleNode("//ds:DigestValue", nsmgr);
            var securityCode = digestNode?.InnerText ?? string.Empty;
            
            return (xmlDoc.OuterXml, securityCode);
        });
    }
}