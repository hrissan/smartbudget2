using System;
using System.Drawing;
using Foundation;
using UIKit;

namespace SmartBudgetiOS
{
	public partial class UiFontSelectCell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("UiFontSelectCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("UiFontSelectCell");
		private UIView empty_acc_view;
		private UIView checkmark_acc_view;
		public UiFontSelectCell (IntPtr handle) : base (handle)
		{
		}
		~UiFontSelectCell()
		{
			Console.WriteLine ("~UiFontSelectCell");
		}
		public void configAccessory(bool acc, bool checkmark)
		{
			if (!acc) {
				//Accessory = UITableViewCellAccessory.None;
				AccessoryView = null;
			} else {
				//Accessory = checkmark ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
				AccessoryView = checkmark ? checkmark_acc_view : empty_acc_view;
			}
		}
		public void setFontName(string name)
		{
			UIFont f = UIFont.FromName (name, 20);  // TODO - constant
			labelName.Font = f;
			labelName.Text = name;
		}
		public static UiFontSelectCell Create (UITableView tv)
		{
			UiFontSelectCell cell = (UiFontSelectCell)Nib.Instantiate (null, null) [0];
			cell.BackgroundColor = tv.BackgroundColor;
			cell.SelectedBackgroundView = new UIImageView (AppDelegate.selected_cell);
			cell.empty_acc_view = new UIView (new RectangleF(0f,0f,14f,16f));
			cell.checkmark_acc_view = new UIImageView (AppDelegate.get_checkmark());
			cell.labelName.BackgroundColor = tv.BackgroundColor;
			NameValueCell.add_separator (cell);
			Utility.fix_rtl_label (cell.labelName);
			return cell;
		}
	}
}

