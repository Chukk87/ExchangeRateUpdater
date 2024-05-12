using ExchangeRateUpdater.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExchangeRateUpdater.Interfaces
{
    public interface IExchangeRateProvider
    {
        public Task<IEnumerable<ExchangeRate>> GetExchangeRates(IEnumerable<Currency> currencies);
        public decimal ExchangeRateConvert(ExchangeRate SelectedExchangeRate, decimal Amount);
    }
}
