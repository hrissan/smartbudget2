using System;
using System.Drawing;
using Foundation;
using CoreGraphics;
using UIKit;

namespace SmartBudgetiOS
{
	public partial class Category4Cell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("Category4Cell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("Category4Cell");
		public CategorySquareView[] square_views;
		public UIView separator;
		public Category4Cell (IntPtr handle) : base (handle)
		{
		}
		~Category4Cell()
		{
			Console.WriteLine ("~Category4Cell");
		}	
		public static int get_columns()
		{
			return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? 6 : 4;
		}
		public static Category4Cell Create (UITableView tv, Action<string> cat_click, Action<string, UIView> cat_hold)
		{
			Category4Cell c = (Category4Cell)Nib.Instantiate (null, null) [0];
			c.BackgroundColor = tv.BackgroundColor;
			int num = get_columns ();
			nfloat hei = c.ContentView.Bounds.Height;
			nfloat wid = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? 90 : 80;
			c.square_views = new CategorySquareView[num];
			for(int i = 0; i != num; ++i)
			{
				c.square_views[i] = CategorySquareView.Create(tv, cat_click, cat_hold);
				c.square_views[i].Frame = new CGRect(i*wid, 0, wid, hei);
				c.ContentView.Add(c.square_views[i]);
				Utility.fix_rtl_view (c.square_views [i]);
			}
			c.separator = NameValueCell.add_separator (c);
			return c;
		}
	}
}

