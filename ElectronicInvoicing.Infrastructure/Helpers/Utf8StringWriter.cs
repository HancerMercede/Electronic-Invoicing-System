using System.Text;

namespace ElectronicInvoicing.Infrastructure.Helpers;

public class Utf8StringWriter:StringWriter
{
    public override Encoding Encoding => Encoding.UTF8;
}