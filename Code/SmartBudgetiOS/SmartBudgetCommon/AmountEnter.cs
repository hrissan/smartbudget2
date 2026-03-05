using System;
using System.Globalization;

namespace SmartBudgetCommon
{
	public class AmountEnter
	{
		private string value_str = "";
		public bool value_dot { get; private set; }
		private string value_str_ac = "";
		public bool negative = true;
		static string[] digit_strings = new string[]{"0","1","2","3","4","5","6","7","8","9"};

		public int maximum_precision = 0;
		public bool float_behavior = false;
		public AmountEnter ()
		{
		}
		public string get_value_str_ac()
		{
			return value_str_ac;
		}
		public bool del()
		{
			truncate_value ();
			if (value_str_ac.Length > 0) {
				value_str_ac = value_str_ac.Substring (0, value_str_ac.Length - 1);
				return true;
			}
			if (value_dot) {
				value_dot = false;
				return true;
			}
			if (value_str.Length > 0) {
				value_str = value_str.Substring (0, value_str.Length - 1);
				return true;
			}
			return false; // nothing to delete
		}
		public void comma_click()
		{
			value_dot = true;
		}
		public void minus_click()
		{
			negative = !negative;
		}
		public void clear()
		{
			value_str = "";
			value_dot = false;
			value_str_ac = "";
		}
		public void number_click(int digit)
		{
			truncate_value ();
			if (value_dot) {
				if (value_str_ac.Length >= maximum_precision)
					return;
				value_str_ac += digit_strings [digit];
			} else {
				if( value_str.Length > 12 )
					return;
				value_str += digit_strings [digit];
			}
		}
		public void from_amount(decimal amount, int allowed_sign)
		{
			negative = allowed_sign != 0 ? allowed_sign < 0 : (amount <= 0);

			decimal val = Math.Abs (amount);
			decimal ival = Math.Floor (val);
			decimal aval = val - ival;
			for (int i = 0; i != maximum_precision; ++i)
				aval *= 10;
			long val_ac = (long)Math.Floor(aval);
			value_str = "";
			if (ival != 0)
				value_str += ival.ToString(CultureInfo.InvariantCulture);
			value_dot = false;
			value_str_ac = "";
			if (val_ac != 0) {
				value_dot = true;
				value_str_ac += val_ac.ToString(CultureInfo.InvariantCulture);
			}
		}
		public void truncate_value()
		{
			if (value_str_ac.Length > maximum_precision)
				value_str_ac = value_str_ac.Substring (0, maximum_precision);
			if (maximum_precision == 0)
				value_dot = false;
		}
		public decimal to_amount(out bool is_negative)
		{
			// Do not change any properties in this method
			string str_ac = value_str_ac;
			if (str_ac.Length > maximum_precision)
				str_ac = str_ac.Substring (0, maximum_precision);
			decimal val = 0;
			decimal.TryParse (value_str, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
			decimal val_ac = 0;
			if (float_behavior) {
				decimal.TryParse ("0."+str_ac, NumberStyles.Number, CultureInfo.InvariantCulture, out val_ac);
			} else {
				decimal.TryParse (str_ac, NumberStyles.Number, CultureInfo.InvariantCulture, out val_ac);
				for (int i = 0; i != maximum_precision; ++i)
					val_ac /= 10;
			}
			is_negative = negative;
			return (negative ? -1 : 1) * (val + val_ac);
		}
	}
}

