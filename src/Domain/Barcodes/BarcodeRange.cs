using RaboidCaseStudy.Domain.Common;
namespace RaboidCaseStudy.Domain.Barcodes;
public class BarcodeRange : Entity
{
    // EAN-13: We'll manage numeric prefix and a counter [start, end)
    public string Prefix { get; set; } = "8691234"; // Example Turkey prefix
    public long Current { get; set; } // next serial to use
    public long End { get; set; } // exclusive
}
