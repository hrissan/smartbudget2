using System;
using System.Drawing;
using Foundation;
using UIKit;
using SmartBudgetCommon;
using System.Collections.Generic;
using CoreGraphics;
using System.IO;
using System.Text;

namespace SmartBudgetiOS
{
	public partial class SettingsVC : UIViewController
	{
		UIButton btnHelp;
		UIButton btnReview;
		UIButton btnPriceTag;
		UIBarButtonItem button_done;
		UIView panelButtons;
		bool limit_mode;
		public SettingsVC () : base ("SettingsVC", null)
		{
			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
				ContentSizeForViewInPopover = new SizeF (320, 480);
			button_done = new UIBarButtonItem(UIBarButtonSystemItem.Done, (sender, e) => {
				Utility.dismiss_or_pop(this.NavigationController, true);
				//panelButtons.RemoveFromSuperview();
				//panelButtons.btns.Clear();
				//panelButtons = null;
				//this.NavigationController.PresentingViewController.DismissViewController(true, null);
			});
			NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Compose, (sender, e) => {
				UIAlertView av2 = new UIAlertView (i18n.get ("WriteEMailTitle"), i18n.get ("WriteEMailText"), null, i18n.get ("Cancel"), i18n.get ("OK"));
				av2.Clicked += (sender2, e2) => {
					if( e2.ButtonIndex != av2.CancelButtonIndex )
						AppDelegate.create_email(this.NavigationController, i18n.get ("EMailQuestionSubject"), i18n.get ("EMailQuestionBody"), null, "", true);
				};
				av2.Show ();
			});
			NavigationItem.RightBarButtonItem.SetBackgroundImage (UIImage.FromBundle ("new_design/nav_plus_n.png").StretchableImage (14, 0), UIControlState.Normal, UIBarMetrics.Default); // iOS 8+
			//AppDelegate.app.docs.anything_changed += (docs, e) => {
			//	anything_changed();
			//};
		}
		void construct(bool limit_mode)
		{
			this.limit_mode = limit_mode;
			NavigationItem.Title = i18n.get(limit_mode ? "LimitTitle" : "AboutTitle");
			NavigationItem.LeftBarButtonItem = (limit_mode || UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Pad) ? button_done : null;
		}
		~SettingsVC()
		{
			Console.WriteLine("~SettingsVC");
		}
		private static Utility.ReuseVC<SettingsVC> reuse = new Utility.ReuseVC<SettingsVC> ();
		public static SettingsVC create_or_reuse(bool limit_mode)
		{
			SettingsVC result = reuse.create_or_reuse();
			result.construct(limit_mode);
			return result;
		}
		private static UIImage tag_normal = UIImage.FromBundle ("new_design/tag_normal.png");
		private static UIImage tag_sale = UIImage.FromBundle ("new_design/tag_sale.png");

		private void anything_changed (Documents doc, Documents.DocumentChangeEventArgs e)
		{
			anything_changed ();
		}
		private void anything_changed()
		{
			labelLimit.Hidden = !limit_mode;

			table.ReloadData ();
			bool fu = AppDelegate.app.docs.full_version;
			bool pp = AppDelegate.app.processing_payment;
			bool pr = AppDelegate.app.processing_restore;
			btnPurchase.Hidden = !(!fu && !pp && !pr);
			btnRestore.Hidden = btnPurchase.Hidden;
			activityPurchasing.Hidden = !(pp || pr);
			labelStatus.Hidden = !btnPurchase.Hidden;
			labelStatus.Text = pp ? i18n.get("PurchaseStatusPurchasing") : pr ? i18n.get("PurchaseStatusRestoring") : i18n.get("PurchaseStatusFullVersion");
			btnReview.Hidden = !fu;
			string price = AppDelegate.app.get_full_version_price(AppDelegate.app.ok_for_sale);
			btnPriceTag.Hidden = btnPurchase.Hidden;
			btnPriceTag.SetBackgroundImage (AppDelegate.app.ok_for_sale ? tag_sale : tag_normal, UIControlState.Normal);
			btnPriceTag.SetTitle(price != null ? price : "", UIControlState.Normal);
		}
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();

			if (!IsViewLoaded) {
				Console.WriteLine("SettingsVC.DidReceiveMemoryWarning freeing stuff");
				table = Utility.free_view (table);
			}
//			if (!IsViewLoaded) {
//				Console.WriteLine("SettingsVC.DidReceiveMemoryWarning !IsViewLoaded");
//				table = Utility.free_view (table);
//			}else
//				Console.WriteLine("SettingsVC.DidReceiveMemoryWarning IsViewLoaded");
			// Release any cached data, images, etc that aren't in use.
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = AppDelegate.app.dark_background_color;

			table = AppDelegate.create_table_and_background (View, 3);
			panelButtons = BottomPanelView.create_bottom_panel (View);

			btnHelp = BottomPanelView.create_help_button(panelButtons, "help");

