using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using FinanceTracker.API.Models;
using FinanceTracker.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;


[ApiController]
[Route("api/[controller]")]
public class PlaidController : ControllerBase
{
    private readonly PlaidService _plaid;
    private readonly AppDbContext _db;

    public PlaidController(PlaidService plaid, AppDbContext db)
    {
        _plaid = plaid;
        _db = db;
    }

    [HttpPost("exchange-token")]
    public async Task<IActionResult> ExchangeToken([FromBody] PublicTokenRequest req)
    {
        var exchangeResult = await _plaid.ExchangePublicToken(req.PublicToken);
        var userId = req.UserId;

        // Parse access_token & item_id from Plaid's response
        var parsed = JsonDocument.Parse(exchangeResult);
        var accessToken = parsed.RootElement.GetProperty("access_token").GetString();
        var itemId = parsed.RootElement.GetProperty("item_id").GetString();
        // Store it in your database
        var linkedItem = new LinkedItem
        {
            UserId = userId, // convert to int (assuming it's int)
            AccessToken = accessToken!,
            ItemId = itemId!,
            InstitutionName = "Unknown" // You can populate this later via metadata
        };

        await _db.LinkedItems.AddAsync(linkedItem);
        await _db.SaveChangesAsync();

        return Ok(new { success = true });
    }
    [HttpGet("create-link-token")]
    public async Task<IActionResult> CreateLinkToken()
    {
        var result = await _plaid.CreateLinkToken();
        return Ok(result);
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        // Get access token from your database
        var tokenEntry = await _db.LinkedItems.FirstOrDefaultAsync(x => x.UserId == userId);

        if (tokenEntry == null)
            return NotFound("No linked account found.");

        // Set endDate to today (current UTC date)
        var endDate = DateTime.UtcNow;

        // Set startDate to the first day of the current month
        var startDate = new DateTime(endDate.Year, endDate.Month, 1);


        var result = await _plaid.GetTransactions(tokenEntry.AccessToken, startDate, endDate);

        return Ok(result); // You can deserialize this too
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> PlaidWebhook([FromBody] JsonElement payload)
    {
        Console.WriteLine("Plaid Webhook Received:");
        Console.WriteLine(payload.ToString());

        // Optional: handle different webhook types
        var webhookType = payload.GetProperty("webhook_type").GetString();
        var webhookCode = payload.GetProperty("webhook_code").GetString();
      
        Console.WriteLine($"Webhook Type: {webhookType}");
        Console.WriteLine($"Webhook Code: {webhookCode}");

        if (webhookType == "TRANSACTIONS" && webhookCode == "INITIAL_UPDATE")
        {
            var itemId = payload.GetProperty("item_id").GetString();
            var linkedItem = await _db.LinkedItems.FirstOrDefaultAsync(i => i.ItemId == itemId);
            if (linkedItem != null)
            {
                linkedItem.IsReady = true;
                await _db.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine($"Item not found for itemId: {itemId}");
            }
        }


        // Example: you could react to 'TRANSACTIONS_INITIAL_UPDATE'
        // by flagging the item as ready to sync

        return Ok();
    }

    [HttpGet("status/{userId}")]
    public async Task<IActionResult> GetPlaidStatus(int userId)
    {
        var item = await _db.LinkedItems.FirstOrDefaultAsync(i => i.UserId == userId);
        if (item == null) return NotFound();
        return Ok(new { ready = item.IsReady });
    }
    
    [HttpGet("check")]
    public async Task<IActionResult> CheckPlaidStatus()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var hasLinkedItem = await _db.LinkedItems
            .AnyAsync(li => li.UserId == userId && !string.IsNullOrEmpty(li.AccessToken));

        return Ok(new { plaidLinked = hasLinkedItem });
    }





}

public class PublicTokenRequest
{
    public string? PublicToken { get; set; }
    public int UserId { get; set; }  //Add this
}

