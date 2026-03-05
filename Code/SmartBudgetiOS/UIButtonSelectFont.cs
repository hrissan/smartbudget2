using System;
using Foundation;
using UIKit;
using System.Drawing;

namespace SmartBudgetiOS
{
	[Register ("UIButtonSelectFont")]
	public class UIButtonSelectFont : UIButton, UIFontSelect.Delegate
	{
		private nfloat original_size;
		public UIButtonSelectFont (nfloat original_size, UIButtonType type) : base ()
		{
			this.original_size = original_size;
			UIFontSelect.AddDelegate(this);
		}
		public UIButtonSelectFont (IntPtr handle) : base (handle)
		{
		}
		public void on_font_changed(string font_name, float scale)
		{
			UIFont new_font = UIFont.FromName (font_name, (nfloat)Math.Floor(original_size * scale));
			TitleLabel.Font = new_font;
		}
		public override void AwakeFromNib ()
		{
			original_size = TitleLabel.Font.PointSize;
			UIFontSelect.AddDelegate(this);
		}
	}
}
