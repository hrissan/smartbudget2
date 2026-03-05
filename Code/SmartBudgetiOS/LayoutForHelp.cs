using System;
using UIKit;
using System.Drawing;
using CoreGraphics;

namespace SmartBudgetiOS
{
	public class LayoutForHelp
	{
		public UIView dark_view;
		private nfloat header_height;
		public nfloat last_pos_from_top;
		public nfloat last_pos_from_bottom;
		public const float MIN_Y_FROM_VIEW_CENTER = 26;
		public const float MARGIN_X = 10;
		public const float MARGIN_Y = 10;
		public const float BORDER_X = 16;
		public const float BORDER_Y = 4;
		public const float MEDIUM_WIDTH = 192;
		public const float LARGE_WIDTH = 320 - 2*MARGIN_X;
		public Action on_close;
		public LayoutForHelp (UINavigationController nc, nfloat working_height)
		{
			CGRect nbr = nc.NavigationBar.ConvertRectToView (nc.NavigationBar.Bounds, nc.View);
			header_height = nbr.Y + nbr.Height + AppDelegate.ORANGE_LINE_HEIGHT;
			last_pos_from_top = header_height;
			last_pos_from_bottom = header_height + working_height;

			CGRect big_rect = nc.View.Bounds;
			dark_view = new UIView(big_rect);
			dark_view.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
			dark_view.BackgroundColor = new UIColor(0,0,0,0.5f);
			dark_view.UserInteractionEnabled = true;
			dark_view.Alpha = 0;
			//var weak_this = new WeakReference(this);
			UITapGestureRecognizer tap_recognizer = new UITapGestureRecognizer( (r)=>{
				//this.dark_view.RemoveGestureRecognizer(tap_recognizer);
				UIView.Animate(0.5, delegate {
					//var strong_this = weak_this.Target as LayoutForHelp;
					//if( strong_this != null)
					this.dark_view.Alpha = 0;
				}, delegate {
					//var strong_this = weak_this.Target as LayoutForHelp;
					//if( strong_this != null) {
					if( this.dark_view != null ) {
						this.dark_view.RemoveFromSuperview();
					}
					if( this.on_close != null )
						this.on_close.Invoke();
					this.dark_view = null;
					this.on_close = null;
					//}
				});
			});
			dark_view.AddGestureRecognizer(tap_recognizer);
			nc.View.AddSubview(dark_view);
		}
		~LayoutForHelp()
		{
			Console.WriteLine ("~LayoutForHelp");
		}
		private static UIImage help_buble = UIImage.FromBundle ("new_design/help_buble.png").StretchableImage (6, 6);
		private static UIImage tail_up = UIImage.FromBundle ("new_design/help_tail_up.png").StretchableImage (0, 6);
		private static UIImage tail_down = UIImage.FromBundle ("new_design/help_tail_down.png").StretchableImage (0, 6);
		private static UIImage tail_arrow_down = UIImage.FromBundle ("new_design/help_arrow_down.png");
		private static UIImage tail_hold = UIImage.FromBundle ("new_design/help_hold.png");
		private const float TAIL_WIDTH_2 = 3;

