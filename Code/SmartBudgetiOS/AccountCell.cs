using System;
using System.Drawing;
using Foundation;
using UIKit;
using CoreGraphics;
using System.Collections.Generic;
using SmartBudgetCommon;

namespace SmartBudgetiOS
{
	public partial class AccountCell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("AccountCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("AccountCell");
		private List<AccountCurrencyLine> lines = new List<AccountCurrencyLine>();

		private Action<UITableViewCell> taphold_action;
		private UILongPressGestureRecognizer taphold_recognizer;
		private UIView separator;

		public AccountCell (IntPtr handle) : base (handle)
		{
		}
		~AccountCell()
		{
			Console.WriteLine ("~AccountCell");
		}
		public nfloat height_for_balance(bool short_balance, int lines)
		{
			nfloat real_lines = Math.Max (0, lines - (short_balance ? 1 : 0));
			return ContentView.Frame.Height + viewLineParagon.Frame.Height * (real_lines - 1);
		}
		static public string copy_account_balance(bool short_balance, List<DBAccountBalance> balance, string report_currency)
		{
			string result = "";
			CurrencyFormat report_formatter = CurrencyFormat.get_currency (report_currency);
			long total_amount_1000 = 0;
			for (int i = 0; i != balance.Count; ++i) {
				string sam = CurrencyFormat.get_currency (balance[i].currency).format_amount_precise (balance[i].sum_1000);
				string scam;
				long cam_1000 = 0;
				if (AppDelegate.app.docs.convert_currency (ref cam_1000, report_currency, balance [i].sum_1000, balance [i].currency)) {
					scam = report_formatter.format_approximate_amount (cam_1000);
					total_amount_1000 += cam_1000;
				} else {
					scam = i18n.get ("ConversionNA");
				}
				if (i != 0)
					result += "\n";
				if (short_balance) {
					result += sam;
				}else{
					result += sam + "\t" + scam;
				}
			}
			if (!short_balance && balance.Count > 1)
				result += "\n\t" + report_formatter.format_approximate_amount (total_amount_1000);
			if( balance.Count == 0 )
				result += report_formatter.format_approximate_amount (total_amount_1000);
			return result;
		}
		public void set_account_balance(string name, bool short_balance, List<DBAccountBalance> balance, string report_currency)
		{
			// Create enough lines, excess 1 in case of short_balance, but do not care
			while (lines.Count < balance.Count) {
				AccountCurrencyLine line = AccountCurrencyLine.Create (labelName); // Will get background color from labelName
				CGRect r = viewLineParagon.Frame;
				r.Y += lines.Count * r.Height;
				line.Frame = r;
				viewLineParagon.Superview.InsertSubview(line, 0); // Under separator
				lines.Add (line);
			}
			labelName.Text = name;
			if (short_balance) {
				if (balance.Count == 0) {
					labelValue.Text = balance.Count.ToString ();// Culture OK, CurrencyFormat.get_currency (report_currency).format_amount_precise (0);
					labelValue.TextColor = AppDelegate.app.color_for_amount (0);
				} else {
					labelValue.Text = CurrencyFormat.get_currency (balance[0].currency).format_amount_precise (balance[0].sum_1000);
					labelValue.TextColor = AppDelegate.app.color_for_amount (balance[0].sum_1000);
				}
				NameValueCell.align_name_label_to_value_label (labelName, labelValue);
				int pos = 0;
				for (; pos < balance.Count - 1; ++pos) {
					string sam = CurrencyFormat.get_currency (balance[pos+1].currency).format_amount_precise (balance[pos+1].sum_1000);
					AccountCurrencyLine line = lines [pos];
					line.Hidden = false;
					line.setNameValue ("", sam);
					line.setNameValueColors (AppDelegate.app.neutral_color, AppDelegate.app.color_for_amount (balance[pos+1].sum_1000));
				}
				for (; pos < lines.Count; ++pos) {
					lines [pos].Hidden = true;
				}
				return;
			}
			CurrencyFormat report_formatter = CurrencyFormat.get_currency (report_currency);
			long total_amount_1000 = 0;
			for (int i = 0; i != balance.Count; ++i) {
				AccountCurrencyLine line = lines [i];
				line.Hidden = false;
				string sam = CurrencyFormat.get_currency (balance[i].currency).format_amount_precise (balance[i].sum_1000);
				string scam;
				long cam_1000 = 0;
				if (AppDelegate.app.docs.convert_currency (ref cam_1000, report_currency, balance [i].sum_1000, balance [i].currency)) {
					scam = report_formatter.format_approximate_amount (cam_1000);
					total_amount_1000 += cam_1000;
				} else {
					scam = i18n.get ("ConversionNA");
				}
				if (short_balance) {
					line.setNameValue ("", sam);
					line.setNameValueColors (AppDelegate.app.neutral_color, AppDelegate.app.color_for_amount (balance[i].sum_1000));
				}else{
					line.setNameValue (sam, scam);
					line.setNameValueColors (AppDelegate.app.color_for_amount (balance[i].sum_1000),AppDelegate.app.conversion_color);
				}
			}
			for (int i = balance.Count; i != lines.Count; ++i) {
				lines [i].Hidden = true;
			}
			if (short_balance && balance.Count != 0)
				labelValue.Text = "";
			else
				labelValue.Text = report_formatter.format_approximate_amount (total_amount_1000);
			labelValue.TextColor = AppDelegate.app.conversion_color;
			NameValueCell.align_name_label_to_value_label (labelName, labelValue);
		}
		private void configure(UITableView tv)
		{
			SelectedBackgroundView = new UIImageView (AppDelegate.selected_cell);
			separator = NameValueCell.add_separator (this);
			labelName.BackgroundColor = tv.BackgroundColor;
			labelValue.BackgroundColor = tv.BackgroundColor;
			Utility.fix_rtl_label (labelName);
			Utility.fix_rtl_label (labelValue);
		}
		public static AccountCell Create (UITableView tv)
		{
			AccountCell cell = (AccountCell)Nib.Instantiate (null, null) [0];
			cell.configure (tv);
			return cell;
		}
		public static AccountCell Create (UITableView tv, Action<UITableViewCell> taphold_action)
		{
			AccountCell cell = (AccountCell)Nib.Instantiate (null, null) [0];
			cell.BackgroundColor = tv.BackgroundColor;
			cell.taphold_action = taphold_action;
			var weak_cell = new WeakReference(cell);
			cell.taphold_recognizer = new UILongPressGestureRecognizer( (r)=>{
				if( r.State == UIGestureRecognizerState.Began ) {
					var strong_cell = weak_cell.Target as AccountCell;
					if( strong_cell != null)
						strong_cell.taphold_action.Invoke(strong_cell);
				}
			});
			cell.ContentView.AddGestureRecognizer (cell.taphold_recognizer);
			cell.configure (tv);
			return cell;
		}
	}
}

