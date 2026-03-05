using System;
using UIKit;
using QuickLook;
using Foundation;
using System.Drawing;
using CoreGraphics;
using System.Collections.Generic;
using ObjCRuntime;
using System.Globalization;

namespace SmartBudgetiOS
{
	public class Utility
	{
		/*public static void simulate_memory_warning()
		{
			Selector selector = new Selector ("_performMemoryWarning");
			IntPtr ha = UIApplication.SharedApplication.Handle;
			ObjCRuntime.Messaging.void_objc_msgSend (ha, selector.Handle);
		}*/
		public class ReuseVC<T> where T : UIViewController, new()
		{
			private List<T> vcs_for_reuse = new List<T>();
			public T create_or_reuse()
			{
				return new T ();
				/*T result = null;
				foreach (var vc in vcs_for_reuse) {
					if (vc.NavigationController != null && vc.NavigationController.ParentViewController == null) {
						vc.NavigationController.ViewControllers = new UIViewController[0];
						result = vc;
						break;
					}
					if (vc.NavigationController == null) {
						result = vc;
						break;
					}
				}
				if (result == null) {
					result = new T ();
					vcs_for_reuse.Add (result);
				}
				return result;*/
			}
		};
		public interface KeyboardListener
		{
			void on_keyboard_show (CGRect endFrame);
			void on_keyboard_hide ();
		};
		private static NSObject keyboard_did_show_token;
		private static NSObject keyboard_did_hide_token;
		private static List<WeakReference> keyboard_listeners = new List<WeakReference>();
		public static void on_keyboard_show(NSNotification n)
		{
			CGRect endFrame = (n.UserInfo[UIKeyboard.FrameEndUserInfoKey] as NSValue).RectangleFValue;

			foreach (var kl in keyboard_listeners) {
				KeyboardListener listener = kl.Target as KeyboardListener;
				if (listener != null)
					listener.on_keyboard_show (endFrame);
			}
		}
		public static void on_keyboard_hide(NSNotification n)
		{
			foreach (var kl in keyboard_listeners) {
				KeyboardListener listener = kl.Target as KeyboardListener;
				if (listener != null)
					listener.on_keyboard_hide ();
			}
		}
		public static void add_keyboard_listener(KeyboardListener listener)
		{
			if (keyboard_did_show_token == null) {
				keyboard_did_show_token = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.DidShowNotification, on_keyboard_show);
				keyboard_did_hide_token = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.DidHideNotification, on_keyboard_hide);
			}
			keyboard_listeners.RemoveAll (w => w.Target == null);
			keyboard_listeners.Add (new WeakReference(listener));
		}
		public static byte[] from_nsdata(NSData data)
		{
			byte[] dataBytes = new byte[data.Length];
			System.Runtime.InteropServices.Marshal.Copy(data.Bytes, dataBytes, 0, Convert.ToInt32(data.Length));
			return dataBytes;
		}
		public static string urlsafe_b64encode(byte[] data)
		{
			return Convert.ToBase64String (data).Replace ('+', '-').Replace ('/', '_');
		}
		public static byte[] urlsafe_b64decode(string str)
		{
			return Convert.FromBase64String(str.Replace('-','+').Replace('_','/'));
		}
		public static UIImage image_from_color(UIColor color)
		{
			RectangleF rect = new RectangleF (0, 0, 1, 1);
			UIGraphics.BeginImageContext (rect.Size);
			CGContext con = UIGraphics.GetCurrentContext ();
			con.SetBlendMode (CGBlendMode.Copy);
			con.SetFillColor (color.CGColor);
			con.FillRect(rect);
			UIImage result = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();
			return result;
		}
		public class PreviewDataSource : QLPreviewControllerDataSource {
			class PreviewItem : QLPreviewItem {
				private NSUrl url;
				private string title;
				public PreviewItem(NSUrl url, string title){
					this.url = url;
					this.title = title;
				}
				public override NSUrl ItemUrl {
					get {
						return url;
					}
				}
				public override string ItemTitle {
					get {
						return title;
					}
				}
			};
			private PreviewItem item;
			public PreviewDataSource(NSUrl url, string title) {
				this.item = new PreviewItem(url, title);
			}
			public override nint PreviewItemCount (QLPreviewController controller)
			{
				return 1;
			}
			public override IQLPreviewItem GetPreviewItem (QLPreviewController controller, nint index)
			{
				return item;
			}
		};
		public class UIScrollViewExtender : UIView {
			public UIScrollView sv;
			public UIScrollViewExtender (UIScrollView sv, CGRect rect) : base (rect)
			{
				this.sv = sv;
				//				UserInteractionEnabled = false;
				AutoresizingMask = sv.AutoresizingMask;
			}
			public override UIView HitTest (CGPoint point, UIEvent uievent)
			{
				if (this.PointInside (point, uievent)){
					UIView result = sv.HitTest (point, uievent);
					if( result != null)
						return result;
					return sv;
				}
				return null;
				//return sv.HitTest (point, uievent);
			}
		};
		//static private List<UINavigationController> slide_up_vcs = new List<UINavigationController>();
		static public void slide_up(UINavigationController main_nc, UIViewController vc, UIModalTransitionStyle trans = UIModalTransitionStyle.CoverVertical)
		{
			/*List<UINavigationController> new_slide_up_vcs = new List<UINavigationController> ();
			foreach(var svc in slide_up_vcs) {
				if (svc.PresentingViewController == null) {
					var avc = svc.ViewControllers;
					svc.ViewControllers = new UIViewController[0];
					foreach (var ssvc in avc) {
						ssvc.NavigationItem.LeftBarButtonItem = null;
						ssvc.NavigationItem.RightBarButtonItem = null;
						ssvc.NavigationItem.TitleView = null;
						//ssvc.Dispose ();
					}
					//svc.Dispose ();
				} else
					new_slide_up_vcs.Add (svc);
			}
			slide_up_vcs = new_slide_up_vcs;*/
			//GC.Collect ();
			//Console.WriteLine("GC.GetTotalMemory(true)={0} {1}", GC.GetTotalMemory(true), NSObject.IsNewRefcountEnabled ());// TODO - remove in release
			UINavigationController nc = new UINavigationController(vc);
			//slide_up_vcs.Add (nc);
			nc.ModalTransitionStyle = trans;
			if( UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad )
				nc.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
			//nc.NavigationBar.LayoutSubviews ();
			nc.View.LayoutSubviews ();
			//nc.NavigationBar.TintColor = AppDelegate.app.tint_color;
			main_nc.PresentViewController(nc, true, null);
		}
		static public void free_token(ref NSObject token)
		{
			if (token == null)
				return;
			token.Dispose ();
			token = null;
		}
		static public UITableView free_view(UITableView view)
		{
			if (view != null) {
				view.RemoveFromSuperview ();
				view.Source = null;
			}
			return null;
		}

		static public S free_view<S>(S view) where S : UIView
		{
			if (view != null)
				view.RemoveFromSuperview ();
			return null;
		}
		static public void push_or_present(UINavigationController nc, UIViewController vc, bool animated)
		{
			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) {
				slide_up(nc, vc);
			} else
				nc.PushViewController (vc, animated);
		}
		static public void dismiss_or_pop(UINavigationController nc, bool animated)
		{
			WeakReference weak_nc = new WeakReference (nc);
			if( nc.ViewControllers.Length > 1 )
				nc.PopViewController(animated);
			else
				nc.PresentingViewController.DismissViewController(animated, delegate {
					UINavigationController strong_nc = weak_nc.Target as UINavigationController;
					if( strong_nc == null )
						return;
					foreach (var ssvc in strong_nc.ViewControllers) {
						ssvc.NavigationItem.LeftBarButtonItem = null;
						ssvc.NavigationItem.RightBarButtonItem = null;
						ssvc.NavigationItem.TitleView = null;
					}	
					strong_nc.ViewControllers = new UIViewController[0];
				});
		}
		static public void replace_last_controllers(UINavigationController nc, int count, UIViewController c, bool animated)
		{
			List<UIViewController> vcs = new List<UIViewController>( nc.ViewControllers );
			vcs.RemoveRange (vcs.Count - count, count);
			vcs.Add (c);
			nc.SetViewControllers (vcs.ToArray(), animated);
		}
		static public void play_transition(UIView view, UIViewAnimationTransition trans)
		{
			UIView.BeginAnimations(null);
			UIView.SetAnimationDuration(0.5);
			UIView.SetAnimationTransition(trans, view, true);
			UIView.CommitAnimations();
		}
		static private UIActionSheet last_action_sheet = null;
