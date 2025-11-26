using EcomShopping.Domain.Interfaces;

namespace EcomShopping.Infrastructure.Payment;

public class FakePaymentProvider : IPaymentProvider
{
    private readonly Dictionary<string, PaymentTransaction> _transactions = new();
    private int _transactionCounter = 1000;

    public Task<PaymentResult> AuthorizePaymentAsync(PaymentRequest request)
    {
        // Simulate payment authorization
        var transactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{_transactionCounter++}";
        
        // Simple validation: fail if card number ends with "0000"
        var shouldFail = request.CardNumber?.EndsWith("0000") ?? false;
        
        if (shouldFail)
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                ErrorMessage = "Payment declined by issuing bank",
                ErrorCode = "DECLINED"
            });
        }

        // Store transaction
        _transactions[transactionId] = new PaymentTransaction
        {
            TransactionId = transactionId,
            Amount = request.Amount,
            Status = "Authorized",
            CreatedAt = DateTime.UtcNow
        };

        return Task.FromResult(new PaymentResult
        {
            Success = true,
            TransactionId = transactionId,
            Metadata = new Dictionary<string, string>
            {
                { "AuthorizationCode", Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper() }
            }
        });
    }

    public Task<PaymentResult> CapturePaymentAsync(string transactionId, decimal amount)
    {
        if (!_transactions.ContainsKey(transactionId))
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                ErrorMessage = "Transaction not found",
                ErrorCode = "NOT_FOUND"
            });
        }

        var transaction = _transactions[transactionId];
        
        if (transaction.Status != "Authorized")
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                ErrorMessage = $"Cannot capture payment in status: {transaction.Status}",
                ErrorCode = "INVALID_STATUS"
            });
        }

        if (amount > transaction.Amount)
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                ErrorMessage = "Capture amount exceeds authorized amount",
                ErrorCode = "AMOUNT_EXCEEDED"
            });
        }

        transaction.Status = "Captured";
        transaction.CapturedAmount = amount;

        return Task.FromResult(new PaymentResult
        {
            Success = true,
            TransactionId = transactionId
        });
    }

    public Task<PaymentResult> RefundPaymentAsync(string transactionId, decimal amount)
    {
        if (!_transactions.ContainsKey(transactionId))
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                ErrorMessage = "Transaction not found",
                ErrorCode = "NOT_FOUND"
            });
        }

        var transaction = _transactions[transactionId];
        
        if (transaction.Status != "Captured")
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                ErrorMessage = "Can only refund captured payments",
                ErrorCode = "INVALID_STATUS"
            });
        }

        var capturedAmount = transaction.CapturedAmount ?? transaction.Amount;
        if (amount > capturedAmount)
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                ErrorMessage = "Refund amount exceeds captured amount",
                ErrorCode = "AMOUNT_EXCEEDED"
            });
        }

        transaction.Status = amount >= capturedAmount ? "Refunded" : "PartiallyRefunded";
        transaction.RefundedAmount = (transaction.RefundedAmount ?? 0) + amount;

        return Task.FromResult(new PaymentResult
        {
            Success = true,
            TransactionId = transactionId
        });
    }

    public Task<PaymentStatusResult> GetPaymentStatusAsync(string transactionId)
    {
        if (!_transactions.ContainsKey(transactionId))
        {
            return Task.FromResult(new PaymentStatusResult
            {
                Success = false,
                Status = "NotFound"
            });
        }

        var transaction = _transactions[transactionId];

        return Task.FromResult(new PaymentStatusResult
        {
            Success = true,
            Status = transaction.Status,
            TransactionId = transactionId,
            Amount = transaction.Amount
        });
    }

    private class PaymentTransaction
    {
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? CapturedAmount { get; set; }
        public decimal? RefundedAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
