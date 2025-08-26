using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Services.Static
{
    public static class CurrencyConverter
    {
        // سعر الصرف ثابت - ممكن تخليه يجي من config أو من DB
        private const decimal EGP_TO_USD_RATE = 50.0m;

        /// <summary>
        /// Converts EGP amount to USD based on predefined rate.
        /// </summary>
        /// <param name="amountInEGP">Amount in Egyptian Pounds</param>
        /// <returns>Equivalent amount in USD</returns>
        public static decimal ConvertEgpToUsd(decimal amountInEGP)
        {
            if (amountInEGP <= 0)
                throw new ArgumentException("Amount must be greater than zero.", nameof(amountInEGP));

            return Math.Round(amountInEGP / EGP_TO_USD_RATE, 2, MidpointRounding.AwayFromZero);
        }
    }
}
