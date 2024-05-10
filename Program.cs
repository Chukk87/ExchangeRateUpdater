using ExchangeRateUpdater.Classes;
using ExchangeRateUpdater.Preperations;
using ExchangeRateUpdater.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExchangeRateUpdater
{
    public static class Program
    {
        public async static Task Main(string[] args)
        {
            try
            {
                var provider = new ExchangeRateProvider();
                var rates = await provider.GetExchangeRates(AvailableCurrency.Currencies);

                if (rates != null)
                {
                    Console.WriteLine($"Successfully retrieved {rates.Count()} available exchange rates:\n");
                    foreach (var rate in rates)
                    {
                        Console.WriteLine(rate.ToString());
                    }

                    bool validCurrencyInput = false;
                    string selectedCurency = string.Empty;

                    while(!validCurrencyInput)
                    {
                        Console.WriteLine("\nEnter one of the available currency to calculate...");
                        string userInput = Console.ReadLine();

                        //Validate user input
                        validCurrencyInput = rates.Any(c => c.TargetCurrency.ToString() == userInput.Trim().ToUpper());
                        selectedCurency = userInput.Trim().ToUpper();

                        if (validCurrencyInput)
                        {
                            bool validNumber = false;
                            decimal userDecimalInput = 0;

                            while(!validNumber)
                            {
                                Console.WriteLine($"Enter the number of {SettingsConstants.BankCurrency} to exchange...");
                                userInput = Console.ReadLine();

                                //Validate user input
                                validNumber = decimal.TryParse(userInput, out userDecimalInput);

                                if (validNumber)
                                {
                                    //Convert
                                    ExchangeRate selectedExchangeRate = rates.First(er => er.TargetCurrency.ToString() == selectedCurency);
                                    decimal exchangeValue = userDecimalInput * selectedExchangeRate.Value;

                                    Console.WriteLine($"The equvalent value of {userDecimalInput}{SettingsConstants.BankCurrency} is {exchangeValue}{selectedCurency}\n");

                                    //Reset variables to start over
                                    validCurrencyInput = false;
                                    selectedCurency = string.Empty;
                                }
                                else
                                {
                                    Console.WriteLine("Invalid decimal value, please enter a numeric value");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid currency selected");
                        }
                    }
                }
                else { Console.WriteLine("No available currencies"); }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not retrieve exchange rates: '{e.Message}'.");
            }
        }
    }
}