			btnPurchase = AppDelegate.create_flat_bottom_button (View, i18n.get ("Purchase20ButtonTitle"), 2);
			btnRestore = AppDelegate.create_flat_bottom_button (View, i18n.get ("RestoreButtonTitle"), 1);
			btnReview = AppDelegate.create_flat_bottom_button (View, i18n.get ("WriteReviewTitle"), 1);

			{
				//const float TAG_SHIFT = 4;
				btnPurchase.ContentEdgeInsets = new UIEdgeInsets (0, 4, 0, PRICE_TAG_SHIFT);
				btnPriceTag = new UIButton (UIButtonType.Custom);
				btnPriceTag.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin | UIViewAutoresizing.FlexibleRightMargin;
				btnPriceTag.SetTitleColor (AppDelegate.app.dark_background_color, UIControlState.Normal);
				btnPriceTag.TitleLabel.AdjustsFontSizeToFitWidth = true;
				btnPriceTag.TitleLabel.MinimumScaleFactor = 0.5f;
				//btnPriceTag.TitleLabel.MinimumFontSize = 10;
				btnPriceTag.ContentEdgeInsets = new UIEdgeInsets (0, 10, 0, 0);
				//RectangleF rr = btnPurchase.Frame;
				//btnPriceTag.Frame = new RectangleF (rr.X + rr.Width - tag_normal.Size.Width + TAG_SHIFT, btnPurchase.Center.Y - tag_normal.Size.Height/2, tag_normal.Size.Width, tag_normal.Size.Height);
				btnPriceTag.Frame = new CGRect (0, btnPurchase.Center.Y - tag_normal.Size.Height/2, tag_normal.Size.Width, tag_normal.Size.Height);
				View.Add (btnPriceTag);

				btnPriceTag.TouchUpInside += (sender, e) => {
					AppDelegate.app.startPurchaseFullVersion(limit_mode ? "Limit" : "Settings");
					//this.show_help();
				};
			}	

			CGPoint ce = labelStatus.Center;
			ce.Y = btnPurchase.Center.Y;
			labelStatus.Center = ce;
			labelStatus.TextColor = AppDelegate.app.white_text_color;
			//ce = activityPurchasing.Center;
			//ce.Y = labelStatus.Center.Y;
			//activityPurchasing.Center = ce;
			labelLimit.Text = i18n.get ("LimitText");

