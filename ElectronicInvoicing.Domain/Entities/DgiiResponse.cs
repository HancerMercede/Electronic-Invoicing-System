namespace ElectronicInvoicing.Domain.Entities;

public class DgiiResponse
{
    public string? TrackId { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public bool Success => !string.IsNullOrEmpty(TrackId);
    public List<string> Errors { get; set; } = new();
}