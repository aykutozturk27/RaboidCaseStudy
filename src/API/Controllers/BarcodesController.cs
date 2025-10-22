using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using RaboidCaseStudy.Domain.Barcodes;
using RaboidCaseStudy.Infrastructure.Persistence;

namespace RaboidCaseStudy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BarcodesController : ControllerBase
{
	private readonly MongoContext _ctx;
	public BarcodesController(MongoContext ctx) => _ctx = ctx;

	[HttpPost("next")]
	[Authorize(Roles = "Client,Admin")]
	public async Task<ActionResult<string>> Next(CancellationToken ct)
	{
		var col = _ctx.GetCollection<BarcodeRange>();

		// Barcode aralığından atomik şekilde bir numara artır
		var range = await col.FindOneAndUpdateAsync(
			Builders<BarcodeRange>.Filter.Where(r => r.Current < r.End),
			Builders<BarcodeRange>.Update.Inc(r => r.Current, 1),
			new FindOneAndUpdateOptions<BarcodeRange> { ReturnDocument = ReturnDocument.Before },
			ct
		);

		if (range is null)
			return Conflict("No barcodes left");

		// 🔹 Prefix + Current birleşimini dinamik olarak 12 haneye tamamla
		var raw = range.Prefix + range.Current.ToString();
		var base12 = raw.Length switch
		{
			> 12 => raw[^12..],         // Fazlaysa sondaki 12 hane alınır
			< 12 => raw.PadLeft(12, '0'), // Eksikse sola sıfır eklenir
			_ => raw                    // Tam 12 hane ise direkt kullanılır
		};

		var ean13 = ToEan13(base12);
		return Ok(ean13);
	}

	// 🔹 EAN-13 checksum hesaplayıcı
	private static string ToEan13(string base12)
	{
		if (base12.Length != 12 || !base12.All(char.IsDigit))
			throw new ArgumentException("base12 must be 12 digits");

		int sum = 0;
		for (int i = 0; i < 12; i++)
		{
			int d = base12[i] - '0';
			sum += (i % 2 == 0) ? d : d * 3;
		}
		int check = (10 - (sum % 10)) % 10;
		return base12 + check.ToString();
	}
}
