namespace Polar.Models;

public class CheckoutRequest
{
    public required string ProductPriceId { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
    public string? ExternalCustomerId { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public bool AllowDiscountCodes { get; set; } = true;
    public bool RequireBillingAddress { get; set; } = false;
}

public class CheckoutResponse
{
    public required string Id { get; set; }
    public required string Url { get; set; }
    public required string Status { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class Product
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsArchived { get; set; }
    public required string OrganizationId { get; set; }
    public List<ProductPrice> Prices { get; set; } = new();
}

public class ProductPrice
{
    public required string Id { get; set; }
    public required string ProductId { get; set; }
    public required string Type { get; set; } // one_time or recurring
    public int AmountType { get; set; } // fixed or custom
    public int PriceAmount { get; set; }
    public required string PriceCurrency { get; set; }
    public string? RecurringInterval { get; set; } // month, year
}

public class Customer
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public string? Name { get; set; }
    public string? ExternalCustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class Subscription
{
    public required string Id { get; set; }
    public required string Status { get; set; }
    public required string CustomerId { get; set; }
    public required string ProductId { get; set; }
    public required string PriceId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public DateTime? CancelAtPeriodEnd { get; set; }
}

public class WebhookEvent
{
    public required string Type { get; set; }
    public required object Data { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalCount { get; set; }
}