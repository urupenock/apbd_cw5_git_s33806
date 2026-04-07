namespace LegacyRenewalApp.Interfaces
{
    public interface ITaxService
    {
        decimal GetTaxRate(string country);
    }
}