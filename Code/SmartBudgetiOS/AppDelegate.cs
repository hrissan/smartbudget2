using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;
using StoreKit;
using SmartBudgetCommon;
using ObjCRuntime;
using System.Json;
using System.IO;
using MessageUI;
using System.Drawing;
using System.Text;
using CoreGraphics;
using System.Globalization;
using SystemConfiguration;
using System.Threading;

namespace SmartBudgetiOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		static DateTime was = DateTime.UtcNow;
		static DateTime was_rememberd = was;

		UIWindow window;
		UINavigationController navigationController;
		UISplitViewController ipadSplitController;
		public AccountsVC accounts_vc; // on iPad only
		public SheetVC sheet_vc;
		public const string itunes_link = "http://itunes.apple.com/app/smart-budget-2/id675039911?ls=1&mt=8"; // TODO - fix link
		public const string SUPPORT_EMAIL = "support@smartbudgetapp.com";
		public UIColor positive_color = new UIColor (84f/255f, 129f/255f, 34f/255f, 1f);
		public UIColor neutral_color = new UIColor (83f/255f, 53f/255f, 36f/255f, 1f);
		public UIColor negative_color = new UIColor (196f/255f, 41f/255f, 10f/255f, 1f);

		//public UIColor dark_background_color = new UIColor (30/255f, 34/255f, 44/255f, 1f);
		public UIColor dark_background_color = new UIColor (40/255f, 46/255f, 58/255f, 1f);
		public UIColor orange_line_color = new UIColor (253/255f, 209/255f, 152/255f, 1f);
		public static UIColor table_background_color = new UIColor (255f/255f, 245f/255f, 223f/255f, 1f); // Alisa
		public static UIColor section_text_color = new UIColor (132 / 255f, 95 / 255f, 48 / 255f, 1);
		//public static UIColor table_background_color = new UIColor (254f/255f, 246f/255f, 185f/255f, 1f); // Old SB
		//public static UIColor table_background_color = new UIColor (255f/255f, 255f/255f, 255f/255f, 1f); // White
		public UIColor gray_text_color = new UIColor (194/255f, 195/255f, 199/255f, 1f);
		public UIColor white_text_color = new UIColor (1, 245 / 255f, 223 / 255f, 1);
		public UIColor conversion_color = new UIColor (163f/255f, 149f/255f, 138f/255f, 1f);
		public UIColor color_for_amount(long am_1000) {
			return am_1000 == 0 ? neutral_color : am_1000 < 0 ? negative_color : positive_color;
		}

		public static UIImage table_background_image = Utility.image_from_color (table_background_color);

		static public SmartBudgetiOS.AppDelegate app { get; private set; }
		public SmartBudgetCommon.Documents docs { get; private set; }

		static private List<string> report_times = new List<string>();
		static public void ReportTime(string text)
		{
			//DateTime now = DateTime.UtcNow;
			//report_times.Add( String.Format("dt={0, 5}milliSec - {1}", now.Subtract (was).Ticks / 10000, text));
			//was = now;
		}
		public class SplitViewDelegate : UISplitViewControllerDelegate {
			public override bool ShouldHideViewController (UISplitViewController svc, UIViewController viewController, UIInterfaceOrientation inOrientation)
			{
				return inOrientation == UIInterfaceOrientation.Portrait || inOrientation == UIInterfaceOrientation.PortraitUpsideDown;
			}

			public override void WillHideViewController (UISplitViewController svc, UIViewController aViewController, UIBarButtonItem barButtonItem, UIPopoverController pc)
			{
				UINavigationController detailView = svc.ViewControllers[1] as UINavigationController;
				detailView.ViewControllers[0].NavigationItem.LeftBarButtonItem = barButtonItem;
				barButtonItem.Title = i18n.get ("AccountsTitle");

				Utility.remember_splitview_popover (pc);
			}

			public override void WillShowViewController (UISplitViewController svc, UIViewController aViewController, UIBarButtonItem button)
			{
				UINavigationController detailView = svc.ViewControllers[1] as UINavigationController;
				detailView.ViewControllers[0].NavigationItem.LeftBarButtonItem = null;
				Utility.remember_splitview_popover (null);
			}
		}
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			SmartBudgetiOS.AppDelegate.app = this;
//			app.StatusBarStyle = UIStatusBarStyle.BlackOpaque;
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			NSUrl options_nsurl = null;
			{
				NSObject url;
				if (options != null && options.TryGetValue (UIApplication.LaunchOptionsUrlKey, out url)) {
					options_nsurl = url as NSUrl;
					if (options_nsurl != null && options_nsurl.Scheme == "smartbudgetapp" && options_nsurl.ResourceSpecifier.IndexOf("emergency") != -1 ) {
						navigationController = new UINavigationController (new EmergencyVC());
						navigationController.NavigationBar.TintColor = AppDelegate.app.dark_background_color;
						window.RootViewController = navigationController;
						window.MakeKeyAndVisible ();
						return false;
					}
				}
			}

			ReportTime("00");
			FlurryAnalytics.Flurry.StartSession ("C3V7JP46BD9Y6W7XG5SC");
			ReportTime("00.1");
			FlurryAnalytics.Flurry.SetSessionReportsOnClose(false);
			FlurryAnalytics.Flurry.SetSessionReportsOnPause(false);
			//DateTime start = DateTime.UtcNow;
			ReportTime("0");

			ReportTime("0.1");
			var attrs = new UITextAttributes () {
				//				Font = UIFont.FromName ("Chalkduster", 20),
				TextColor = UIColor.White,
				TextShadowColor = UIColor.Black,
				TextShadowOffset = new UIOffset (0, 0)
			};
