using System;
using Foundation;
using System.Globalization;

namespace SmartBudgetiOS
{
	public class CurrencyFormatBTC : CurrencyFormat
	{
		public const string BTC_symbol = "ᗸ";
		public CurrencyFormatBTC ():base("BTC")
		{
		}
		public override bool float_behavior()
		{
			return true;
		}
		protected override long make_approximate(long val_1000)
		{
			int counter = 0;
			while (val_1000 >= 1000 && counter < 10) {
				val_1000 /= 10;
				counter += 1;
			}
			//val = Math.Floor(val + 0.5m);
			while (counter != 0) {
				val_1000 *= 10;
				counter -= 1;
			}
			return val_1000;
		}
		public override string format_amount_for_enter(long amount_1000, bool dot, int minimum_frac)
		{
			return base.format_amount_for_enter (amount_1000, dot, minimum_frac);
/*			create_formatters();
			NSNumberFormatter nuf = dot ? numberFormatter : numberFormatterWhole;
			int was_minimum_fraction_digits = nuf.MinimumFractionDigits;
			bool was_show_dec = nuf.AlwaysShowsDecimalSeparator;
			if (dot && minimum_frac != -1) {
				nuf.MinimumFractionDigits = minimum_frac;
				nuf.AlwaysShowsDecimalSeparator = true;
			}
			string result = nuf.StringFromNumber(Utility.from_decimal(amount));
			if (dot && minimum_frac != -1) {
				nuf.MinimumFractionDigits = was_minimum_fraction_digits;
				nuf.AlwaysShowsDecimalSeparator = was_show_dec;
			}
			return result;*/
		}
		protected override void create_formatters()
		{
			base.create_formatters ();

			numberFormatter.CurrencySymbol = BTC_symbol;
			numberFormatter.InternationalCurrencySymbol = BTC_symbol;
			numberFormatter.MinimumFractionDigits = 0;
			numberFormatter.MaximumFractionDigits = 8;

			numberFormatterWhole.CurrencySymbol = BTC_symbol;
			numberFormatterWhole.InternationalCurrencySymbol = BTC_symbol;
		}
	}
}

