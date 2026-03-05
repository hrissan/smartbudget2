using System;
using Foundation;
using UIKit;
using CoreGraphics;
using System.Drawing;

namespace SmartBudgetiOS
{
	[Register ("TitleButton")]
	public class TitleButton : UIView
	{
		public UIButton button;
		private CGSize content_width;
		public nfloat get_attention_delta()
		{
			CGSize imgs = new CGSize();
			UIImage img = button.ImageForState (UIControlState.Normal);
			if (img != null)
				imgs = img.Size;
			nfloat result_width = content_width.Width;
			if (this.Frame.Width < content_width.Width)
				result_width = this.Frame.Width;
			return (nfloat)Math.Floor ((result_width - imgs.Width) / 2);
		}
		public TitleButton (CGRect frame) : base (frame)
		{
		}
		public void set_text_image(string text, UIImage image)
		{
			content_width = text.StringSize (button.TitleLabel.Font);
			if (image != null)
				content_width.Width += image.Size.Width;
			CGRect rr = this.Frame;
			rr.Width = content_width.Width;
			if (rr.Width > 320)
				rr.Width = 320;
			if( rr.Width < 160 )
				rr.Width = 160;
			this.Frame = rr;
			button.SetTitle (text, UIControlState.Normal);
			button.SetImage(image, UIControlState.Normal);
		}
		public static TitleButton create_title_button()
		{
			UIImage hig = AppDelegate.Gray_button_selected();

			TitleButton sc = new TitleButton (new CGRect(0,0,160,44));
			sc.BackgroundColor = UIColor.Clear;

			sc.button = new UIButton (UIButtonType.Custom);
			sc.button.Frame = sc.Bounds;
			sc.button.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			sc.button.SetBackgroundImage(hig, UIControlState.Highlighted);
			sc.button.TitleLabel.TextAlignment = UITextAlignment.Center;
			sc.button.TitleLabel.Font = UIFont.BoldSystemFontOfSize (20);
			sc.button.TitleLabel.ShadowOffset = new SizeF (0, -1);
			sc.button.TitleLabel.ShadowColor = new UIColor (0, 0, 0, 0.5f);
			sc.button.ImageEdgeInsets = new UIEdgeInsets (0, 0, 0, 4);

			sc.AddSubview (sc.button);
			return sc;
		}
	}
}

