using System;
using System.Drawing;
using Foundation;
using UIKit;

namespace SmartBudgetiOS
{
	public partial class SimpleCellBig : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("SimpleCellBig", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("SimpleCellBig");

		public SimpleCellBig (IntPtr handle) : base (handle)
		{
		}
		~SimpleCellBig()
		{
			Console.WriteLine ("~SimpleCellBig");
		}		
		public void setNameValue(UIImage image, string name, string value)
		{
			imgImage.Image = image;
			labelName.Text = name;
			labelValue.Text = value;
		}

		public static SimpleCellBig Create (UITableView tv)
		{
			SimpleCellBig cell = (SimpleCellBig)Nib.Instantiate (null, null) [0];
			cell.BackgroundColor = tv.BackgroundColor;
			cell.labelName.BackgroundColor = tv.BackgroundColor;
			cell.labelValue.BackgroundColor = tv.BackgroundColor;
			NameValueCell.add_separator (cell);
			Utility.fix_rtl_label (cell.labelName);
			Utility.fix_rtl_label (cell.labelValue);
			Utility.fix_rtl_view (cell.imgImage);
			return cell;
		}
	}
}

