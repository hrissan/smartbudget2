using System;
using System.Drawing;
using Foundation;
using UIKit;
using SmartBudgetCommon;
using System.Collections.Generic;
using MessageUI;
using System.IO;

namespace SmartBudgetiOS
{
	public partial class EmergencyVC : UIViewController
	{
		private UITableView table;
		List<string> filenames = new List<string>();

		public EmergencyVC () : base ("EmergencyVC", null)
		{
			NavigationItem.Title = "Emergency mode";

			NavigationItem.RightBarButtonItem = new UIBarButtonItem ("iCloud", UIBarButtonItemStyle.Plain, (sender, e) => {
				NSDictionary dic = NSUbiquitousKeyValueStore.DefaultStore.ToDictionary();
				NSError error;
				NSData kvs_data = NSPropertyListSerialization.DataWithPropertyList (dic, NSPropertyListFormat.Xml, NSPropertyListWriteOptions.Immutable, out error);

				if (kvs_data == null)
					new UIAlertView ("Error", "Failed to get iCloud data", null, i18n.get ("Ok")).Show ();
				else
					create_email("iCloud data", kvs_data, "application/xml", "debug_kvs.plist");
			});
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = AppDelegate.app.dark_background_color;

			table = new UITableView (View.Bounds);
			table.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			table.BackgroundColor = AppDelegate.table_background_color;
			table.Source = new TableSource (this);
			View.AddSubview (table);

			anything_changed ();
		}
		void anything_changed()
		{
			filenames.Clear ();
			foreach(var fileName in System.IO.Directory.EnumerateFiles(Documents.get_dbs_path ()))
			{
				System.IO.FileInfo fi = new System.IO.FileInfo (fileName);
				filenames.Add (fi.Name);
				//if(fi.f)
				//{
				//}
			}
			table.ReloadData ();
		}
		public MFMailComposeViewController create_email(string subj, NSData sb2backup, string backup_mime, string backup_name)
		{
			if (!MFMailComposeViewController.CanSendMail) {
				new UIAlertView (i18n.get ("NoEMailSetupTitle"), i18n.get ("NoEMailSetupText"), null, i18n.get ("Cancel")).Show ();
				return null;
			}
			MFMailComposeViewController picker = new MFMailComposeViewController ();
			picker.SetSubject (subj);
			//body = i18n.ReplaceFirst( i18n.ReplaceFirst( body, "%@", Utility.get_bundle_version()), "%@", Utility.get_system_version());
			//picker.SetMessageBody (body, false);
			picker.SetToRecipients (new string[]{AppDelegate.SUPPORT_EMAIL});

			if( sb2backup != null)
				picker.AddAttachmentData (sb2backup, backup_mime, backup_name);
			picker.Finished += (sender, e) => {
				NavigationController.DismissViewController(true, null);
			};
			NavigationController.PresentViewController (picker, true, null);
			return picker;
		}

		public class TableSource : UITableViewSource {
			EmergencyVC parent_vc;
			public TableSource (EmergencyVC parent_vc)
			{
				this.parent_vc = parent_vc;
			}
			public override nint RowsInSection (UITableView tableview, nint section)
			{
				return parent_vc.filenames.Count;
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				UITableViewCell cell = (UITableViewCell)tableView.DequeueReusableCell ("Key");
				// if there are no cells to reuse, create a new one
				if (cell == null) {
					cell = new UITableViewCell(UITableViewCellStyle.Default, "Key");
					cell.TextLabel.Font = UIFont.SystemFontOfSize (15);
				}
				cell.TextLabel.Text = parent_vc.filenames [indexPath.Row];
				return cell;
			}
			static string send = "Send by E-Mail";
			static string restore = "Restore deleted";
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				string filename = parent_vc.filenames [indexPath.Row];
				string fullname = Path.Combine (Documents.get_dbs_path (), filename);
				List<string> btns = new List<string> ();
				btns.Add (send);
				if (filename.EndsWith (".db3.erased"))
					btns.Add (restore);
				UIActionSheet ash2 = new UIActionSheet(fullname, null, i18n.get ("Cancel"), null, btns.ToArray() );
				ash2.Clicked += (sender2, e2) => {
					if( e2.ButtonIndex == ash2.DestructiveButtonIndex ) {
//						FileInfo old_fi = new FileInfo (fullname);
//						old_fi.Delete();
//						parent_vc.anything_changed();
						return;
					}
					if (ash2.ButtonTitle (e2.ButtonIndex) == send) {
						parent_vc.create_email("Emergency mode - file data", NSData.FromFile(fullname), "application/data", filename);
						return;
					}
					if (ash2.ButtonTitle (e2.ButtonIndex) == restore) {
						FileInfo old_fi = new FileInfo (fullname);
						string fullname2 = fullname.Replace(".db3.erased", ".db3");
						old_fi.MoveTo(fullname2);
						parent_vc.anything_changed();
						return;
					}
				};
				ash2.ShowInView(parent_vc.View);
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
		}
	}
}

