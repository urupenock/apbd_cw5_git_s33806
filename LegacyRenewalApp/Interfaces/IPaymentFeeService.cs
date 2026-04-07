namespace LegacyRenewalApp.Interfaces;

public interface IPaymentFeeService
{
    decimal CalculateFee(string paymentMethod, decimal amount);
}