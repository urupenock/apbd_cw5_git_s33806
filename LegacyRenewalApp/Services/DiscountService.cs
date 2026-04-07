using LegacyRenewalApp.Interfaces;
using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Services
{
    public class DiscountService : IDiscountService
    {
        public DiscountResult CalculateTotalDiscount(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
        {
            decimal totalAmount = 0m;
            string notes = string.Empty;
            
            if (customer.Segment == "Silver") { totalAmount += baseAmount * 0.05m; notes += "silver discount; "; }
            else if (customer.Segment == "Gold") { totalAmount += baseAmount * 0.10m; notes += "gold discount; "; }
            else if (customer.Segment == "Platinum") { totalAmount += baseAmount * 0.15m; notes += "platinum discount; "; }
            else if (customer.Segment == "Education" && plan.IsEducationEligible) { totalAmount += baseAmount * 0.20m; notes += "education discount; "; }
            
            if (customer.YearsWithCompany >= 5) { totalAmount += baseAmount * 0.07m; notes += "long-term loyalty discount; "; }
            else if (customer.YearsWithCompany >= 2) { totalAmount += baseAmount * 0.03m; notes += "basic loyalty discount; "; }
            
            if (seatCount >= 50) { totalAmount += baseAmount * 0.12m; notes += "large team discount; "; }
            else if (seatCount >= 20) { totalAmount += baseAmount * 0.08m; notes += "medium team discount; "; }
            else if (seatCount >= 10) { totalAmount += baseAmount * 0.04m; notes += "small team discount; "; }
            
            if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
            {
                int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
                totalAmount += pointsToUse;
                notes += $"loyalty points used: {pointsToUse}; ";
            }

            return new DiscountResult { Amount = totalAmount, Description = notes };
        }
    }
}