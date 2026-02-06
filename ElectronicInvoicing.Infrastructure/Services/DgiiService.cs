using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Domain.Entities;
using ElectronicInvoicing.Infrastructure.ExternalsModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ElectronicInvoicing.Infrastructure.Services;

public class DgiiService(IHttpClientFactory httpClientFactory, ISignatureService signatureService, IMemoryCache memoryCache, ILogger<DgiiService> logger) : IDgiiService
{
    
    public async Task<string?> GetAuthTokenAsync(string certificatePath, string password)
    {
        string cacheKey = $"DGII_Token_{certificatePath.GetHashCode()}";
        
        if (memoryCache.TryGetValue(cacheKey, out string? cachedToken)) return cachedToken;

        var client = httpClientFactory.CreateClient("DgiiClient");
        
        var seedXml = await client.GetStringAsync("autenticacion/semilla");
        var (signedSeed, _) = await signatureService.SignXmlAsync(seedXml, certificatePath, password);

        var content = new StringContent(signedSeed, Encoding.UTF8, "application/xml");
        var response = await client.PostAsync("autenticacion/token", content);
        
        var result = DeserializeXml<DgiiTokenResponse>(await response.Content.ReadAsStringAsync());
        
        memoryCache.Set(cacheKey, result.Token, TimeSpan.FromHours(23));

        return result.Token;
    }

    public async Task<DgiiResponse> SendInvoiceAsync(string signedXml, string token)
    {
        var client = httpClientFactory.CreateClient("DgiiClient");
        
        var request = new HttpRequestMessage(HttpMethod.Post, "recepcion/api/recepcion/ecf");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(signedXml, Encoding.UTF8, "application/xml");
        
        var response = await client.SendAsync(request);
        var xmlResponse = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode || (int)response.StatusCode == 400) // 400 puede traer errores de validación
        {
            if (string.IsNullOrWhiteSpace(xmlResponse) || !xmlResponse.Trim().StartsWith("<"))
                return new DgiiResponse { Message = "DGII returned an empty or invalid response." };
            
            return DeserializeXml<DgiiResponse>(xmlResponse);
        }

        return new DgiiResponse { Message = $"Connection Error: {response.StatusCode}" };
    }
    
    private T DeserializeXml<T>(string xml)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var reader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { IgnoreWhitespace = true });
        return (T)serializer.Deserialize(xmlReader);
    }
}