/*			var attrs2 = new UITextAttributes () {
				//				Font = UIFont.FromName ("Chalkduster", 20),
				TextColor = UIColor.Gray,
				TextShadowColor = UIColor.Black,
				TextShadowOffset = new UIOffset (0, 0)
			};*/
			UINavigationBar.Appearance.SetTitleTextAttributes (attrs);

			UINavigationBar.Appearance.BarTintColor = new UIColor (66/255f, 74 / 255f, 91 / 255f, 1);
			UINavigationBar.Appearance.TintColor = white_text_color;
			//UINavigationBar.Appearance.SetBackgroundImage (UIImage.FromBundle ("new_design/navbar.png").StretchableImage(20, 0), UIBarMetrics.Default);
			ReportTime("0.2");
			//UINavigationBar.Appearance.SetBackgroundImage (UIImage.FromBundle ("new_design/navbar.png"), UIBarMetrics.LandscapePhone);
			//UISearchBar.Appearance.BackgroundImage = table_background_image;// UIImage.FromBundle ("new_design/table.png");
			//UISearchBar.Appearance.ScopeBarBackgroundImage = table_background_image;//UIImage.FromBundle ("new_design/table.png");
			ReportTime("0.3");
			UISearchBar.Appearance.TintColor = white_text_color;
			UISearchBar.Appearance.BarTintColor = UINavigationBar.Appearance.BarTintColor;
			ReportTime("0.4");
			UIBarButtonItem.UIBarButtonItemAppearance bbap = UIBarButtonItem.AppearanceWhenContainedIn(typeof(UINavigationBar));
			bbap.SetBackgroundImage (UIImage.FromBundle ("new_design/nav_btn_n.png").StretchableImage (14, 0), UIControlState.Normal, UIBarMetrics.Default);
			bbap.SetBackgroundImage (UIImage.FromBundle ("new_design/nav_btn_s.png").StretchableImage (14, 0), UIControlState.Highlighted, UIBarMetrics.Default);
			bbap.SetBackgroundImage (UIImage.FromBundle ("new_design/nav_btn_d.png").StretchableImage (14, 0), UIControlState.Disabled, UIBarMetrics.Default);
			bbap.SetBackButtonBackgroundImage (UIImage.FromBundle ("new_design/nav_back_n.png").StretchableImage (14, 0), UIControlState.Normal, UIBarMetrics.Default);
			bbap.SetBackButtonBackgroundImage (UIImage.FromBundle ("new_design/nav_back_s.png").StretchableImage (14, 0), UIControlState.Highlighted, UIBarMetrics.Default);
			bbap.SetBackButtonTitlePositionAdjustment(new UIOffset(4,0), UIBarMetrics.Default);
			bbap.TintColor = white_text_color;
			//bbap.SetTitleTextAttributes(attrs, UIControlState.Normal);
			//bbap.SetTitleTextAttributes(attrs, UIControlState.Highlighted);
			//bbap.SetTitleTextAttributes(attrs2, UIControlState.Disabled);

			UITableViewHeaderFooterView.Appearance.TintColor = AppDelegate.table_background_color;
			ReportTime("1");

			//SmartBudgetCommon.i18n.add_strings (File.ReadAllText(Path.Combine (NSBundle.MainBundle.ResourcePath, "i18n.txt"), Encoding.UTF8));
			//SmartBudgetCommon.i18n.add_locs ("en");

			ReportTime("2");
			Utility.RTL = i18n.get ("RTL") == "1";
			NSLocale loc = NSLocale.CurrentLocale;
			string local_iso_symbol = loc.CurrencyCode;
			//create_formatters (loc); // TODO - optimize

			ReportTime("2.1");

			//string settings_json = NSUserDefaults.StandardUserDefaults.StringForKey ("docs");

			ReportTime("2.2");

			WebClientChinese.dispatch = new DispatchAdapter (app);
			ReportTime("2.3");
			//WebClientChinese.start_check_direct ();
			NSTimer.CreateScheduledTimer (1, delegate{
				//bool cydia_url = app.CanOpenUrl(new NSUrl("cydia://package/com.example.package"));
				//string device = DeviceHardware.Version;
				//Console.WriteLine("Device: {0}", device);
				//FlurryAnalytics.Flurry.LogEvent("JB", NSDictionary.FromObjectsAndKeys(new object[]{device, cydia_url ? "1" : "0"}, new object[]{"hw", "cydia_url"}));
				//string[] langs = NSLocale.PreferredLanguages;
				//if( langs.Length > 0 )
				//	FlurryAnalytics.Flurry.LogEvent("Language", NSDictionary.FromObjectsAndKeys(new object[]{langs[0]}, new object[]{"0"}));

				WebClientChinese.start_check_direct ();

				refresh_ok_for_sale ();
				refresh_price ();

				docs.sync_some_document (true, null);
			});
			NSTimer.CreateScheduledTimer (5, delegate{
				WebClientChinese.start_check_proxy ();
			});
			ReportTime("2.4");

			XLSXStringsExporter.path_to_xlsx_template = Path.Combine (NSBundle.MainBundle.ResourcePath, "xlsx");

			ReportTime("3");

			docs = new SmartBudgetCommon.Documents (local_iso_symbol);

			ReportTime("4");
			UIFontSelect.load_settings();

			ReportTime("4.1");
			if (options_nsurl != null && options_nsurl.Scheme == "smartbudgetapp" && options_nsurl.ResourceSpecifier.IndexOf ("resetpin") != -1) {
				pin_code = "";
			}
			must_enter_initial_pincode = !String.IsNullOrEmpty(pin_code);

			NSNotificationCenter.DefaultCenter.AddObserver (NSUbiquitousKeyValueStore.DidChangeExternallyNotification, (n) => {
				docs.synchronize_icloud_changes();
//					if( navigationController.PresentedViewController != null )
//						navigationController.DismissViewController (false, null);
//					if( navigationController.ChildViewControllers.Length != 1 )
//						navigationController.PopToRootViewController (false);
					//docs.send_another_loaded(null);
			});
			NSUbiquitousKeyValueStore.DefaultStore.Synchronize ();
			ReportTime("4.15");
			docs.synchronize_icloud_changes();
			ReportTime("4.2");

			ReportTime("4.3");

			custom_payment_observer = new CustomPaymentObserver ();
			SKPaymentQueue.DefaultQueue.AddTransactionObserver (custom_payment_observer);

			ReportTime("4.4");

			if( options_nsurl != null )
				OpenBackup (options_nsurl);

			ReportTime("5");
			sheet_vc = new SheetVC ();;
			// TODO - add pin controller in set
			ReportTime("6");
			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) {
				ipadSplitController = new UISplitViewController ();
				navigationController = new UINavigationController (sheet_vc);
				accounts_vc = AccountsVC.create_or_reuse();
				ipadSplitController.Delegate = new SplitViewDelegate ();
				/*ipadSplitController.ShouldHideViewController = (svc, vc, orientation) => {
					return orientation == UIInterfaceOrientation.Portrait || orientation == UIInterfaceOrientation.PortraitUpsideDown;
				};
				ipadSplitController.WillHideViewController += (sender, e) => {
					UINavigationController detailView = (sender as UISplitViewController).ViewControllers[1] as UINavigationController;
					detailView.ViewControllers[0].NavigationItem.LeftBarButtonItem = e.BarButtonItem;
					e.BarButtonItem.Title = i18n.get ("AccountsTitle");
				};
				ipadSplitController.WillShowViewController += (sender, e) => {
					UINavigationController detailView = (sender as UISplitViewController).ViewControllers[1] as UINavigationController;
					detailView.ViewControllers[0].NavigationItem.LeftBarButtonItem = null;
				};*/
				ipadSplitController.ViewControllers = new UIViewController[] { new UINavigationController (accounts_vc), navigationController };
				window.RootViewController = ipadSplitController;
			} else {
				navigationController = new UINavigationController (sheet_vc);
				window.RootViewController = navigationController;
			}
			ReportTime("7");
			// If you have defined a root view controller, set it here:
			// make the window visible
			window.MakeKeyAndVisible ();
			ReportTime("8");
			showPinAgain ();
			ReportTime("9");

			ReportTime("10");

			ReportTime("11");
			//Console.WriteLine("dt={0, 5}milliSec - AppDelegate.FinishedLaunching", DateTime.UtcNow.Subtract (was_rememberd).Ticks / 10000);
			//foreach (var str in report_times)
			//	Console.WriteLine (str);

			return false; // Otherwise HandleOpenURL will be called
		}
		public bool OpenBackup(NSUrl url)
		{
			if (url == null)
				return false;
			Console.WriteLine ("OpenBackup {0}", url.Path);
			byte[] dataBytes;
			NSError error;
			using (NSData data = NSData.FromUrl (url, NSDataReadingOptions.Mapped, out error)) {
				if (data == null)
					return false;
				dataBytes = Utility.from_nsdata (data);
				if (dataBytes == null)
					return false;
			}
			Stream ss2 = new MemoryStream (dataBytes);
			string error_text;
			Document dd = docs.create_standalone_doc(out error_text);
			//Document dd = docs.create_standalone_doc_from_data (ss2, out error_text);
			if (dd == null) {
				new UIAlertView (i18n.get ("LoadFromBackupErrorTitle"), i18n.get (error_text), null, i18n.get ("Ok")).Show ();
				return false;
			}
			docs.loading_operations_in_progress += 1;
			docs.send_anything_changed(null);
			new Thread(() =>
			{
				// dd and error_text is owned by the thread
				Console.WriteLine("OpenBackup Thread Started");
				try {
					JsonObject jbody = null;
					using (TextReader tr = new StreamReader (ss2, System.Text.Encoding.UTF8)) {
						jbody = (JsonObject)JsonObject.Load (tr);
					}

					dd.load_from_backup (jbody);
				} catch(Exception) {
					error_text = "LOAD_FROM_BACKUP_BAD_FORMAT";
					if( dd != null ) {
						dd.conn.Dispose (); // We do not want stuck open databases
						dd.conn = null;
						dd = null;
					}
				}
				WebClientChinese.dispatch.Invoke(delegate{
					Console.WriteLine("OpenBackup Dispatch Invoke Started");
					if( dd == null ) {
						new UIAlertView (i18n.get ("LoadFromBackupErrorTitle"), i18n.get (error_text), null, i18n.get ("Ok")).Show ();
					}else if( !docs.add_standalone_doc (dd) ) {
						error_text = "LOAD_FROM_BACKUP_EXISTS";
						new UIAlertView (i18n.get ("LoadFromBackupErrorTitle"), i18n.get (error_text), null, i18n.get ("Ok")).Show ();
					}
					docs.loading_operations_in_progress -= 1;
					docs.send_anything_changed(null);
					docs.sync_some_document(false, null);
					Console.WriteLine("OpenBackup Dispatch Invoke Finished");
				});
				Console.WriteLine("OpenBackup Thread Finished");
			}).Start();
			//docs.load_data_streams.Add(ss2);
			//docs.select_document(dd, null);
			//docs.load_from_backup (ss2);

			// TODO - start sync process
			// save_settings ();
			return true;
		}
		public override bool HandleOpenURL (UIApplication application, NSUrl url)
		{
			if (url.Scheme == "smartbudgetapp") {
				if (url.ResourceSpecifier.IndexOf ("emergency") != -1) {
					UIAlertView av2 = new UIAlertView ("Emergency mode", "The app will be terminated, launch again with the same link to enter emergency mode.", null, i18n.get ("Ok"));
					av2.Clicked += (sender2, e2) => {
						throw new Exception("We quit the app so we can enter emergency mode");
					};
					av2.Show ();
				}
				if (url.ResourceSpecifier.IndexOf ("resetpin") != -1) {
					dismissPin ();
					pin_code = "";
					must_enter_initial_pincode = false;
				}
				return true;
			}
			if (OpenBackup (url)) {
				//dismissPin ();
				//if( navigationController.PresentedViewController != null )
				//	navigationController.DismissViewController (false, null);
				//if( navigationController.ChildViewControllers.Length != 1 )
				//	navigationController.PopToRootViewController (false);
				// showPinAgain ();
				//docs.show_new_documents = true;
				docs.send_anything_changed (null);
				return true;
			}
			return true;
		}
		public override void DidEnterBackground (UIApplication application)
		{
			sheet_vc.AppDidEnterForeground ();
			// Reset formatters :(
			this.date_week_formatter_v = null;
			this.date_formatter_v = null;
			this.month_year_formatter_v = null;
			docs.cancel_sync_if_unpublished (); // This will crash us in emergency mode :)
			must_enter_initial_pincode = !String.IsNullOrEmpty(pin_code);
			Utility.dismiss_popovers_and_action_sheets (false);
			showPinAgain ();
		}
		public override void WillEnterForeground (UIApplication application)
		{
			refresh_ok_for_sale ();
			refresh_price ();
			docs.start_exchange_rate_update ();
			docs.sync_some_document (true, null);
			//sheet_vc.AppWillEnterForeground ();
		}
		private const string PINCODE_KEY = "pin_code";
		public string pin_code {
			get {
				return NSUserDefaults.StandardUserDefaults.StringForKey (PINCODE_KEY);
			}
			set {
				FlurryAnalytics.Flurry.LogEvent("Pin", NSDictionary.FromObjectsAndKeys(new object[]{String.IsNullOrEmpty(value) ? "0" : "1"}, new object[]{"set"}));
				NSUserDefaults.StandardUserDefaults.SetString (value, PINCODE_KEY);
				NSUserDefaults.StandardUserDefaults.Synchronize ();
			}
		}
		public bool must_enter_initial_pincode;
		private PinVC pin_vc;
		private UIViewController find_top_modal_controller()
		{
			UIViewController vc = window.RootViewController;
			while( vc.PresentedViewController != null )
				vc = vc.PresentedViewController;
			return vc;
		}
		private void dismissPin()
		{
			// Dismiss popovers on iPad
			if (pin_vc != null && pin_vc.PresentingViewController != null) {
				pin_vc.PresentingViewController.DismissViewController (false, null);
			}
		}
		private void showPinAgain()
		{
			dismissPin ();
			if (must_enter_initial_pincode) {
				if( pin_vc == null)
					pin_vc = new PinVC (false);
				UINavigationController nc = new UINavigationController(pin_vc);
				nc.ModalTransitionStyle = UIModalTransitionStyle.CoverVertical;
				if( UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad )
					nc.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
				find_top_modal_controller().PresentViewController(nc, false, null);
			}
		}
		private NSDateFormatter date_week_formatter_v; 
		public NSDateFormatter date_week_formatter { get {
				if (date_week_formatter_v == null) {
					NSLocale loc = NSLocale.CurrentLocale;
					date_week_formatter_v = new NSDateFormatter ();
					date_week_formatter_v.TimeStyle = NSDateFormatterStyle.None;
					string test = NSDateFormatter.GetDateFormatFromTemplate ("yyyyMMMMdEE", 0, loc);
					//IntPtr cl_handle = new ObjCRuntime.Class (typeof(NSDateFormatter)).Handle;
					//Selector selector = new Selector ("dateFormatFromTemplate:options:locale:");
					//NSString str = new NSString("yyyyMMMMdEE");
					//IntPtr handle = ObjCRuntime.Messaging.IntPtr_objc_msgSend_IntPtr_int_IntPtr (cl_handle, selector.Handle,str.Handle,0,loc.Handle);
					//NSString test = new NSString (handle);
					date_week_formatter_v.DateFormat = test;
				}
				return date_week_formatter_v;
			}}
		private NSDateFormatter date_formatter_v; 
		public NSDateFormatter date_formatter { get {
				if (date_formatter_v == null) {
					date_formatter_v = new NSDateFormatter ();
					date_formatter_v.TimeStyle = NSDateFormatterStyle.None;
					date_formatter_v.DateStyle = NSDateFormatterStyle.Long;
				}
				return date_formatter_v;
			}}
		private NSDateFormatter month_year_formatter_v; 
		public NSDateFormatter month_year_formatter { get {
				if (month_year_formatter_v == null) {
					month_year_formatter_v = new NSDateFormatter ();
					month_year_formatter_v.DateFormat = "LLLL yyyy";
				}
				return month_year_formatter_v;
			}}

