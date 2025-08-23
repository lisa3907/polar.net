using Microsoft.Extensions.Configuration;

namespace PolarSample.Demos
{
    /// <summary>
    /// Demonstrates Payment and Refund API operations.
    /// </summary>
    public class PaymentDemo : DemoBase
    {
        public PaymentDemo(IConfigurationRoot config) : base(config) { }

        public override async Task RunAsync()
        {
            WriteHeader("Payment & Refund Demo");

            Console.WriteLine("Select an operation:");
            Console.WriteLine("1. List all payments");
            Console.WriteLine("2. Get payment details");
            Console.WriteLine("3. List payments by order");
            Console.WriteLine("4. List all refunds");
            Console.WriteLine("5. Get refund details");
            Console.WriteLine("6. Create refund (requires valid order)");
            Console.Write("\nEnter choice [1-6]: ");
            
            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await ListPaymentsAsync();
                        break;
                    case "2":
                        await GetPaymentDetailsAsync();
                        break;
                    case "3":
                        await ListPaymentsByOrderAsync();
                        break;
                    case "4":
                        await ListRefundsAsync();
                        break;
                    case "5":
                        await GetRefundDetailsAsync();
                        break;
                    case "6":
                        await CreateRefundAsync();
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Payment Demo: {ex.Message}");
            }
        }

