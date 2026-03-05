using System;
using Foundation;
using UIKit;
using CoreGraphics;
using System.Drawing;

namespace SmartBudgetiOS
{
//	[Register ("DrawArea")]
	public class DrawArea : UIView
	{
		public int brush_index;
		//private	bool touch_down;
		private CGPoint touch_down_pos;
		private byte[] contextBuffer;
		private CGBitmapContext context;
		private CGRect image_rect;
		private static UIImage[] brushes = new[] { UIImage.FromBundle("brush_marker.png"), UIImage.FromBundle("brush_eraser.png") };
		private static UIImage circle_eraser_image = UIImage.FromBundle("circle_eraser.png");
		//private Action<DrawArea> save_action;
		private UIImageView eraser_circle;
		private UIImageView output_image;

		public delegate void DrawAreaChanged (DrawArea sender, EventArgs e);
		public event DrawAreaChanged changed;

		public DrawArea (IntPtr handle) : base (handle)
		{
		}
		public DrawArea(CGRect frame, UIImageView output_image):base(frame)
		{
			this.output_image = output_image;
			UserInteractionEnabled = true;
			BackgroundColor = UIColor.Clear;
		}
		~DrawArea()
		{
			Console.WriteLine ("~DrawArea");
		}
		private void copyImage()
		{
			if (output_image == null)
				return;
			using (CGImage cgImage = context.ToImage ()) {
				if (cgImage.Handle.ToInt32() != 0) {
					UIImage newImage = new UIImage (cgImage);
					output_image.Image = newImage;
				}
			}
		}
		private void prepareContext()
		{
			if( context != null )
				return;
			eraser_circle = new UIImageView (circle_eraser_image);
			eraser_circle.Frame = new RectangleF (0, 0, 64, 64);
			eraser_circle.Hidden = true;
			this.Superview.Add (eraser_circle);

//			brushes = ;
			int wi = 144;
			int he = 144;
			image_rect = new RectangleF (0, 0, wi, he);

			using(CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB())
			{
				//IntPtr data = Marshal.AllocHGlobal(height * width * 4);
				//Marshal.FreeHGlobal(data);
				contextBuffer = new byte[wi*he*4];
				context = new CGBitmapContext (contextBuffer, wi, he, 8, wi * 4, colorSpace, CGImageAlphaInfo.PremultipliedLast);
				context.SetBlendMode (CGBlendMode.Clear);
				context.SetFillColor (1, 1, 1, 1);
				context.FillRect (image_rect);
				context.TranslateCTM (0, image_rect.Height);
					context.ScaleCTM (1f, -1f);
			}
		}

		private void clearImage()
		{
			prepareContext ();
			context.SetBlendMode (CGBlendMode.Clear);
			context.SetFillColor (1, 1, 1, 0);
			context.FillRect (image_rect);

			copyImage ();
		}

		public void revertToImage(UIImage img)
		{
			prepareContext ();
			UIGraphics.PushContext(context);

			img.Draw (PointF.Empty, CGBlendMode.Copy, 1);

			UIGraphics.PopContext();
			copyImage();
		}

		private void clearAtPoint(CGPoint p)
		{
			CGSize si = new CGSize (20f, 20f);
			context.SetStrokeColor (UIColor.Clear.CGColor);
			context.SetFillColor (UIColor.Clear.CGColor);
			context.AddEllipseInRect(new CGRect(p.X - si.Width/2, p.Y - si.Height/2,si.Width,si.Height));
			context.ClosePath ();
			context.DrawPath (CGPathDrawingMode.FillStroke);
		}
		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			prepareContext ();
			//touch_down = true;

			UITouch touch = (UITouch)touches.AnyObject;
			touch_down_pos = touch.LocationInView(this);
			eraser_circle.Hidden = (brush_index == 0);
			CGRect circle_fr = eraser_circle.Frame;
			circle_fr.X = Frame.X + touch_down_pos.X - circle_fr.Width / 2;
			circle_fr.Y = Frame.Y + touch_down_pos.Y - circle_fr.Height / 2;
			eraser_circle.Frame = circle_fr;
			touch_down_pos.X *= image_rect.Width / Frame.Width;
			touch_down_pos.Y *= image_rect.Height / Frame.Height;

			context.SetBlendMode(brush_index == 0 ? CGBlendMode.Normal : CGBlendMode.Clear);
			UIGraphics.PushContext(context);
			UIImage br = brushes[brush_index];
			if( brush_index == 0 )
			{
				br.Draw (new CGPoint(touch_down_pos.X - br.Size.Width/2,touch_down_pos.Y - br.Size.Height/2));
			}
			else
				clearAtPoint(touch_down_pos);
			UIGraphics.PopContext();
			copyImage ();
		}
		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			CGPoint old_pos = touch_down_pos;

			prepareContext ();
			UITouch touch = (UITouch)touches.AnyObject;
			touch_down_pos = touch.LocationInView(this);
			eraser_circle.Hidden = (brush_index == 0);
			CGRect circle_fr = eraser_circle.Frame;
			circle_fr.X = Frame.X + touch_down_pos.X - circle_fr.Width / 2;
			circle_fr.Y = Frame.Y + touch_down_pos.Y - circle_fr.Height / 2;
			eraser_circle.Frame = circle_fr;
			touch_down_pos.X *= image_rect.Width / Frame.Width;
			touch_down_pos.Y *= image_rect.Height / Frame.Height;

			nfloat dx = old_pos.X - touch_down_pos.X;
			nfloat dy = old_pos.Y - touch_down_pos.Y;
			nfloat len = (nfloat)Math.Sqrt (dx*dx + dy*dy);
			if (len == 0)
				return;
			context.SetBlendMode(brush_index == 0 ? CGBlendMode.Normal : CGBlendMode.Clear);
			UIGraphics.PushContext(context);
			UIImage br = brushes[brush_index];

			int step = Math.Max (1, (int)(br.Size.Width / 6));
			for (int i = 0; i < len; i += step) {
				CGPoint p = new CGPoint (touch_down_pos.X + i*dx/len, touch_down_pos.Y + i*dy/len);
				if( brush_index == 0 )
					br.Draw (new CGPoint(p.X - br.Size.Width/2,p.Y - br.Size.Height/2));
				else
					clearAtPoint(p);
			}
			UIGraphics.PopContext ();
			copyImage ();
		}
		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			//touch_down = false;
			eraser_circle.Hidden = true;
			changed.Invoke(this, EventArgs.Empty);
		}
		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			//touch_down = false;
			eraser_circle.Hidden = true;
			changed.Invoke(this, EventArgs.Empty);
		}
	}
}

