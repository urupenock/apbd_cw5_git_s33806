using LegacyRenewalApp.Models;
namespace LegacyRenewalApp.Interfaces
{
    public interface IDiscountService
    {
        DiscountResult CalculateTotalDiscount(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints);
    }
}