using System;
using Foundation;
using System.Collections.Generic;
using System.Globalization;
using SmartBudgetCommon;

namespace SmartBudgetiOS
{
	public class CurrencyFormat
	{
		protected NSNumberFormatter numberFormatter;
		protected NSNumberFormatter numberFormatterWhole;
		private static Dictionary<string, CurrencyFormat> used = new Dictionary<string, CurrencyFormat>();
		private static List<string> all_currencies;

		public string iso_symbol { get; private set; }
		public double exchange_rate_to_dollar; // 0 - unknown
		public CurrencyFormat (string iso_symbol)
		{
			this.iso_symbol = iso_symbol;
		}
		public static CurrencyFormat get_currency (string iso_symbol)
		{
			CurrencyFormat c;
			if (used.TryGetValue (iso_symbol, out c))
				return c;
			c = iso_symbol == "BTC" ? new CurrencyFormatBTC() : iso_symbol == "KZT" ? new CurrencyFormatKZT() : new CurrencyFormat(iso_symbol);
			used.Add (iso_symbol, c);
			return c;
		}
		public static List<string> get_all_currencies()
		{
			if (all_currencies == null) {
				all_currencies = new List<string> ();
				foreach (string str in NSLocale.CommonISOCurrencyCodes) {
					all_currencies.Add (str);
				}
				//all_currencies.Add ("BTC");
				all_currencies.Sort ();
			}
			return all_currencies;
		}
		public static string get_display_string(string iso_symbol)
		{
			if (iso_symbol == "BTC")
				return i18n.get ("BitCoin");
			return NSLocale.CurrentLocale.GetCurrencyCodeDisplayName (iso_symbol);
		}
		public int get_maximum_precision()
		{
			create_formatters ();
			return (int)numberFormatter.MaximumFractionDigits;
		}
		public virtual bool float_behavior()
		{
			return false;
		}
		public int get_precision_exp()
		{
			int mp = get_maximum_precision();
			int result = 1;
			for(int i = 0; i < mp; ++i)
				result *= 10;
			return result;    
		}
		protected virtual long make_approximate(long val_1000)
		{
			if (val_1000 >= 100*1000)
				val_1000 = (val_1000 / 1000) * 1000;
			return val_1000;
		}
		public string format_approximate_amount(long amount_1000)
		{
			int sgn = Math.Sign (amount_1000);
			long val = make_approximate( Math.Abs(amount_1000) );
			return format_amount_precise(sgn*val);
		}
		public string format_amount_precise(long amount_1000)
		{
			return format_amount_for_enter (amount_1000, amount_1000 % 1000 != 0, -1);
		}
		//static private NSDecimalNumber test_dec = new NSDecimalNumber ("12.12");
		public virtual string format_amount_for_enter(long amount_1000, bool dot, int minimum_frac)
		{
			create_formatters();
			NSNumberFormatter nuf = dot ? numberFormatter : numberFormatterWhole;
			//string result = nuf.StringFromNumber(new NSDecimalNumber(amount.ToString(CultureInfo.InvariantCulture)));
			string result = nuf.StringFromNumber(new NSDecimalNumber(Math.Abs(amount_1000), -3, amount_1000 < 0));
			return result;
		}
		protected virtual void create_formatters()
		{
			if (numberFormatter == null) {
				numberFormatter = new NSNumberFormatter();
				numberFormatter.NumberStyle = NSNumberFormatterStyle.Currency;
				numberFormatter.CurrencyCode = iso_symbol;
			}
			if (numberFormatterWhole == null) {
				numberFormatterWhole = new NSNumberFormatter();
				numberFormatterWhole.NumberStyle = NSNumberFormatterStyle.Currency;
				numberFormatterWhole.CurrencyCode = iso_symbol;
				numberFormatterWhole.MinimumFractionDigits = 0;
			}
		}
	}
}

