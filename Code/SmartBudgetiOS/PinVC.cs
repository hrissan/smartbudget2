using System;
using System.Drawing;
using Foundation;
using UIKit;
using CoreGraphics;
using SmartBudgetCommon;

namespace SmartBudgetiOS
{
	public partial class PinVC : UIViewController, Utility.KeyboardListener
	{
		//private NSObject keyboard_did_show_token;
		//private NSObject keyboard_did_hide_token;
		private bool remove_or_add_code;
		private UIView panelButtons;
		private UIButton btnHelp;
		//UIButton[] btnDigits;
		public PinVC (bool remove_or_add_code) : base ("PinVC", null)
		{
			this.remove_or_add_code = remove_or_add_code;
			NavigationItem.Title = i18n.get ("PinTitle");
			if (remove_or_add_code) {
				NavigationItem.LeftBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Cancel, (sender, e) => {
					Utility.dismiss_or_pop (this.NavigationController, true);
				});
			}
			Utility.add_keyboard_listener (this);
		}
		~PinVC()
		{
			Console.WriteLine ("~PinVC");
		}
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();

			if (!IsViewLoaded) {
				editPin1 = Utility.free_view (editPin1);
				editPin2 = Utility.free_view (editPin2);
				//Utility.free_token (ref keyboard_did_show_token);
				//Utility.free_token (ref keyboard_did_hide_token);
			}
			// Release any cached data, images, etc that aren't in use.
		}
		public void on_keyboard_show (CGRect endFrame)
		{
			if (!IsViewLoaded)
				return;
			endFrame = tableParagon.Superview.ConvertRectFromView(endFrame, null);
			CGRect rect = tableParagon.Frame;
			rect.Height = endFrame.Y - rect.Y;
			tablePlace.Frame = rect;
		}
		public void on_keyboard_hide ()
		{
			if (!IsViewLoaded)
				return;
			tablePlace.Frame = tableParagon.Frame;
		}
		partial void edit1Changed (UIKit.UITextField sender)
		{
			if( editPin1.Text.Length == 4 ){
				string pin = AppDelegate.app.pin_code;
				if( String.IsNullOrEmpty(pin) ){
					UIView.Animate(0.5, delegate {
						CGRect rr = slidingView.Frame;
						rr.X = -rr.Width/2;
						slidingView.Frame = rr;
					});
					editPin2.BecomeFirstResponder();
					return;
				}
				if( editPin1.Text != pin ){
					editPin1.Text = "";
					labelPin1.Text = i18n.get ("PinWrongText");
					return;
				}
				if( remove_or_add_code ) {
					AppDelegate.app.pin_code = "";
					AppDelegate.app.docs.send_anything_changed(null);
				}
				editPin1.ResignFirstResponder();
				AppDelegate.app.must_enter_initial_pincode = false;
				//					PresentingViewController.DismissViewController(true, null);
				Utility.dismiss_or_pop(NavigationController, true);
			}
		}
		partial void edit2Changed (UIKit.UITextField sender)
		{
			if( editPin2.Text.Length == 4 ){
				if( editPin1.Text != editPin2.Text ){
					labelPin1.Text = i18n.get ("PinNoMatchText");
					editPin1.Text = "";
					editPin2.Text = "";
					UIView.Animate(0.5, delegate {
						CGRect rr = slidingView.Frame;
						rr.X = 0;
						slidingView.Frame = rr;
					});
					editPin1.BecomeFirstResponder();
					return;
				}
				AppDelegate.app.pin_code = editPin1.Text;
				AppDelegate.app.docs.send_anything_changed(null);
				editPin2.ResignFirstResponder();
				Utility.dismiss_or_pop(NavigationController, true);
				//					PresentingViewController.DismissViewController(true, null);
			}
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			View.BackgroundColor = AppDelegate.app.dark_background_color;
			tablePlace.BackgroundColor = AppDelegate.app.dark_background_color;
			//AppDelegate.create_table_background(View, 0, tablePlace.Frame.Y, UIViewAutoresizing.FlexibleBottomMargin);
			//table = AppDelegate.create_table_and_background (tablePlace, 1, false);

			UIView back = AppDelegate.create_table_background(tablePlace, 0, tablePlace.Bounds.Height - AppDelegate.BUTTON_ROW_HEIGHT, UIViewAutoresizing.FlexibleHeight);
			AppDelegate.create_table_cover(tablePlace, back, tablePlace.Bounds.Height - AppDelegate.BUTTON_ROW_HEIGHT);

			/*WeakReference weak_this = new WeakReference(this);
			keyboard_did_show_token = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.DidShowNotification, (n) => {
				PinVC strong_this = weak_this.Target as PinVC;
				if( strong_this == null )
					return;
				RectangleF endFrame = (n.UserInfo[UIKeyboard.FrameEndUserInfoKey] as NSValue).RectangleFValue;
				endFrame = strong_this.tableParagon.Superview.ConvertRectFromView(endFrame, null);
				RectangleF rect = strong_this.tableParagon.Frame;
				rect.Height = endFrame.Y - rect.Y;
				strong_this.tablePlace.Frame = rect;
			});
			keyboard_did_hide_token = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.DidHideNotification, (n) => {
				PinVC strong_this = weak_this.Target as PinVC;
				if( strong_this == null )
					return;
				strong_this.tablePlace.Frame = strong_this.tableParagon.Frame;
			});*/
			panelButtons = BottomPanelView.create_bottom_panel (tablePlace);

			btnHelp = BottomPanelView.create_help_button( panelButtons, "help");

			btnHelp.TouchUpInside += (sender, e) => {
				LayoutForHelp lh = new LayoutForHelp(NavigationController, this.tablePlace.Frame.Height - AppDelegate.BUTTON_ROW_HEIGHT);
				lh.create_help_label(LayoutForHelp.LARGE_WIDTH, NavigationController.NavigationBar, 0, i18n.get (remove_or_add_code ? "HelpPin" : "HelpForgotPin"), LayoutForHelp.BubleType.BUTTON);
				lh.show ();
			};

			Utility.fix_rtl_view (imgSeparator);
			/*WeakReference weak_this = new WeakReference (this);
			editPin1.EditingChanged += (sender, e) => {
				PinVC strong_this = weak_this.Target as PinVC;
				if( strong_this == null )
					return;
				strong_this.edit1_changed();
			};
			editPin2.EditingChanged += (sender, e) => {
				PinVC strong_this = weak_this.Target as PinVC;
				if( strong_this == null )
					return;
				strong_this.edit2_changed();
			};*/
		}
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			CGRect rr = slidingView.Frame;
			rr.X = 0;
			slidingView.Frame = rr;
			editPin1.Text = "";
			editPin2.Text = "";
			labelPin1.Text = i18n.get ("PinEnterText");
			labelPin2.Text = i18n.get ("PinRepeatText");
			editPin1.BecomeFirstResponder();
		}
		//public override void ViewDidAppear(bool animated)
		//{
			//base.ViewDidAppear(animated);
			//editPin1.BecomeFirstResponder();
		//}
		public override void ViewWillDisappear (bool animated)
		{
			editPin1.ResignFirstResponder ();
			editPin2.ResignFirstResponder ();
			base.ViewWillDisappear (animated);
		}
		public override void ViewWillLayoutSubviews ()
		{
			base.ViewWillLayoutSubviews ();
			BottomPanelView.layout (panelButtons, btnHelp);
		}
	}
}

