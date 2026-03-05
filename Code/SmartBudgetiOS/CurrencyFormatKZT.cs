using System;

namespace SmartBudgetiOS
{
	public class CurrencyFormatKZT : CurrencyFormat
	{
		public CurrencyFormatKZT ():base("KZT")
		{
		}
		public override string format_amount_for_enter(long amount_1000, bool dot, int minimum_frac)
		{
			string result = base.format_amount_for_enter (amount_1000, dot, minimum_frac);
			return result.Replace ("\u20B8", "〒");
		}
	}
}

