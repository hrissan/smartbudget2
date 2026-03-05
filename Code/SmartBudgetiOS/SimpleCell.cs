using System;
using System.Drawing;
using Foundation;
using UIKit;

namespace SmartBudgetiOS
{
	public partial class SimpleCell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("SimpleCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("SimpleCell");
		private Action<UITableViewCell> taphold_action;
		private UILongPressGestureRecognizer taphold_recognizer;
		private UIView empty_acc_view;
		private UIView checkmark_acc_view;
		private UIView separator;

		public SimpleCell (IntPtr handle) : base (handle)
		{
		}
		~SimpleCell()
		{
			Console.WriteLine ("~SimpleCell");
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
		public void setImageName(UIImage image, string name, string value, bool en = true)
		{
			imgImage.Image = image;
			labelName.Text = name;
			labelValue.Text = value;
			labelName.Alpha = en ? 1f : 0.5f;
			labelValue.Alpha = en ? 1f : 0.5f;
		}
		public void center_image()
		{
			imgImage.ContentMode = UIViewContentMode.Center;
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
			Utility.fix_rtl_label (labelName);
			Utility.fix_rtl_label (labelValue);
			Utility.fix_rtl_view (imgImage);
		}
		public static SimpleCell Create (UITableView tv)
		{
			SimpleCell cell =  (SimpleCell)Nib.Instantiate (null, null) [0];
			cell.configure (tv);
			return cell;
		}
		public static SimpleCell Create (UITableView tv, Action<UITableViewCell> taphold_action)
		{
			SimpleCell cell = (SimpleCell)Nib.Instantiate (null, null) [0];
			cell.taphold_action = taphold_action;
			var weak_cell = new WeakReference(cell);
			cell.taphold_recognizer = new UILongPressGestureRecognizer( (r)=>{
				if( r.State == UIGestureRecognizerState.Began ) {
					var strong_cell = weak_cell.Target as SimpleCell;
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

