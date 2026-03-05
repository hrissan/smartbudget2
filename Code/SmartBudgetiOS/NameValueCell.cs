using System;
using System.Drawing;
using Foundation;
using UIKit;
using CoreGraphics;

namespace SmartBudgetiOS
{
	public partial class NameValueCell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("NameValueCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("NameValueCell");

		public NameValueCell (IntPtr handle) : base (handle)
		{
		}
		~NameValueCell()
		{
			Console.WriteLine ("~NameValueCell");
		}
		public void setImage(UIImage img)
		{
			this.icon.Image = img;
		}
		public UIImageView get_image_view()
		{
			return this.icon;
		}
		public void setValueColor(UIColor color)
		{
			this.value.TextColor = color;
		}
		public void setNameValueComment(string name, string value, string comment)
		{
			this.name.Text = name;
			this.value.Text = value;
			this.comment.Text = comment;
			align_name_label_to_value_label (this.name, this.value);
			this.comment.Hidden = String.IsNullOrEmpty (comment);
			this.name.Hidden = String.IsNullOrEmpty (name);
		}
		static public void align_name_label_to_value_label(UILabel name, UILabel value)
		{
			CGRect fr = name.Frame;
			CGRect vfr = value.Frame;
			CGSize max_size = new CGSize (Utility.RTL ? fr.X + fr.Width - vfr.X : vfr.X + vfr.Width - fr.X, vfr.Height);

			NSString nsstr = new NSString (value.Text);
			CGSize vsi = nsstr.StringSize (value.Font, max_size, value.LineBreakMode);
			vsi.Width = (float)Math.Ceiling(vsi.Width); // iOS 8+ fix

			if (!Utility.RTL) {
				fr.Width = vfr.X + vfr.Width - fr.X - vsi.Width;
				vfr.X = vfr.X + vfr.Width - vsi.Width;
				vfr.Width = vsi.Width;
			} else {
				nfloat fsi = vfr.X + vsi.Width - fr.X;
				fr.X += fsi;
				fr.Width -= fsi;
				vfr.Width = vsi.Width;
			}
			name.Frame = fr;
			value.Frame = vfr;
		}
		private static UIImage separator_image = UIImage.FromBundle("new_design/separator.png").CreateResizableImage(new UIEdgeInsets(0,0,0,0), UIImageResizingMode.Tile);
		public static UIView add_separator(UITableViewCell cell)
		{
			UIImageView bim = new UIImageView (separator_image);
			bim.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin | UIViewAutoresizing.FlexibleWidth;
			CGRect rr = cell.ContentView.Bounds;
			rr.X = 18;//320 - bim.Frame.Width; // CONSTANT in code
			rr.Y = rr.Height - separator_image.Size.Height;
			rr.Height = separator_image.Size.Height;
			rr.Width = rr.Width - rr.X;
			if (Utility.RTL)
				rr.X = 0; // Quicker fix, than calling fix_rtl_view
			bim.Frame = rr;
			cell.ContentView.InsertSubview (bim, 0);
			return bim;
		}
		public static NameValueCell Create (UITableView tv)
		{
			NameValueCell cell = (NameValueCell)Nib.Instantiate (null, null) [0];
			cell.SelectedBackgroundView = new UIImageView (AppDelegate.selected_cell);
			cell.BackgroundColor = tv.BackgroundColor;
			//cell.name.BackgroundColor = tv.BackgroundColor;
			//cell.value.BackgroundColor = tv.BackgroundColor;
			//cell.comment.BackgroundColor = tv.BackgroundColor;
			add_separator (cell);
			Utility.fix_rtl_label (cell.name);
			Utility.fix_rtl_label (cell.value);
			Utility.fix_rtl_label (cell.comment);
			Utility.fix_rtl_view (cell.icon);
			return cell;
		}
	}
}

