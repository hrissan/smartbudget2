using System;
using System.Drawing;
using Foundation;
using UIKit;
using CoreGraphics;
using System.IO;
using System.Text;

namespace SmartBudgetiOS
{
	public partial class FaqVC : UIViewController
	{
		UIWebView web_view;
		string str;
		public FaqVC () : base ("FaqVC", null)
		{
			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
				ContentSizeForViewInPopover = new SizeF (320, 480);
		}
		~FaqVC()
		{
			Console.WriteLine ("~FaqVC");
		}
		public static FaqVC create_or_reuse(string title, string str)
		{
			FaqVC result = new FaqVC ();
			result.NavigationItem.Title = title;
			result.str = str;
			return result;
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();

			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = AppDelegate.app.dark_background_color;

			AppDelegate.create_table_background(View, 0, View.Bounds.Height, UIViewAutoresizing.FlexibleHeight);

			CGRect rr = View.Bounds;
			rr.Y += AppDelegate.ORANGE_LINE_HEIGHT;
			rr.Height -= AppDelegate.ORANGE_LINE_HEIGHT + AppDelegate.BOBO_HEIGHT;
			web_view = new UIWebView (rr);
			web_view.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			web_view.Opaque = false;
			web_view.BackgroundColor = UIColor.Clear;
			View.AddSubview (web_view);

			AppDelegate.create_table_cover(View, web_view, View.Bounds.Height);

			NSUrl rel_url = new NSUrl(NSBundle.MainBundle.BundlePath, true);
			web_view.LoadHtmlString (str, rel_url);
		}
	}
}

