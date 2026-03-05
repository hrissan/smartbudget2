using System;
using System.Drawing;
using Foundation;
using UIKit;

namespace SmartBudgetiOS
{
	public partial class ReportCell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("ReportCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("ReportCell");

		private UIView empty_acc_view;
		private UIView checkmark_acc_view;
		private UIView separator;

		public ReportCell (IntPtr handle) : base (handle)
		{
		}
		~ReportCell()
		{
			Console.WriteLine ("~ReportCell");
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
		public void setImageName(UIImage image, string name, string value, string value2)
		{
			imgImage.Image = image;
			labelName.Text = name;
			labelValue.Text = value;
			labelValue2.Text = value2;
		}
		private void configure(UITableView tv) 
		{
			SelectedBackgroundView = new UIImageView (AppDelegate.selected_cell);
			separator = NameValueCell.add_separator (this);
			empty_acc_view = new UIView (new RectangleF(0f,0f,14f,16f));
			checkmark_acc_view = new UIImageView (AppDelegate.get_checkmark());
			BackgroundColor = tv.BackgroundColor;
			labelName.BackgroundColor = tv.BackgroundColor;
			labelValue.BackgroundColor = tv.BackgroundColor;
			labelValue2.BackgroundColor = tv.BackgroundColor;
			Utility.fix_rtl_label (labelName);
			Utility.fix_rtl_label (labelValue);
			Utility.fix_rtl_label (labelValue2);
			Utility.fix_rtl_view (imgImage);
		}
		public static ReportCell Create (UITableView tv)
		{
			ReportCell cell =  (ReportCell)Nib.Instantiate (null, null) [0];
			cell.configure (tv);
			return cell;
		}
	}
}

