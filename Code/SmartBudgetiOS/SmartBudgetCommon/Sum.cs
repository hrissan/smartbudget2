using System;

namespace SmartBudgetCommon
{
	public struct Sum
	{
		public string account;
		public long amount_1000;
		public string currency; // may be null for sum2

		public bool IsValid()
		{
			return !String.IsNullOrEmpty (account) && !String.IsNullOrEmpty (currency);
		}
	}
}

