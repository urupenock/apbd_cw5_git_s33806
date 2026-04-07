namespace LegacyRenewalApp.Services;
using LegacyRenewalApp.Interfaces;
using System;

public class PaymentFeeService : IPaymentFeeService
{
    public decimal CalculateFee(string paymentMethod, decimal amount)
    {
        return paymentMethod switch
        {
            "CARD" => amount * 0.02m,
            "BANK_TRANSFER" => amount * 0.01m,
            "PAYPAL" => amount * 0.035m,
            "INVOICE" => 0m,
            _ => throw new ArgumentException("Unsupported payment method")
        };
    }
}