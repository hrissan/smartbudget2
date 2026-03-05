using System;
using System.Drawing;
using Foundation;
using UIKit;

namespace SmartBudgetiOS
{
	public partial class AccountCurrencyLine : UIView
	{
		public static readonly UINib Nib = UINib.FromName ("AccountCurrencyLine", NSBundle.MainBundle);

		public AccountCurrencyLine (IntPtr handle) : base (handle)
		{
		}
		~AccountCurrencyLine()
		{
			Console.WriteLine ("~AccountCurrencyLine");
		}		
		public void setNameValue(string name, string value)
		{
			labelName.Text = name;
			labelValue.Text = value;
		}
		public void setNameValueColors(UIColor name, UIColor value)
		{
			labelName.TextColor = name;
			labelValue.TextColor = value;
		}
		public static AccountCurrencyLine Create (UIView tv)
		{
			AccountCurrencyLine result = (AccountCurrencyLine)Nib.Instantiate (null, null) [0];
			result.labelName.BackgroundColor = tv.BackgroundColor;
			result.labelValue.BackgroundColor = tv.BackgroundColor;
			result.BackgroundColor = tv.BackgroundColor;
			Utility.fix_rtl_label (result.labelName);
			Utility.fix_rtl_label (result.labelValue);
			return result;
		}
	}
}

