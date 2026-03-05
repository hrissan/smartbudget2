using System;
using UIKit;
using CoreGraphics;
using System.Drawing;
using System.Collections.Generic;

namespace SmartBudgetiOS
{
	public static class BottomPanelView
	{
		public const int BUTTON_WIDTH = 80;
		public const int PANEL_HEIGHT = 58;
		public const int HELP_WIDTH = 44;
/*		UIButton btnHelp;
		public List<UIButton> btns = new List<UIButton>();
		public BottomPanelView (UIView parent, float Y, float Height)
		{
			BackgroundColor = UIColor.Clear;
			RectangleF rr = parent.Bounds;
			rr.Y = Y;
			rr.Height = Height;
			Frame = rr;
			AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin;
			parent.AddSubview (this);
		}
		~BottomPanelView()
		{
			Console.WriteLine ("~BottomPanelView");
		}
		public BottomPanelView (UIView parent):this(parent, parent.Bounds.Height - PANEL_HEIGHT, PANEL_HEIGHT)
		{
		}
		public UIButton create_help_button (string image_prefix)
		{
			UIButton result = create_bottom_button_rc (this, image_prefix, HELP_WIDTH);
			btnHelp = result;
			return result;
		}
		public UIButton create_bottom_button (string image_prefix)
		{
			UIButton result = create_bottom_button_rc (this, image_prefix, BUTTON_WIDTH);
			btns.Add( result );
			return result;
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			int left = 0;
			int right = 0;
			if (btnHelp != null) {
				RectangleF rr = btnHelp.Frame;
				rr.X = 0;
				rr.Width = HELP_WIDTH;
				btnHelp.Frame = rr;
				left = HELP_WIDTH;
				right = HELP_WIDTH;
			}
			int iwid = (int)Bounds.Width;
			int req_wid = btns.Count * BUTTON_WIDTH;
			if (iwid - left - right < req_wid) {
				right = iwid - left - req_wid;
				if (right < 0)
					right = 0;
			}
			iwid -= left + right;
			if (iwid > req_wid)
				iwid = (int)req_wid;
			int start = (int)( left + (Bounds.Width - left - right - iwid) / 2 );
			for (int i = 0; i != btns.Count; ++i) {
				RectangleF rr = btns [i].Frame;
				rr.X = start + iwid * i / btns.Count;
				rr.Width = start + iwid * (i + 1) / btns.Count - rr.X;
				btns [i].Frame = rr;
			}
		}*/
		static public UIButton create_bottom_button_rc(UIView view, string image_prefix, float width)
		{
			UIButton btn = new UIButton (UIButtonType.Custom);
			btn.Frame = new RectangleF(0, 0, width, PANEL_HEIGHT);
			btn.AutoresizingMask = UIViewAutoresizing.FlexibleBottomMargin | UIViewAutoresizing.FlexibleRightMargin;
			btn.SetImage (UIImage.FromBundle("new_design/bb_" + image_prefix + "_n.png"), UIControlState.Normal);
			btn.SetImage (UIImage.FromBundle("new_design/bb_" + image_prefix + "_s.png"), UIControlState.Highlighted);
			btn.SetImage (UIImage.FromBundle("new_design/bb_" + image_prefix + "_d.png"), UIControlState.Disabled);
			//btn.BackgroundColor = UIColor.Cyan;
			//			btn.SetBackgroundImage(UIImage.FromBundle("b4_background_high.png"), UIControlState.Highlighted);
			/*if (title != null) {
				int label_hei = 13;
				btn.ContentEdgeInsets = new UIEdgeInsets (0, 0, 10, 0);
				UILabel lab = new UILabel (new RectangleF(0, rr.Height - label_hei, rr.Width, label_hei));
				lab.Font = UIFont.SystemFontOfSize (11);
				lab.BackgroundColor = UIColor.Clear;
				lab.TextColor = app.gray_text_color;
				lab.MinimumFontSize = 8;
				lab.TextAlignment = UITextAlignment.Center;
				lab.LineBreakMode = UILineBreakMode.Clip;
				lab.Text = title;
				btn.AddSubview (lab);
			}*/
			view.Add (btn);
			return btn;
		}
		static public UIView create_bottom_panel(UIView parent, nfloat Y, nfloat Height)
		{
			CGRect rr = parent.Bounds;
			rr.Y = Y;
			rr.Height = Height;
			UIView result = new UIView (rr);
			result.BackgroundColor = UIColor.Clear;
			result.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin;
			parent.AddSubview (result);
			return result;
		}
		static public UIView create_bottom_panel(UIView parent) 
		{
			return create_bottom_panel (parent, parent.Bounds.Height - PANEL_HEIGHT, PANEL_HEIGHT);
		}
		static public UIButton create_help_button (UIView bottom_panel, string image_prefix)
		{
			UIButton result = create_bottom_button_rc (bottom_panel, image_prefix, HELP_WIDTH);
			//btnHelp = result;
			return result;
		}
		static public UIButton create_bottom_button (UIView bottom_panel, string image_prefix)
		{
			UIButton result = create_bottom_button_rc (bottom_panel, image_prefix, BUTTON_WIDTH);
			//btns.Add( result );
			return result;
		}
		static public void layout (UIView bottom_panel, UIButton btnHelp)
		{
			int left = 0;
			int right = 0;
			if (btnHelp != null) {
				CGRect rr = btnHelp.Frame;
				rr.X = 0;
				rr.Width = HELP_WIDTH;
				btnHelp.Frame = rr;
				left = HELP_WIDTH;
				right = HELP_WIDTH;
			}
			int iwid = (int)bottom_panel.Bounds.Width;
			UIView[] subviews = bottom_panel.Subviews;
			int count = subviews.Length - (btnHelp != null ? 1 : 0);
			int req_wid = count * BUTTON_WIDTH;
			if (iwid - left - right < req_wid) {
				right = iwid - left - req_wid;
				if (right < 0)
					right = 0;
			}
			iwid -= left + right;
			if (iwid > req_wid)
				iwid = (int)req_wid;
			int start = (int)( left + (bottom_panel.Bounds.Width - left - right - iwid) / 2 );
			int real_i = 0;
			for (int i = 0; i != subviews.Length; ++i) {
				if (subviews [i] == btnHelp) {
					continue;
				}
				CGRect rr = subviews [i].Frame;
				rr.X = start + iwid * real_i / count;
				rr.Width = start + iwid * (real_i + 1) / count - rr.X;
				subviews [i].Frame = rr;

				real_i += 1;
			}
		}

	}
}