/*		static public UIPopoverController show_in_popover(UIViewController vc, UIView origin)
		{
			UINavigationController nc = new UINavigationController (vc);
			UIPopoverController result = new UIPopoverController (nc);
			result.PresentFromRect(origin.Bounds, origin, UIPopoverArrowDirection.Any, true);
			return result;
		}*/
		public const int BUTTON_ROW_HEIGHT = 58;
		//public const int[] ROWS_HEIGHTS = new int[] { 58, 58+48, 58+48+48 };
		static public void decorate_bottom_button(UIButton btn, string image_prefix, bool checkmark)
		{
			btn.SetImage (UIImage.FromBundle("new_design/bb_" + image_prefix + "_n.png"), checkmark ? UIControlState.Highlighted : UIControlState.Normal);
			btn.SetImage (UIImage.FromBundle("new_design/bb_" + image_prefix + "_s.png"), checkmark ? UIControlState.Normal : UIControlState.Highlighted);
		}
		static public UIButton create_flat_bottom_button(UIView view, string title, nfloat bottom_y)
		{
			CGRect rr = view.Bounds;
			UIButton btn = new UIButtonSelectFont (18, UIButtonType.Custom);
			//btn.BackgroundColor = UIColor.Magenta;
			//int bottom_hei = BUTTON_ROW_HEIGHT*(row + 1);
			rr.Y = bottom_y - (BUTTON_ROW_HEIGHT - 5);//view.Bounds.Height - bottom_hei + 10;
			rr.Height = 48;
			rr.Width = 246;
			rr.X = (view.Bounds.Width - rr.Width)/2;
			btn.Frame = rr;
			btn.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin | UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleLeftMargin;
			btn.SetTitle (title, UIControlState.Normal);
			UIImage nor = Gray_button ();
			UIImage hig = Gray_button_selected ();
			btn.SetBackgroundImage (nor, UIControlState.Normal);
			btn.SetBackgroundImage(hig, UIControlState.Highlighted);
			btn.SetTitleColor(app.white_text_color, UIControlState.Normal);
			btn.ContentEdgeInsets = new UIEdgeInsets (0, 6, 0, 6);
			btn.TitleLabel.AdjustsFontSizeToFitWidth = true;
			btn.TitleLabel.MinimumScaleFactor = 0.5f;
			//btn.TitleLabel.MinimumFontSize = 10;
			view.Add (btn);
			return btn;
		}
		static public UIButton create_flat_bottom_button(UIView view, string title, int row)
		{
			return create_flat_bottom_button(view, title, view.Bounds.Height - BUTTON_ROW_HEIGHT*row + 5);
		}
		public const int ORANGE_LINE_HEIGHT = 4;
		public const int ORANGE_LINE_OVERLAY = 1;
		public const float BOBO_HEIGHT = 5;
		static public UITableView create_table_and_background(UIView view, int bottom_rows, bool orange_line = true)
		{
			return create_table_and_background (view, 0, view.Bounds.Height - bottom_rows*BUTTON_ROW_HEIGHT, orange_line);
		}
		static public UITableView create_table_and_background(UIView view, nfloat y, nfloat height, bool orange_line)
		{
			CGRect all_rr = view.Bounds;
			all_rr.Y = y;
			all_rr.Height = height;

			CGRect table_rr = all_rr;
			table_rr.Height -= BOBO_HEIGHT;
			if (orange_line) {
				table_rr.Y += ORANGE_LINE_HEIGHT - ORANGE_LINE_OVERLAY;
				table_rr.Height -= ORANGE_LINE_HEIGHT - ORANGE_LINE_OVERLAY;
			}
			CGRect orange_line_rr = all_rr;
			orange_line_rr.Height = ORANGE_LINE_HEIGHT;

			CGRect bobo_rr = all_rr;
			bobo_rr.Y = bobo_rr.Y + bobo_rr.Height - BOBO_HEIGHT;
			bobo_rr.Height = BOBO_HEIGHT;
//			rr.Y = orange_line ? ORANGE_LINE_HEIGHT - ORANGE_LINE_OVERLAY : 0;
//			rr.Height -= bottom_hei + (orange_line ? ORANGE_LINE_HEIGHT - ORANGE_LINE_OVERLAY : 0);
//			RectangleF irr = rr;

			/*UIImageView ipad_bim = null;
			if (margins != 0 ) {
				ipad_bim = new UIImageView (table_background_image);
				ipad_bim.Frame = rr;
				ipad_bim.ContentMode = UIViewContentMode.ScaleToFill;
				ipad_bim.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
				view.InsertSubview (ipad_bim, 0);
				irr.X += margins;
				irr.Width -= 2*margins;
			}*/
			UITableView table = new UITableView (table_rr, UITableViewStyle.Plain);
			table.RegisterClassForHeaderFooterViewReuse (typeof(UITableViewHeaderFooterView), SectionHeader2.Key);
			table.BackgroundColor = table_background_color;
			table.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			table.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			table.RowHeight = 40;
			//if (ipad_bim != null)
			//	view.InsertSubviewAbove (table, ipad_bim);
			//else
			view.InsertSubview (table, 0);

			UIView bobo = new UIView (bobo_rr);
			bobo.BackgroundColor = table_background_color;
			bobo.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin;
			view.InsertSubview (bobo, 0);

			create_table_cover (view, table, all_rr.Y + all_rr.Height);

/*			rr.Y = view.Bounds.Height - bottom_hei;
			rr.Height = bottom_hei;
			UIView bim = new UIView (rr);
			bim.BackgroundColor = app.dark_background_color;
			//bim.Frame = rr;
			//bim.ContentMode = UIViewContentMode.ScaleToFill;
			bim.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin;
			view.InsertSubview (bim, 0);
*/
			if( orange_line) {
				UIView orv = new UIView (orange_line_rr);
				orv.BackgroundColor = app.orange_line_color;
				orv.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleBottomMargin;
				view.InsertSubviewAbove(orv, table);
			}
			return table;
		}
		static public UIView create_table_background(UIView view, nfloat from_y, nfloat height, UIViewAutoresizing for_height)
		{
			CGRect rr = view.Bounds;
			rr.Y = from_y + ORANGE_LINE_HEIGHT;
			rr.Height = height - ORANGE_LINE_HEIGHT;
			UIView table = new UIView (rr);
			table.BackgroundColor = table_background_color;
			table.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | for_height;
			view.InsertSubview (table, 0);

			rr.Y = 0;
			rr.Height = ORANGE_LINE_HEIGHT;
			UIView orv = new UIView (rr);
			orv.BackgroundColor = app.orange_line_color;
			orv.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleBottomMargin;
			view.InsertSubview (orv, 0);

			return table;
		}
		public const float NOTEBOOK_HEADER_HEIGHT = 51;
		static public UIView create_notebook_header(UIView view)
		{
			UIImage notebook_header_i = UIImage.FromBundle ("new_design/notebook_header.png");

			CGRect rr = view.Bounds;
			rr.Height = NOTEBOOK_HEADER_HEIGHT;
			UIImageView bim = new UIImageView (rr);
			bim.Image = notebook_header_i.StretchableImage(120, 0);
			bim.ContentMode = UIViewContentMode.ScaleToFill;
			bim.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleBottomMargin;
			view.InsertSubview (bim, 0);

			return bim;
		}	
		static UIImage table_bot_i = UIImage.FromBundle ("new_design/table_bot.png").StretchableImage(16, 0);
		static public UIView create_table_cover(UIView view, UIView above, nfloat bottom_y)
		{
			CGRect rr = view.Bounds;
			rr.Y = bottom_y - table_bot_i.Size.Height;
			rr.Height = table_bot_i.Size.Height;
			UIImageView bim = new UIImageView (rr);
			bim.Image = table_bot_i;
			bim.ContentMode = UIViewContentMode.ScaleToFill;
			bim.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin;
			view.InsertSubviewAbove(bim, above);

			return bim;
		}
		//public static UIImage attention_place_image = UIImage.FromBundle ("new_design/attention_place.png");
		private static UIImage attention_icon_v;
		public static UIImage get_attention_icon() {
			return attention_icon_v ?? (attention_icon_v = UIImage.FromBundle ("new_design/attention_icon.png"));
		}
		private static UIImage attention_icon_n_v;
		public static UIImage get_attention_icon_n() {
			return attention_icon_n_v ?? (attention_icon_n_v = UIImage.FromBundle ("new_design/attention_icon_n.png"));
		}
		private static UIImage attention_icon_small_v;
		public static UIImage get_attention_icon_small() {
			return attention_icon_small_v ?? (attention_icon_small_v = UIImage.FromBundle ("new_design/attention_icon_small.png"));
		}
		private static UIImage attention_calendar_v;
		public static UIImage get_attention_calendar() {
			return attention_calendar_v ?? (attention_calendar_v = UIImage.FromBundle ("new_design/attention_calendar.png"));
		}
		private static UIImage error_triangle_v;
		public static UIImage get_error_triangle() {
			return error_triangle_v ?? (error_triangle_v = UIImage.FromBundle ("new_design/error_triangle.png"));
		}
		public static UIImage attention_icon_top = UIImage.FromBundle ("new_design/attention_icon_top.png");
		public static UIImage error_triangle_small = UIImage.FromBundle ("new_design/error_triangle_small.png");
		public static UIImage selected_cell = UIImage.FromBundle("selected_cell.png");
		private static UIImage checkmark_v;
		public static UIImage get_checkmark() {
			return checkmark_v ?? (checkmark_v = UIImage.FromBundle ("new_design/checkmark.png"));
		}
		private static UIImage Gray_button_v;
		public static UIImage Gray_button() {
			return Gray_button_v ?? (Gray_button_v = UIImage.FromBundle ("new_design/Gray_button.png").StretchableImage (6, 20));
		}
		private static UIImage Gray_button_selected_v;
		public static UIImage Gray_button_selected() {
			return Gray_button_selected_v ?? (Gray_button_selected_v = UIImage.FromBundle ("new_design/Gray_button_selected.png").StretchableImage (6, 20));
		}
		private static UIImage progress_red_v;
		public static UIImage progress_red() {
			return progress_red_v ?? (progress_red_v = UIImage.FromBundle ("new_design/progress_red.png"));
		}
		private static UIImage progress_green_v;
		public static UIImage progress_green() {
			return progress_green_v ?? (progress_green_v = UIImage.FromBundle ("new_design/progress_green.png"));
		}
		//public void save_settings ()
		//{
		//	NSUserDefaults.StandardUserDefaults.SetString(docs.save_settings(), "docs");
		//}
		public static MFMailComposeViewController create_email(UINavigationController nc, string subj, string body, byte[] sb2backup, string backup_name, bool debug_info)
		{
			if (!MFMailComposeViewController.CanSendMail) {
				new UIAlertView (i18n.get ("NoEMailSetupTitle"), i18n.get ("NoEMailSetupText"), null, i18n.get ("Cancel")).Show ();
				return null;
			}
			MFMailComposeViewController picker = new MFMailComposeViewController ();
			picker.SetSubject (subj);
			body = i18n.ReplaceFirst( i18n.ReplaceFirst( body, "%@", Utility.get_bundle_version()), "%@", Utility.get_system_version());
			picker.SetMessageBody (body, false);
			if( sb2backup != null)
				picker.AddAttachmentData (NSData.FromArray(sb2backup), "application/vnd.com.smartbudgetapp.sb2backup", backup_name);
			if (debug_info) {
				NSDictionary dic = NSUbiquitousKeyValueStore.DefaultStore.ToDictionary(); // TODO test ios8
				NSError error;
				NSData kvs_data = NSPropertyListSerialization.DataWithPropertyList (dic, NSPropertyListFormat.Xml, NSPropertyListWriteOptions.Immutable, out error);

				if (kvs_data != null)
					picker.AddAttachmentData (kvs_data, "application/xml", "debug_kvs.plist");
				//dic = NSUserDefaults.StandardUserDefaults.AsDictionary ();
				//kvs_data = NSPropertyListSerialization.DataWithPropertyList (dic, NSPropertyListFormat.Xml, NSPropertyListWriteOptions.Immutable, out error);
				kvs_data = NSData.FromFile (Documents.get_db_path (Documents.settings_filename));

				if (kvs_data != null)
					picker.AddAttachmentData (kvs_data, "application/json", "settings.txt");
				picker.SetToRecipients (new string[]{SUPPORT_EMAIL});
			}
			picker.Finished += (sender, e) => {
				nc.DismissViewController(true, null);
			};
			nc.PresentViewController (picker, true, null);
			return picker;
		}
		static public AmountVC start_expense_chain(bool planned, Document doc)
		{
			if (doc.sorted_sheets.Count == 0)
				return null;
			if (doc.sorted_accounts.Count == 0)
				return null;
			DBExpense ex = new DBExpense ();
			ex.planned = planned;
			ex.sheet = doc.selected_sheet;
			if (String.IsNullOrEmpty(ex.sheet))
				ex.sheet = doc.sorted_sheets [0].id;
			ex.category = ""; // ok, unknown
			ex.sum.account = doc.sorted_accounts[0].id;
			ex.sum.currency = AppDelegate.app.docs.next_selected_currency ("", "");
			AmountVC am = AmountVC.create_or_reuse(doc, ex.sum, new Sum(), 0, true, (avc, sum)=>{
				ex.sum = sum;
				avc.NavigationController.PushViewController(CategoryVC.create_or_reuse(doc, Math.Sign(ex.sum.amount_1000), ex.category, true, (cvc, cat)=>{
					ex.category = cat;
					if (ex.category == Document.CONVERT_CATEGORY) {
						ex.sum2.amount_1000 = 0;
						ex.sum2.currency = AppDelegate.app.docs.next_selected_currency(ex.sum.currency, "");
						ex.sum2.account = ex.sum.account;
						cvc.NavigationController.PushViewController(AmountVC.create_or_reuse (doc, ex.sum2, ex.sum, -Math.Sign (ex.sum.amount_1000), true, (avc2, sum2)=>{
							ex.sum2 = sum2;
							Utility.replace_last_controllers(avc2.NavigationController, 3, ExpenseVC.create_or_reuse(doc, ex, null), true);
						}), true);
						return;
					}
					if (ex.category == Document.TRANSFER_CATEGORY) {
						ex.sum2.amount_1000 = -ex.sum.amount_1000;
						ex.sum2.currency = ex.sum.currency;
						ex.sum2.account = doc.next_selected_account(ex.sum.account);
						if(doc.sorted_accounts.Count == 2) {
							Utility.replace_last_controllers(cvc.NavigationController, 2, ExpenseVC.create_or_reuse(doc, ex, null), true);
							return;
						}
						cvc.NavigationController.PushViewController(AccountSelect.create_or_reuse( doc, i18n.get ("SelectAccountTitleTo"), "", ex.sum.account, true, (asc, acc)=>{ // Not ex.sum2.account
							ex.sum2.account = acc;
							Utility.replace_last_controllers(asc.NavigationController, 3, ExpenseVC.create_or_reuse(doc, ex, null), true);
						}), true);
						return;
					}
					if (ex.is_any_loan()) {
						ex.sum2.amount_1000 = 0;
						ex.sum2.currency = null;
						ex.sum2.account = null;
						cvc.NavigationController.PushViewController(NameVC.create_or_reuse(doc, i18n.get ("ExpenseTitleNew"), i18n.get ("ExpenseNamePlaceholder"),true,true,true,"", ex.category, (nvc, str) => {
							ex.name = str;
							if( ex.category == Document.LOAN_CATEGORY ) {
								nvc.NavigationController.PushViewController(DateVC.create_or_reuse(i18n.get ("ExpenseReturnDate"),i18n.get ("SelectNoReturnDate"),true,DateTime.Now.AddDays(14).Ticks, (dvc, da)=>{ // TODO - constant 7 days
									Utility.replace_last_controllers(dvc.NavigationController, 4, ExpenseVC.create_or_reuse(doc, ex, null, da), true);
								}),true);
							}else
								Utility.replace_last_controllers(nvc.NavigationController, 3, ExpenseVC.create_or_reuse(doc, ex, null), true);
						}), true);
						return;
					}
					ex.sum2.amount_1000 = 0;
					ex.sum2.currency = null;
					ex.sum2.account = null;
					// TODO - check for SKIP_DESCRIPTION and react accordingly
					cvc.NavigationController.PushViewController(NameVC.create_or_reuse(doc, i18n.get ("ExpenseTitleNew"), i18n.get ("ExpenseNamePlaceholder"),false,true,true,"", ex.category, (nvc, str) => {
						ex.name = str;
						Utility.replace_last_controllers(nvc.NavigationController, 3, ExpenseVC.create_or_reuse(doc, ex, null), true);
					}), true);
				}), true);
			});
			return am;
		}
		public override void ReceiveMemoryWarning (UIApplication application)
		{
			images_cache.Clear ();
			images_cat_cache.Clear ();
		}
		private Dictionary<byte[], UIImage> images_cache = new Dictionary<byte[], UIImage> (new DBCategory.ByteArrayEqualityComparer());
		private Dictionary<string, UIImage> images_cat_cache = new Dictionary<string, UIImage> ();
		public UIImage get_category_image(DBCategory cat)
		{
			return get_category_image (cat.image_name, cat.image_data);
		}
		public UIImage get_category_hires_image(DBCategory cat)
		{
			if (cat.image_name == null)
				return get_category_image ("", cat.image_data);
			return get_category_image (cat.image_name.Replace(".png", "@2x.png"), cat.image_data);
		}
		public UIImage get_category_image(string image_name, byte[] image_data)
		{
			DateTime was = DateTime.UtcNow;
			if( image_data == null ) {
				UIImage result = null;
				if (images_cat_cache.TryGetValue (image_name, out result))
					return result;
				result = UIImage.FromBundle ("cats/" + image_name);
				images_cat_cache.Add (image_name, result);
//				DateTime now = DateTime.UtcNow;
//				Console.WriteLine("get_category_image {0, 8} mksec", now.Subtract (was).Ticks / 10);
				return result;
			}
			UIImage cached_image;
			if (images_cache.TryGetValue (image_data, out cached_image))
				return cached_image;
			nfloat fr, fg, fb, fa;
			neutral_color.GetRGBA (out fr, out fg, out fb, out fa);
			// will divide by 256, so we mul by 256 also
			int rr = Math.Max(0, Math.Min(256, (int)Math.Floor(256*fr)));
			int gg = Math.Max(0, Math.Min(256, (int)Math.Floor(256*fg)));
			int bb = Math.Max(0, Math.Min(256, (int)Math.Floor(256*fb)));
			UIImage img = UIImage.LoadFromData ( NSData.FromArray(image_data) );
			int wi = (int)img.Size.Width;
			int he = (int)img.Size.Height;
			RectangleF image_rect = new RectangleF (0, 0, wi, he);

			using(CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB()) {
				byte[] contextBuffer = new byte[wi*he*4];
				using( CGBitmapContext context = new CGBitmapContext (contextBuffer, wi, he, 8, wi * 4, colorSpace, CGImageAlphaInfo.PremultipliedLast) ) {
					context.SetBlendMode (CGBlendMode.Clear);
					context.SetFillColor (1, 1, 1, 0);
					context.FillRect (image_rect);
					context.TranslateCTM (0, image_rect.Height);
					context.ScaleCTM (1f, -1f);
					UIGraphics.PushContext(context);
					img.Draw (PointF.Empty, CGBlendMode.Copy, 1);
					UIGraphics.PopContext();
					for (int i = 0; i != wi*he; ++i) {
						byte a = contextBuffer [i * 4 + 3];
						contextBuffer [i * 4 + 0] = (byte)(rr * a >> 8);
						contextBuffer [i * 4 + 1] = (byte)(gg * a >> 8);
						contextBuffer [i * 4 + 2] = (byte)(bb * a >> 8);
					}
					using (CGImage cgImage = context.ToImage ()) {
						if (cgImage.Handle.ToInt32() != 0) {
							cached_image = new UIImage (cgImage);
						}
					}
				}
			}
			images_cache.Add (image_data, cached_image);
			DateTime now2 = DateTime.UtcNow;
			//Console.WriteLine("get_category_image RENDER {0, 8} mksec", now2.Subtract (was).Ticks / 10);
			return cached_image;
		}
	}
}