        private async Task ListPaymentsAsync()
        {
            WriteSubHeader("Listing Payments");
            
            var payments = await Client.ListPaymentsAsync(1, 20);
            Console.WriteLine($"Total Payments: {payments.Items.Count}");
            
            if (payments.Items.Count == 0)
            {
                Console.WriteLine("No payments found.");
                return;
            }
            
            foreach (var payment in payments.Items.Take(10))
            {
                Console.WriteLine($"\nPayment ID: {payment.Id}");
                Console.WriteLine($"  Amount: {payment.Amount} {payment.Currency}");
                Console.WriteLine($"  Status: {payment.Status}");
                Console.WriteLine($"  Created: {payment.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                
                if (!string.IsNullOrEmpty(payment.OrderId))
                {
                    Console.WriteLine($"  Order ID: {payment.OrderId}");
                }
            }
            
            if (payments.Items.Count > 10)
            {
                Console.WriteLine($"\n... and {payments.Items.Count - 10} more payments");
            }
        }

        private async Task GetPaymentDetailsAsync()
        {
            WriteSubHeader("Get Payment Details");
            
            // First, list some payments to help user choose
            var payments = await Client.ListPaymentsAsync(1, 5);
            if (payments.Items.Count > 0)
            {
                Console.WriteLine("Recent payments:");
                foreach (var p in payments.Items)
                {
                    Console.WriteLine($"  {p.Id}: {p.Amount} {p.Currency} ({p.Status})");
                }
                Console.WriteLine();
            }
            
            Console.Write("Enter payment ID: ");
            var paymentId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(paymentId))
            {
                Console.WriteLine("Payment ID is required.");
                return;
            }
            
            var payment = await Client.GetPaymentAsync(paymentId);
            
            Console.WriteLine($"\nPayment Details:");
            Console.WriteLine($"ID: {payment.Id}");
            Console.WriteLine($"Amount: {payment.Amount} {payment.Currency}");
            Console.WriteLine($"Status: {payment.Status}");
            Console.WriteLine($"Created: {payment.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            
            if (payment.UpdatedAt != null)
            {
                Console.WriteLine($"Updated: {payment.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
            }
            
            if (!string.IsNullOrEmpty(payment.OrderId))
            {
                Console.WriteLine($"Order ID: {payment.OrderId}");
            }
        }

        private async Task ListPaymentsByOrderAsync()
        {
            WriteSubHeader("List Payments by Order");
            
            // First, find an order with payments
            var payments = await Client.ListPaymentsAsync(1, 20);
            var ordersWithPayments = payments.Items
                .Where(p => !string.IsNullOrEmpty(p.OrderId))
                .Select(p => p.OrderId)
                .Distinct()
                .Take(5)
                .ToList();
            
            if (ordersWithPayments.Count > 0)
            {
                Console.WriteLine("Orders with payments:");
                foreach (var orderId in ordersWithPayments)
                {
                    Console.WriteLine($"  {orderId}");
                }
                Console.WriteLine();
            }
            
            Console.Write("Enter order ID: ");
            var orderIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(orderIdInput))
            {
                Console.WriteLine("Order ID is required.");
                return;
            }
            
            var orderPayments = await Client.ListPaymentsByOrderAsync(orderIdInput);
            Console.WriteLine($"\nPayments for Order {orderIdInput}: {orderPayments.Items.Count}");
            
            foreach (var payment in orderPayments.Items)
            {
                Console.WriteLine($"  Payment {payment.Id}: {payment.Amount} {payment.Currency} ({payment.Status})");
            }
        }

        private async Task ListRefundsAsync()
        {
            WriteSubHeader("Listing Refunds");
            
            var refunds = await Client.ListRefundsAsync(1, 20);
            Console.WriteLine($"Total Refunds: {refunds.Items.Count}");
            
            if (refunds.Items.Count == 0)
            {
                Console.WriteLine("No refunds found.");
                return;
            }
            
            foreach (var refund in refunds.Items.Take(10))
            {
                Console.WriteLine($"\nRefund ID: {refund.Id}");
                Console.WriteLine($"  Amount: {refund.Amount} {refund.Currency}");
                Console.WriteLine($"  Reason: {refund.Reason}");
                Console.WriteLine($"  Status: {refund.Status}");
                Console.WriteLine($"  Created: {refund.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                
                if (!string.IsNullOrEmpty(refund.Comment))
                {
                    Console.WriteLine($"  Comment: {refund.Comment}");
                }
                
                if (!string.IsNullOrEmpty(refund.OrderId))
                {
                    Console.WriteLine($"  Order ID: {refund.OrderId}");
                }
            }
            
            if (refunds.Items.Count > 10)
            {
                Console.WriteLine($"\n... and {refunds.Items.Count - 10} more refunds");
            }
        }

        private async Task GetRefundDetailsAsync()
        {
            WriteSubHeader("Get Refund Details");
            
            // First, list some refunds to help user choose
            var refunds = await Client.ListRefundsAsync(1, 5);
            if (refunds.Items.Count > 0)
            {
                Console.WriteLine("Recent refunds:");
                foreach (var r in refunds.Items)
                {
                    Console.WriteLine($"  {r.Id}: {r.Amount} {r.Currency} ({r.Status})");
                }
                Console.WriteLine();
            }
            
            Console.Write("Enter refund ID: ");
            var refundId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(refundId))
            {
                Console.WriteLine("Refund ID is required.");
                return;
            }
            
            var refund = await Client.GetRefundAsync(refundId);
            
            Console.WriteLine($"\nRefund Details:");
            Console.WriteLine($"ID: {refund.Id}");
            Console.WriteLine($"Amount: {refund.Amount} {refund.Currency}");
            Console.WriteLine($"Reason: {refund.Reason}");
            Console.WriteLine($"Status: {refund.Status}");
            Console.WriteLine($"Created: {refund.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            
            if (!string.IsNullOrEmpty(refund.Comment))
            {
                Console.WriteLine($"Comment: {refund.Comment}");
            }
            
            if (!string.IsNullOrEmpty(refund.OrderId))
            {
                Console.WriteLine($"Order ID: {refund.OrderId}");
            }
            
            if (!string.IsNullOrEmpty(refund.PaymentId))
            {
                Console.WriteLine($"Payment ID: {refund.PaymentId}");
            }
        }

        private async Task CreateRefundAsync()
        {
            WriteSubHeader("Create Refund");
            
            Console.WriteLine("WARNING: This will create a real refund in the system!");
            Console.WriteLine("Only proceed if you have a valid paid order to refund.\n");
            
            // List recent orders to help user
            var orders = await Client.ListOrdersAsync(1, 10);
            if (orders.Items.Count > 0)
            {
                Console.WriteLine("Recent orders:");
                foreach (var order in orders.Items.Where(o => o.Amount > 0).Take(5))
                {
                    Console.WriteLine($"  {order.Id}: {order.Amount} {order.Currency} ({order.CreatedAt:yyyy-MM-dd})");
                }
                Console.WriteLine();
            }
            
            Console.Write("Enter order ID to refund: ");
            var orderId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(orderId))
            {
                Console.WriteLine("Order ID is required.");
                return;
            }
            
            Console.WriteLine("\nRefund reason options:");
            Console.WriteLine("1. duplicate");
            Console.WriteLine("2. fraudulent");
            Console.WriteLine("3. requested_by_customer");
            Console.WriteLine("4. other");
            Console.Write("Select reason [1-4]: ");
            
            var reasonChoice = Console.ReadLine();
            var reason = reasonChoice switch
            {
                "1" => "duplicate",
                "2" => "fraudulent",
                "3" => "requested_by_customer",
                "4" => "other",
                _ => "requested_by_customer"
            };
            
            Console.Write("Enter refund comment (optional): ");
            var comment = Console.ReadLine();
            
            Console.Write("\nFull or partial refund? (f/p): ");
            var refundType = Console.ReadLine()?.ToLower();
            
            try
            {
                if (refundType == "p")
                {
                    Console.Write("Enter refund amount (in cents): ");
                    if (long.TryParse(Console.ReadLine(), out var amount))
                    {
                        Console.Write($"Confirm partial refund of {amount} cents for order {orderId}? (y/n): ");
                        if (Console.ReadLine()?.ToLower() == "y")
                        {
                            var refund = await Client.CreatePartialRefundAsync(orderId, amount, reason, comment);
                            Console.WriteLine($"\nPartial refund created successfully!");
                            Console.WriteLine($"Refund ID: {refund.Id}");
                            Console.WriteLine($"Amount: {refund.Amount} {refund.Currency}");
                            Console.WriteLine($"Status: {refund.Status}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid amount.");
                    }
                }
                else
                {
                    Console.Write($"Confirm full refund for order {orderId}? (y/n): ");
                    if (Console.ReadLine()?.ToLower() == "y")
                    {
                        var refund = await Client.CreateRefundAsync(orderId, reason, comment);
                        Console.WriteLine($"\nFull refund created successfully!");
                        Console.WriteLine($"Refund ID: {refund.Id}");
                        Console.WriteLine($"Amount: {refund.Amount} {refund.Currency}");
                        Console.WriteLine($"Status: {refund.Status}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create refund: {ex.Message}");
            }
        }
    }
}