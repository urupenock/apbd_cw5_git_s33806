namespace LegacyRenewalApp.Interfaces;

public interface IBillingServices
{
    void SaveInvoice(RenewalInvoice invoice);
    void SendEmail(string email, string subject, string body);
}