		public enum BubleType { BUTTON, HOLD, ARROW_DOWN, NO_TAILS };
		public nfloat view_y_to_dark_view_y(UIView attached_view, nfloat y)
		{
			CGPoint ar = attached_view.ConvertPointToView (new CGPoint(0, y), dark_view);
			return ar.Y + header_height;
		}
		public void create_tail(UIView attached_view, nfloat x_delta, UIView background_view)
		{
			CGPoint ar = attached_view.ConvertPointToView (new CGPoint(0, attached_view.Bounds.Height/2), dark_view);
			bool to_top = ar.Y < background_view.Center.Y;
			create_tail (attached_view, x_delta, background_view, to_top);
		}
		public void create_tail(UIView attached_view, nfloat x_delta, UIView background_view, bool to_top)
		{
			CGRect ar = attached_view.ConvertRectToView (attached_view.Bounds, dark_view);
			CGPoint ac = new CGPoint (ar.X + (nfloat)Math.Floor (ar.Width / 2) + x_delta, ar.Y + (nfloat)Math.Floor (ar.Height / 2));

			CGRect rr = background_view.Frame;
			const float CORNER_WIDTH = 10;
			if (ac.X < rr.X + CORNER_WIDTH) {
				rr.X = ac.X - CORNER_WIDTH;
				background_view.Frame = rr;
			}
			if (ac.X > rr.X + rr.Width - CORNER_WIDTH) {
				rr.X = ac.X - rr.Width + CORNER_WIDTH;
				background_view.Frame = rr;
			}

			UIImageView img_view;
			if (to_top) {
				img_view = new UIImageView (tail_up);
				CGRect tail_r = new CGRect (ac.X - TAIL_WIDTH_2, ac.Y - TAIL_WIDTH_2, TAIL_WIDTH_2 * 2, rr.Y - ac.Y + TAIL_WIDTH_2);
				img_view.Frame = tail_r;
			} else {
				img_view = new UIImageView (tail_down);
				CGRect tail_r = new CGRect (ac.X - TAIL_WIDTH_2, rr.Y + rr.Height, TAIL_WIDTH_2 * 2, ac.Y - rr.Y - rr.Height + TAIL_WIDTH_2);
				img_view.Frame = tail_r;
			}
			img_view.AutoresizingMask = background_view.AutoresizingMask;
			dark_view.InsertSubview (img_view, 0);
		}
		public UIView create_help_label(nfloat max_width, UIView attached_view, nfloat x_delta, string text, BubleType buble_type)
		{
			bool to_top = (attached_view.AutoresizingMask & UIViewAutoresizing.FlexibleTopMargin) == 0;
			return create_help_label (max_width, attached_view, x_delta, text, buble_type, to_top);
		}
		public UIView create_help_label(nfloat max_width, UIView attached_view, nfloat x_delta, string text, BubleType buble_type, bool to_top)
		{
			CGRect ar = attached_view.ConvertRectToView (attached_view.Bounds, dark_view);
			CGPoint ac = new CGPoint (ar.X + (nfloat)Math.Floor (ar.Width / 2) + x_delta, ar.Y + (nfloat)Math.Floor (ar.Height / 2));

			UILabel lab = new UILabelSelectFont (18, new CGRect(0,0,10,10));
			lab.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
			lab.BackgroundColor = UIColor.Clear;
			lab.TextColor = AppDelegate.app.neutral_color;
			lab.AdjustsFontSizeToFitWidth = true;
			lab.MinimumScaleFactor = 0.5f;
			//lab.MinimumFontSize = 10;
			lab.TextAlignment = UITextAlignment.Center;
			lab.LineBreakMode = UILineBreakMode.WordWrap;
			lab.Lines = 0;
			lab.Text = text;

			CGSize string_size = text.StringSize (lab.Font, new CGSize (max_width - 2 * BORDER_X, 320), lab.LineBreakMode);
			string_size.Height += 2 * BORDER_Y;
			CGSize original_string_size = string_size;
			nfloat width = string_size.Width + 2 * BORDER_X;
			nfloat width_2 = (float)Math.Floor (width / 2);

			CGRect rr;
			if( buble_type == BubleType.BUTTON || buble_type == BubleType.HOLD ) {
				nfloat mi = buble_type == BubleType.BUTTON ? MIN_Y_FROM_VIEW_CENTER : tail_hold.Size.Height;
				if (to_top) {
					nfloat bubble_top = (nfloat)Math.Max (ac.Y + mi, last_pos_from_top + MARGIN_Y);
					if (bubble_top + string_size.Height > last_pos_from_bottom - MARGIN_Y)
						string_size.Height = last_pos_from_bottom - MARGIN_Y - bubble_top;
					rr = new CGRect (ac.X - width_2, bubble_top, width, string_size.Height);
					last_pos_from_top = rr.Y + rr.Height;
				} else {
					nfloat bubble_bottom = (nfloat)Math.Min (ac.Y - mi, last_pos_from_bottom - MARGIN_Y);
					if (bubble_bottom - string_size.Height < last_pos_from_top + MARGIN_Y)
						string_size.Height = bubble_bottom - last_pos_from_top - MARGIN_Y;
					rr = new CGRect (ac.X - width_2, bubble_bottom - string_size.Height, width, string_size.Height);
					last_pos_from_bottom = rr.Y;
				}
			}else{
				nfloat top_limit = (nfloat)Math.Max (ar.Y + MARGIN_Y, last_pos_from_top + MARGIN_Y);
				nfloat down_limit = (nfloat)Math.Min (ar.Y + ar.Height - MARGIN_Y, last_pos_from_bottom - MARGIN_Y);
				if (buble_type == BubleType.ARROW_DOWN)
					down_limit -= tail_arrow_down.Size.Height;
				if (top_limit + string_size.Height > down_limit)
					string_size.Height = down_limit - top_limit;
				if (to_top) {
					rr = new CGRect (ac.X - width_2, top_limit, width, string_size.Height);
					last_pos_from_top = rr.Y + rr.Height;
				} else {
					rr = new CGRect (ac.X - width_2, down_limit - string_size.Height, width, string_size.Height);
					last_pos_from_bottom = rr.Y;
				}
			}
			if (original_string_size.Height != string_size.Height) {
				string fn = lab.Font.Name;
				nfloat fs = lab.Font.PointSize;
				nfloat min_fs = fs / 2;
				for (;fs >= min_fs; fs -= 1) {
					UIFont nf = UIFont.FromName (fn, fs);
					CGSize ss = text.StringSize (nf, new CGSize (max_width - 2 * BORDER_X, 320), lab.LineBreakMode);
					ss.Height += 2 * BORDER_Y;
					if (ss.Height < string_size.Height) {
						lab.Font = nf;
						break;
					}
				}
			}
			if (rr.X < MARGIN_X)
				rr.X = MARGIN_X;
			if (rr.X + rr.Width > dark_view.Bounds.Width - MARGIN_X)
				rr.X = dark_view.Bounds.Width - MARGIN_X - rr.Width;
			UIView background_view = new UIView(rr);
			background_view.BackgroundColor = UIColor.Clear;
			background_view.AutoresizingMask = to_top ? UIViewAutoresizing.FlexibleBottomMargin : UIViewAutoresizing.FlexibleTopMargin;

			UIImageView img_back_view = new UIImageView (help_buble);
			img_back_view.Frame = background_view.Bounds;
			img_back_view.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
			background_view.AddSubview (img_back_view);

			lab.Frame = new CGRect (BORDER_X, BORDER_Y, width - 2 * BORDER_X, string_size.Height - 2 * BORDER_Y);
			background_view.AddSubview (lab);

			dark_view.InsertSubview (background_view, 0);
			if( buble_type == BubleType.BUTTON ) {
				create_tail (attached_view, x_delta, background_view);
			}
			if( buble_type == BubleType.ARROW_DOWN ) {
				UIImageView img_view = new UIImageView (tail_arrow_down);
				nfloat arrow_wid_2 = (nfloat)Math.Floor (tail_arrow_down.Size.Width / 2);
				nfloat pos_x = (nfloat)Math.Floor(rr.X + rr.Width / 2);
				CGRect tail_r = new CGRect (pos_x - arrow_wid_2, rr.Y + rr.Height, tail_arrow_down.Size.Width, tail_arrow_down.Size.Height);
				img_view.Frame = tail_r;
				img_view.AutoresizingMask = background_view.AutoresizingMask;
				dark_view.InsertSubview (img_view, 0);

				img_view = new UIImageView (tail_up);
				nfloat tail_wid_2 = (nfloat)Math.Floor (tail_up.Size.Width / 2);
				tail_r = new CGRect (ac.X - tail_wid_2, rr.Y - tail_up.Size.Height, tail_up.Size.Width, tail_up.Size.Height);
				img_view.Frame = tail_r;
				img_view.AutoresizingMask = background_view.AutoresizingMask;
				dark_view.InsertSubview (img_view, 0);
			}
			if( buble_type == BubleType.HOLD ) {
				UIImageView img_view = new UIImageView (tail_hold);
				nfloat tail_wid_2 = (nfloat)Math.Floor (tail_hold.Size.Width / 2);
				CGRect tail_r = new CGRect (ac.X - tail_wid_2, rr.Y - tail_hold.Size.Height, tail_hold.Size.Width, tail_hold.Size.Height);
				img_view.Frame = tail_r;
				img_view.AutoresizingMask = background_view.AutoresizingMask;
				dark_view.InsertSubview (img_view, 0);
			}

			return background_view;
		}
		public void show()
		{
			UIView.Animate(0.5, delegate {
				dark_view.Alpha = 1;
			});
		}
	}
}