//		static private UIView last_action_sheet_origin = null;
//		static private UIView last_action_sheet_view = null;
		static public void show_action_sheet(UIActionSheet ash, UIView view, UIView origin)
		{
			last_action_sheet = ash;
//			last_action_sheet_origin = origin;
//			last_action_sheet_view = view;
			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) {
				CGRect rr = origin.ConvertRectToView(origin.Bounds, view);
				ash.ShowFrom(rr, view, true);
			}else
				ash.ShowInView (view);
		}
		static public void show_action_sheet(UIActionSheet ash, UIView view)
		{
			last_action_sheet = ash;
			ash.ShowInView (view);
		}
		static private UIPopoverController last_popover = null;
		static private UIView last_popover_origin = null;
		static public void show_popover_or_dialog(UINavigationController nc, UIViewController vc, UIView origin)
		{
			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) {
				last_popover_origin = origin;
				UINavigationController nc2 = new UINavigationController (vc);
				last_popover = new UIPopoverController (nc2);
				last_popover.DidDismiss += (sender2, e2) => {
					last_popover = null;
					last_popover_origin = null;
				};
				last_popover.PresentFromRect(last_popover_origin.Bounds, last_popover_origin, UIPopoverArrowDirection.Any, true);
			}else{
				Utility.slide_up(nc, vc);
			}
		}
		private static UIPopoverController splitview_popover = null;
		static public void remember_splitview_popover(UIPopoverController pc)
		{
			splitview_popover = pc;
		}
		static public void reshow_popovers_and_action_sheets()
		{
			if( last_popover != null )
				last_popover.PresentFromRect(last_popover_origin.Bounds, last_popover_origin, UIPopoverArrowDirection.Any, false);
			if( last_action_sheet != null ) {
				last_action_sheet.DismissWithClickedButtonIndex (-1, false);
				last_action_sheet = null;
//				last_action_sheet_origin = null;
//				last_action_sheet_view = null;
			}
		}
		static public void dismiss_popovers_and_action_sheets(bool animated)
		{
			if (splitview_popover != null)
				splitview_popover.Dismiss (animated);

			if( last_popover != null )
				last_popover.Dismiss (animated);
			last_popover = null;
			last_popover_origin = null;
			if (last_action_sheet != null)
				last_action_sheet.DismissWithClickedButtonIndex (-1, animated);
			last_action_sheet = null;
//			last_action_sheet_origin = null;
//			last_action_sheet_view = null;
		}
		public static string get_bundle_version(){
			return (NSString)NSBundle.MainBundle.InfoDictionary ["CFBundleVersion"];
		}
		public static string get_system_version(){
			return String.Format ("{0} ({1})", UIDevice.CurrentDevice.SystemVersion, UIDevice.CurrentDevice.Model);
		}
		public static bool RTL = false;
		public static void fix_rtl_view(UIView view)
		{
			if (!RTL)
				return;
			UIViewAutoresizing mask = view.AutoresizingMask;
			bool left = (mask & UIViewAutoresizing.FlexibleLeftMargin) != UIViewAutoresizing.None;
			bool right = (mask & UIViewAutoresizing.FlexibleRightMargin) != UIViewAutoresizing.None;
			mask &= ~(UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin);
			if( left )
				mask |= UIViewAutoresizing.FlexibleRightMargin;
			if( right )
				mask |= UIViewAutoresizing.FlexibleLeftMargin;
			view.AutoresizingMask = mask;
			CGRect fr = view.Frame;
			CGRect pbo = view.Superview.Bounds;
			nfloat lm = fr.X;
			nfloat rm = pbo.Width - (fr.X + fr.Width);
			fr.X = rm;
			fr.Width = pbo.Width - lm - rm;
			view.Frame = fr;
		}
		public static void fix_rtl_label(UILabel label)
		{
			if (!RTL)
				return;
			fix_rtl_view (label);
			switch (label.TextAlignment) {
				case UITextAlignment.Left:
					label.TextAlignment = UITextAlignment.Right;
				break;
				case UITextAlignment.Right:
					label.TextAlignment = UITextAlignment.Left;
				break;
			}
		}
		public static void fix_rtl_textfield(UITextField tf)
		{
			if (!RTL)
				return;
			fix_rtl_view (tf);
			/*switch (tf.TextAlignment) {
				case UITextAlignment.Left:
					tf.TextAlignment = UITextAlignment.Right;
				break;
				case UITextAlignment.Right:
					tf.TextAlignment = UITextAlignment.Left;
				break;
			}*/
		}
/*		public static NSDecimalNumber from_decimal(decimal val)
		{
			int[] Bits = decimal.GetBits(val);
			long mantissa = ((long)Bits [1] << 32) + Bits [0];
			int exponent = (Bits [3] & 0x00FF0000) >> 16;
			bool neg = (Bits [3] & 0x80000000) != 0;

			NSDecimalNumber result = new NSDecimalNumber (mantissa,(short)-exponent, neg);
			string test = result.ToString ();
			Console.WriteLine("from_decimal {0} {1}", val.ToString(CultureInfo.InvariantCulture), test);
			return result;
		}
		public static NSDecimalNumber from_decimal(decimal val)
		{
			int[] Bits = decimal.GetBits(val);
			long mantissa = ((long)Bits [1] << 32) + Bits [0];
			int exponent = (Bits [3] & 0x00FF0000) >> 16;
			bool neg = (Bits [3] & 0x80000000) != 0;

			NSDecimalNumber result = new NSDecimalNumber (mantissa,(short)-exponent, neg);
			string test = result.ToString ();
			Console.WriteLine("from_decimal {0} {1}", val.ToString(CultureInfo.InvariantCulture), test);
			return result;
		}*/
	}
}