			btnHelp.TouchUpInside += (sender, e) => {
				this.show_help();
			};
			btnPurchase.TouchUpInside += (sender, e) => {
				AppDelegate.app.startPurchaseFullVersion(limit_mode ? "Limit" : "Settings");
			};
			btnRestore.TouchUpInside += (sender, e) => {
				AppDelegate.app.startRestoreFullVersion(limit_mode ? "Limit" : "Settings");
			};
			btnReview.TouchUpInside += (sender, e) => {
				UIAlertView av2 = new UIAlertView (i18n.get ("WriteReviewTitle"), "", null, i18n.get ("Cancel"), i18n.get ("OK")); // i18n.get ("WriteReviewText")
				av2.Clicked += (sender2, e2) => {
					if( e2.ButtonIndex != av2.CancelButtonIndex ) {
						FlurryAnalytics.Flurry.LogEvent("WriteReview");
						UIApplication.SharedApplication.OpenUrl(new NSUrl(AppDelegate.itunes_link));
					}
				};
				av2.Show ();
			};
			table.Source = new TableSource(this);
			//anything_changed ();
			//AppDelegate.app.docs.anything_changed += (docs, e) => {
			//	anything_changed ();
			//	table.ReloadData();
			//};
		}
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			AppDelegate.app.docs.anything_changed += anything_changed;
			anything_changed ();
		}
		public override void ViewWillDisappear (bool animated)
		{
			AppDelegate.app.docs.anything_changed -= anything_changed;
			base.ViewWillDisappear (animated);
		}
		const float PRICE_TAG_SHIFT = 40;
		public override void ViewWillLayoutSubviews ()
		{
			base.ViewWillLayoutSubviews ();
			BottomPanelView.layout (panelButtons, btnHelp);
			CGRect rr = btnPriceTag.Frame;
			rr.X = btnPurchase.Frame.X + btnPurchase.Frame.Width - PRICE_TAG_SHIFT;
			btnPriceTag.Frame = rr;
		}
		private void show_help()
		{
			LayoutForHelp lh = new LayoutForHelp(NavigationController, table.Frame.Height);
			// From top
			lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, NavigationController.NavigationBar, NavigationController.NavigationBar.Bounds.Width/2 - 24, i18n.get ("HelpWriteUs"), LayoutForHelp.BubleType.BUTTON);

			// From bottom
			bool fu = AppDelegate.app.docs.full_version;
			if( fu ) {
				lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, btnReview, 100, i18n.get("HelpWriteReview"), LayoutForHelp.BubleType.BUTTON);
				lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, btnPurchase, -100, i18n.get ("HelpFullVersion"), LayoutForHelp.BubleType.BUTTON);
			}else{
				//string price = AppDelegate.app.get_full_version_price(AppDelegate.app.ok_for_sale);
				UIButton target_button = btnPriceTag;
				if( AppDelegate.app.ok_for_sale )
					lh.create_help_label(LayoutForHelp.LARGE_WIDTH, target_button, 0, i18n.get ("HelpSalePrice"), LayoutForHelp.BubleType.BUTTON);
				else
					lh.create_help_label(LayoutForHelp.LARGE_WIDTH, target_button, 0, i18n.get ("HelpNormalPrice"), LayoutForHelp.BubleType.BUTTON);
			}
			lh.show ();
		}
		public class TableSource : UITableViewSource {
			SettingsVC settings_vc;
			public TableSource (SettingsVC settings_vc)
			{
				this.settings_vc = settings_vc;
			}
			public override nint RowsInSection (UITableView tableview, nint section)
			{
				return settings_vc.limit_mode ? 0 : 5;
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				NameValueCell cell = (NameValueCell)tableView.DequeueReusableCell (NameValueCell.Key);
				// if there are no cells to reuse, create a new one
				if (cell == null)
					cell = NameValueCell.Create(tableView);
				switch (indexPath.Row) {
				case 0:
					NSString str = (NSString)NSBundle.MainBundle.InfoDictionary ["CFBundleVersion"];
					cell.setNameValueComment (i18n.get ("MoneyVersion"), str, "");
					cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
					cell.AccessoryView = null;
					break;
				case 1:
					cell.setNameValueComment (i18n.get ("PinTitle"), "", "");
					if (!String.IsNullOrEmpty (AppDelegate.app.pin_code)) {
						cell.Accessory = UITableViewCellAccessory.None;
						cell.AccessoryView = new UIImageView (AppDelegate.get_checkmark());
					} else {
						cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
						cell.AccessoryView = null;
					}
					break;
				case 2:
					cell.setNameValueComment (i18n.get("CurrencySelectedHeader"), String.Format("{0}", AppDelegate.app.docs.selected_currencies.Count) , "");
					cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
					cell.AccessoryView = null;
					break;
				case 3:
					cell.setNameValueComment (i18n.get("FontTitle"), UIFontSelect.get_font(), "");
					cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
					cell.AccessoryView = null;
					break;
				case 4:
					cell.setNameValueComment (i18n.get ("ImportInstructions20"), "", "");
					cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
					cell.AccessoryView = null;
					break;
				}
				return cell;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				switch (indexPath.Row) {
				case 0:
				{
					FlurryAnalytics.Flurry.LogEvent("Settings", NSDictionary.FromObjectsAndKeys(new object[]{"About"}, new object[]{"action"}));
					//AppDelegate.app.docs.switch_fake_full_version ();
					string path = NSBundle.MainBundle.PathForResource ("about", "html");
					string str = File.ReadAllText (path, Encoding.UTF8);
					str = str.Replace ("{appname2}", i18n.get ("AppName2"));
					settings_vc.NavigationController.PushViewController (FaqVC.create_or_reuse (i18n.get ("RealAboutTitle"), str), true);
					break;
				}
				case 1:
					if (UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Pad)
						settings_vc.NavigationController.PushViewController (new PinVC (true), true);
					else
						Utility.slide_up (settings_vc.NavigationController, new PinVC (true));
					break;
				case 2:
					settings_vc.NavigationController.PushViewController (CurrencyVC.create_or_reuse(), true);
					break;
				case 3:
					settings_vc.NavigationController.PushViewController (UIFontSelect.create_or_reuse(), true);
					break;
				case 4:
				{
					FlurryAnalytics.Flurry.LogEvent("Settings", NSDictionary.FromObjectsAndKeys(new object[]{"Faq"}, new object[]{"action"}));
					string path = NSBundle.MainBundle.PathForResource ("faq", "html");
					string str = File.ReadAllText (path, Encoding.UTF8);

					string instr = i18n.get ("ImportInstructions20");
					int ipn = instr.IndexOf ("{title}");
					int ipic1 = instr.IndexOf ("{picture1}");
					int ipic2 = instr.IndexOf ("{picture2}");
					int ipic3 = instr.IndexOf ("{picture3}");
					str = str.Replace ("{question}", instr.Substring (0, ipn));
					str = str.Replace ("{text1}", instr.Substring (ipn + 7, ipic1 - ipn - 7));
					str = str.Replace ("{text2}", instr.Substring (ipic1 + 10, ipic2-ipic1-10));
					str = str.Replace ("{text3}", instr.Substring (ipic2 + 10, ipic3-ipic2-10));
					settings_vc.NavigationController.PushViewController (FaqVC.create_or_reuse (i18n.get ("FAQTitle"), str), true);
					break;
				}
				}
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
		}
	}
}

