namespace ElectronicInvoicing.Domain.Entities;

public class DigitalCertificateModel
{
    public byte[]? Content { get; set; } = Array.Empty<byte>();
    public string? Password { get; set; } = string.Empty;
    public string? Rnc { get; set; } = string.Empty;
}