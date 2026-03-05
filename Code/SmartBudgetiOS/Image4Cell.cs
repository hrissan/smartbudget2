using System;
using System.Drawing;
using Foundation;
using CoreGraphics;
using UIKit;

namespace SmartBudgetiOS
{
	public partial class Image4Cell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("Image4Cell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("Image4Cell");
		public ImageSquareView[] square_views;

		public Image4Cell (IntPtr handle) : base (handle)
		{
		}
		~Image4Cell()
		{
			Console.WriteLine ("~Image4Cell");
		}	
		public static int get_columns()
		{
			return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? 6 : 4;
		}
		public static Image4Cell Create (UITableView tv, Action<ImageSquareView, int, int> cat_click)
		{
			Image4Cell c = (Image4Cell)Nib.Instantiate (null, null) [0];
			c.BackgroundColor = tv.BackgroundColor;
			int num = get_columns ();
			nfloat hei = c.ContentView.Bounds.Height;
			nfloat wid = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? 90 : 80;
			c.square_views = new ImageSquareView[num];
			for(int i = 0; i != num; ++i)
			{
				c.square_views[i] = ImageSquareView.Create(tv, cat_click);
				c.square_views[i].Frame = new CGRect(i*wid, 0, wid, hei);
				c.ContentView.Add(c.square_views[i]);
				Utility.fix_rtl_view (c.square_views [i]);
			}
			return c;
		}
	}
}

