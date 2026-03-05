using System;
using System.Drawing;
using Foundation;
using UIKit;
using SmartBudgetCommon;

namespace SmartBudgetiOS
{
	public partial class ImageSquareView : UIView
	{
		public static readonly UINib Nib = UINib.FromName ("ImageSquareView", NSBundle.MainBundle);
		private int section;
		private int pos;
		public ImageSquareView (IntPtr handle) : base (handle)
		{
		}
		~ImageSquareView()
		{
			Console.WriteLine ("~ImageSquareView");
		}
		public void set_image(UIImage image, bool selected)
		{
			imgCategory.Image = image;
			imgSelected.Hidden = !selected;
			Hidden = image == null;
		}
		public void set_section_row(int section, int pos)
		{
			this.section = section;
			this.pos = pos;
		}
		public void set_selection(bool selected)
		{
			imgSelected.Hidden = !selected;
		}
		private TimerForButton hold_timer;
		public static ImageSquareView Create (UITableView tv, Action<ImageSquareView, int, int> cat_click)
		{
			ImageSquareView v = (ImageSquareView)Nib.Instantiate (null, null) [0];
			v.btnSelect.TouchUpInside += (sender, e) => {
				cat_click.Invoke(v, v.section, v.pos);
			};
			v.BackgroundColor = tv.BackgroundColor;
			return v;
		}
	}
}

