using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Domain.Entities;
using ElectronicInvoicing.Infrastructure.ExternalsModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ElectronicInvoicing.Infrastructure.Services;

public class DgiiService(
    IHttpClientFactory httpClientFactory, 
    ISignatureService signatureService,
    IMemoryCache memoryCache, 
    ILogger<DgiiService> logger) 
    : IDgiiService
{
    
    public async Task<string?> GetAuthTokenAsync(DigitalCertificateModel cert)
    {
        string cacheKey = $"DGII_Token_{cert.Rnc}";
        
        if (memoryCache.TryGetValue(cacheKey, out string? cachedToken)) return cachedToken;

        var client = httpClientFactory.CreateClient("DgiiClient");
        
        var seedXml = await client.GetStringAsync("api/Autenticacion/Semilla");
        
        
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(seedXml);
        
        string seedValue = xmlDoc.GetElementsByTagName("valor")[0]?.InnerText ??
                           throw new InvalidOperationException("It could not be retrieved the seed value");
        
        string rncEmisor = cert.Rnc;

        var xmlBuilder = new StringBuilder();
       xmlBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
       xmlBuilder.Append("<SemillaModel xmlns=\"http://dgii.gov.do/core/cf\">");
       xmlBuilder.Append($"<RncEmisor>{rncEmisor}</RncEmisor>");
       xmlBuilder.Append($"<Semilla>{seedValue}</Semilla>");
       xmlBuilder.Append("</SemillaModel>");

        string xmlToSign = xmlBuilder.ToString(); 
            
        logger.LogDebug("XML to sign: {XmlToSign}", xmlToSign);
        
        var (signedSeed, securityCode) = await signatureService.SignXmlAsync(xmlToSign, cert);


        using var formContent = new MultipartFormDataContent();
        var content = new StringContent(signedSeed, Encoding.UTF8, "text/xml");
        formContent.Add(content, "xml","semilla.xml");
        
        var response = await client.PostAsync("api/autenticacion/validarsemilla", content);
        
        
        if (!response.IsSuccessStatusCode)
        {
            var jsonError = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"The DGII says: {jsonError}");
        }
        
        var result = await response.Content.ReadFromJsonAsync<DgiiTokenResponse>();

        if (result?.Token is null)
        {
            logger.LogDebug("DGII returned a null token: {response}", result);
        }
        
        memoryCache.Set(cacheKey, result?.Token, TimeSpan.FromMinutes(55));

        return result?.Token;
    }

    public async Task<DgiiResponse> SendInvoiceAsync(string signedXml, string token)
    {
        var client = httpClientFactory.CreateClient("DgiiClient");
        
        var request = new HttpRequestMessage(HttpMethod.Post, "recepcion/api/recepcion/ecf");
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var formContent = new MultipartFormDataContent();
        var content = new StringContent(signedXml, Encoding.UTF8, "text/xml");
        formContent.Add(content, "xml", "factura.xml");
        
        request.Content = formContent;
        
        var response = await client.SendAsync(request);
        var xmlResponse = await response.Content.ReadAsStringAsync();
        logger.LogError(xmlResponse);
        
        if (response.IsSuccessStatusCode || (int)response.StatusCode == 400) 
        {
            if (string.IsNullOrWhiteSpace(xmlResponse) || !xmlResponse.Trim().StartsWith("<"))
                return new DgiiResponse { Message = "DGII returned an empty or invalid response." };
            
            return DeserializeXml<DgiiResponse>(xmlResponse);
        }

        return new DgiiResponse { Message = $"Connection Error: {response.StatusCode}" };
    }

    public async Task<DgiiResponse> GetStatusAsync(string trackId, string token)
    {
        var client = httpClientFactory.CreateClient("DgiiClient");
    
        var request = new HttpRequestMessage(HttpMethod.Get, $"recepcion/api/consultas/trackid?id={trackId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);
        var xmlResponse = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return DeserializeXml<DgiiResponse>(xmlResponse);
        }

        return new DgiiResponse { Message = $"Error consulting status: {response.StatusCode}" };
    }

    private T DeserializeXml<T>(string xml)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var reader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { IgnoreWhitespace = true });
        return (T)serializer.Deserialize(xmlReader);
    }
}