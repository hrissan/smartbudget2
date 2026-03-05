using System;
using Foundation;
using UIKit;
using CoreGraphics;
using System.Drawing;

namespace SmartBudgetiOS
{
/*	[Register ("SectionHeader")]
	public class SectionHeader : UIView
	{
		public UILabel label;
		public SectionHeader (RectangleF frame) : base (frame)
		{
		}
		~SectionHeader()
		{
			Console.WriteLine ("~SectionHeader");
		}
		public static float default_height = 18;
		public static float categories_height = 20;
		private static UIFont section_font = UIFont.FromName("HelveticaNeue", 16);
		private static UIImage section_header_image = UIImage.FromBundle("new_design/section_header.png").CreateResizableImage(new UIEdgeInsets(0,0,0,0), UIImageResizingMode.Tile);
		public static SectionHeader create_or_get_header()
		{
			UIImageView img = new UIImageView (section_header_image);
			img.AutoresizingMask = UIViewAutoresizing.FlexibleRightMargin;
			SectionHeader sc = new SectionHeader (new RectangleF(0,0,320,18));
			img.Frame = new RectangleF (0, 0, 320, 21);
			img.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleBottomMargin;
			sc.AddSubview (img);
			RectangleF la = new RectangleF(4, 1, 320 - 8, 17);
			sc.label = new UILabel(la);
			sc.label.Font = section_font;
			sc.label.AdjustsFontSizeToFitWidth = true;
			sc.label.MinimumScaleFactor = 0.5f;
			//sc.label.MinimumFontSize = 10;
			sc.label.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleBottomMargin;
			sc.label.TextAlignment = UITextAlignment.Center;
			sc.label.TextColor = new UIColor(132/255f,95/255f,48/255f,1);
			sc.label.BackgroundColor = UIColor.Clear;
			sc.AddSubview (sc.label);
			return sc;
		}
	}*/

	public class SectionHeader2
	{
		public static readonly NSString Key = new NSString ("SectionHeader2");
		public static UITableViewHeaderFooterView deque_header(UITableView tableView, out UILabel label)
		{
			UITableViewHeaderFooterView hfv = tableView.DequeueReusableHeaderFooterView (Key);

			UIView[] svs = hfv.ContentView.Subviews;
			label = null;
			if (svs.Length == 0) {
				UIImageView img = new UIImageView (section_header_image);
				img.Frame = new CGRect (0, 0, hfv.ContentView.Bounds.Width, 21);
				img.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleBottomMargin;
				hfv.ContentView.AddSubview (img);

				CGRect la = new CGRect (4, 1, hfv.ContentView.Bounds.Width - 8, 17);
				label = new UILabel (la);
				label.Font = SectionHeader2.section_font;
				label.AdjustsFontSizeToFitWidth = true;
				label.MinimumScaleFactor = 0.5f;
//				label.MinimumFontSize = 10;
				label.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleBottomMargin;
				label.TextAlignment = UITextAlignment.Center;
				label.TextColor = AppDelegate.app.neutral_color;// AppDelegate.section_text_color;
				label.BackgroundColor = UIColor.Clear;
				hfv.ContentView.AddSubview (label);
			} else {
				foreach (var v in svs) {
					label = v as UILabel;
					if (label != null)
						break;
				}
			}
			return hfv;
		}
		public static nfloat default_height = 18;
		public static nfloat categories_height = 20;
		private static UIFont section_font = UIFont.FromName("HelveticaNeue", 16);
		private static UIImage section_header_image = UIImage.FromBundle("new_design/section_header.png").CreateResizableImage(new UIEdgeInsets(0,0,0,0), UIImageResizingMode.Tile);
//		public static SectionHeader2 create_or_get_header()
//		{
//			return sc;
//		}
	}
}

