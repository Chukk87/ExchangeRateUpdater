using ExchangeRateUpdater.Classes;
using ExchangeRateUpdater.Interfaces;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExchangeRateUpdater.Services
{
    public class ExchangeRateProvider : IExchangeRateProvider
    {
        /// <summary>
        /// Should return exchange rates among the specified currencies that are defined by the source. But only those defined
        /// by the source, do not return calculated exchange rates. E.g. if the source contains "CZK/USD" but not "USD/CZK",
        /// do not return exchange rate "USD/CZK" with value calculated as 1 / "CZK/USD". If the source does not provide
        /// some of the currencies, ignore them.
        /// </summary>
        public async Task<IEnumerable<ExchangeRate>> GetExchangeRates(IEnumerable<Currency> currencies)
        {
            var webCurrencyTableRates = await GetWebCurrencyTable();
            //Now filtering by supplied currencies
            webCurrencyTableRates = webCurrencyTableRates.Where(wc => currencies.Any(cc => cc.Code == wc.Code)).ToList();
            
            //Populating the exchange rate class list
            List<ExchangeRate> exchangeRates = webCurrencyTableRates.Select(webCurrencyTable =>
            {
                // Extract data from the filtered WebExchangeRate list
                Currency sourceCurrency = new Currency(SettingsConstants.BankCurrency);
                Currency targetCurrency = new Currency(webCurrencyTable.Code);
                decimal value = Convert.ToDecimal(webCurrencyTable.Rate) / Convert.ToDecimal(webCurrencyTable.Amount); //Rate on website is divisible by the amount to get true figure
                value = Math.Floor(value * 1000) / 1000; //Round down to 3 decimal places

                // Create and return an instance of ExchangeRate using the extracted values
                return new ExchangeRate(sourceCurrency, targetCurrency, value);
            }).ToList();


            return exchangeRates;
        }

        /// <summary>
        /// Extracts the table rates from a website defined in the SettingsConstants
        /// </summary>
        /// <returns>List of WebExchangeRates</returns>
        private async Task<IEnumerable<WebExchangeRate>> GetWebCurrencyTable()
        {
            //Capture the HTML source
            using var httpClient = new HttpClient();
            var htmlSource = await httpClient.GetStringAsync(SettingsConstants.ExchangeFxWebsite);

            //Load the Html source into HtmlAgility and extract the table
            var documemt = new HtmlDocument();
            documemt.LoadHtml(htmlSource);

            var currencyTable = documemt.DocumentNode.SelectSingleNode(string.Concat("//table[@class='", SettingsConstants.SourceTableName, "']"));
            var webCurrencyTable = new List<WebExchangeRate>();

            //Build class list
            if (currencyTable != null)
            {
                //Get all rows from the table
                var rows = currencyTable.SelectNodes(".//tr");

                foreach (var row in rows) 
                {
                    // Selecting all cells within the row
                    var cells = row.SelectNodes(".//td");
                    if (cells != null && cells.Count >= 5)
                    {
                        //Build WebCurrencyClass and populate properties with cell data
                        WebExchangeRate data = new WebExchangeRate();

                        data.Country = cells[0].InnerText.Trim();
                        data.Currency = cells[1].InnerText.Trim();
                        data.Amount = cells[2].InnerText.Trim();
                        data.Code = cells[3].InnerText.Trim();
                        data.Rate = cells[4].InnerText.Trim();

                        webCurrencyTable.Add(data);
                    }
                }
            }
            documemt.DocumentNode.RemoveAll();

            return webCurrencyTable;
        }
    }
}