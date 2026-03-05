using System;
using System.Drawing;
using Foundation;
using UIKit;

namespace SmartBudgetiOS
{
	public partial class CurrencyCell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("CurrencyCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("CurrencyCell");
		public string iso_symbol { get; private set; }

		public UIView separator;
		private UIView empty_acc_view;
		private UIView checkmark_acc_view;

		public CurrencyCell (IntPtr handle) : base (handle)
		{
		}
		~CurrencyCell()
		{
			Console.WriteLine ("~CurrencyCell");
		}
		public void setCurrency(string iso_symbol)
		{
			this.iso_symbol = iso_symbol;
			labelSymbol.Text = iso_symbol;
			labelName.Text = CurrencyFormat.get_display_string(iso_symbol);
			decimal rate = AppDelegate.app.docs.get_exchange_rate (iso_symbol);
			labelSymbol.TextColor = rate == 0 ? AppDelegate.app.negative_color : UIColor.Black;
		}
		public void updateCheckMark(bool check)
		{
			AccessoryView = check ? checkmark_acc_view : empty_acc_view;
			EditingAccessoryView = AccessoryView;
		}
		public static CurrencyCell Create (UITableView tv)
		{
			CurrencyCell cell = (CurrencyCell)Nib.Instantiate (null, null) [0];
			cell.BackgroundColor = tv.BackgroundColor;
			cell.SelectedBackgroundView = new UIImageView (AppDelegate.selected_cell);
			cell.separator = NameValueCell.add_separator (cell);
			cell.empty_acc_view = new UIView (new RectangleF(0f,0f,14f,16f));
			cell.checkmark_acc_view = new UIImageView (AppDelegate.get_checkmark());

			cell.labelSymbol.BackgroundColor = tv.BackgroundColor;
			cell.labelName.BackgroundColor = tv.BackgroundColor;

			Utility.fix_rtl_label (cell.labelSymbol);
			Utility.fix_rtl_label (cell.labelName);
			return cell;
		}
	}
}

