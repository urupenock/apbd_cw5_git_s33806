using System;
using LegacyRenewalApp.Interfaces;
using LegacyRenewalApp.Services;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ISubscriptionPlanRepository _planRepository;
        private readonly IBillingServices _billingService;
        private readonly ITaxService _taxService;
        private readonly IDiscountService _discountService;
        private readonly IPaymentFeeService _paymentFeeService; 
        
        public SubscriptionRenewalService() : this(
            new CustomerRepository(), 
            new SubscriptionPlanRepository(), 
            new LegacyBillingServiceAdapter(),
            new TaxService(),
            new DiscountService(),
            new PaymentFeeService()) 
        {
        }
        
        public SubscriptionRenewalService(
            ICustomerRepository customerRepository,
            ISubscriptionPlanRepository planRepository,
            IBillingServices billingService,
            ITaxService taxService,
            IDiscountService discountService,
            IPaymentFeeService paymentFeeService) 
        {
            _customerRepository = customerRepository;
            _planRepository = planRepository;
            _billingService = billingService;
            _taxService = taxService;
            _discountService = discountService;
            _paymentFeeService = paymentFeeService; 
        }

        public RenewalInvoice CreateRenewalInvoice(int customerId, string planCode, int seatCount, 
            string paymentMethod, bool includePremiumSupport, bool useLoyaltyPoints)
        {
            ValidateInput(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();

            var customer = _customerRepository.GetById(customerId);
            var plan = _planRepository.GetByCode(normalizedPlanCode);

            if (customer == null) throw new InvalidOperationException("Customer not found");
            if (!customer.IsActive) throw new InvalidOperationException("Inactive customers cannot renew");
            
            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            var discount = _discountService.CalculateTotalDiscount(customer, plan, seatCount, baseAmount, useLoyaltyPoints);
            
            decimal discountAmount = discount.Amount;
            string notes = discount.Description;

            decimal subtotalAfterDiscount = Math.Max(baseAmount - discountAmount, 300m);
            if (baseAmount - discountAmount < 300m) notes += "minimum discounted subtotal applied; ";

            decimal supportFee = CalculateSupportFee(normalizedPlanCode, includePremiumSupport, ref notes);

            decimal paymentFee = _paymentFeeService.CalculateFee(normalizedPaymentMethod, subtotalAfterDiscount + supportFee);
            if (paymentFee > 0 || normalizedPaymentMethod != "INVOICE") notes += $"{normalizedPaymentMethod.ToLower()} fee; ";
            
            decimal taxRate = _taxService.GetTaxRate(customer.Country);
            decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;
            decimal taxAmount = taxBase * taxRate;
            decimal finalAmount = Math.Max(taxBase + taxAmount, 500m);
            if (taxBase + taxAmount < 500m) notes += "minimum invoice amount applied; ";
            
            var invoice = BuildInvoice(customerId, customer.FullName, normalizedPlanCode, normalizedPaymentMethod, 
                seatCount, baseAmount, discountAmount, supportFee, paymentFee, taxAmount, finalAmount, notes);

            _billingService.SaveInvoice(invoice);
            SendNotification(customer, normalizedPlanCode, invoice);

            return invoice;
        }
        
        private void ValidateInput(int id, string code, int seats, string method)
        {
            if (id <= 0) throw new ArgumentException("Customer id must be positive");
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Plan code is required");
            if (seats <= 0) throw new ArgumentException("Seat count must be positive");
            if (string.IsNullOrWhiteSpace(method)) throw new ArgumentException("Payment method is required");
        }

        private decimal CalculateSupportFee(string plan, bool include, ref string notes)
        {
            if (!include) return 0m;
            notes += "premium support included; ";
            return plan switch { "START" => 250m, "PRO" => 400m, "ENTERPRISE" => 700m, _ => 0m };
        }

        private RenewalInvoice BuildInvoice(int cId, string name, string plan, string method, int seats, 
            decimal baseAmt, decimal discAmt, decimal suppFee, decimal payFee, decimal taxAmt, decimal finalAmt, string notes)
        {
            return new RenewalInvoice {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{cId}-{plan}",
                CustomerName = name, PlanCode = plan, PaymentMethod = method, SeatCount = seats,
                BaseAmount = Math.Round(baseAmt, 2), DiscountAmount = Math.Round(discAmt, 2),
                SupportFee = Math.Round(suppFee, 2), PaymentFee = Math.Round(payFee, 2),
                TaxAmount = Math.Round(taxAmt, 2), FinalAmount = Math.Round(finalAmt, 2),
                Notes = notes.Trim(), GeneratedAt = DateTime.UtcNow
            };
        }

        private void SendNotification(Customer customer, string plan, RenewalInvoice invoice)
        {
            if (string.IsNullOrWhiteSpace(customer.Email)) return;
            string body = $"Hello {customer.FullName}, your renewal for {plan} is ready. Total: {invoice.FinalAmount:F2}.";
            _billingService.SendEmail(customer.Email, "Subscription renewal invoice", body);
        }
    }
}