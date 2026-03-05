using System;
using UIKit;
using QuickLook;
using System.Collections.Generic;
using System.Drawing;
using SmartBudgetCommon;
using CoreGraphics;
using System.IO;
using Foundation;

namespace SmartBudgetiOS
{
	public partial class SheetVC
	{
		public class DocumentsShower {
			UIDocumentInteractionController preview_vc;
			QLPreviewController preview2_vc;
			public UIView panelDocuments;
			UIButton btnHelpDocuments;
			UIButton btnCloud;
			UIButton btnExport;
			public UIButton btnTrash;

			UIImageView imgCloudWiFiFrames;
			UIButton btnCloudProblem;
			UIButton btnCloudRefresh;
//			UIImageView imgCloudOff;
//			UIImageView imgCloudOn;
			UIImageView imgCloudWifi;

			public List<DocumentView> document_views = new List<DocumentView>();
			public DocumentView new_document_view;

			SheetVC parent_vc;
			public DocumentsShower(SheetVC parent_vc)
			{
				this.parent_vc = parent_vc;
			}
			public void ViewDidLoad ()
			{
				AppDelegate.create_notebook_header (parent_vc.currentDocView);

				panelDocuments = BottomPanelView.create_bottom_panel (parent_vc.View);

				btnHelpDocuments = BottomPanelView.create_help_button( panelDocuments, "help");
				btnCloud = BottomPanelView.create_bottom_button( panelDocuments, "cloud");
				btnExport = BottomPanelView.create_bottom_button( panelDocuments, "export");
				btnTrash = BottomPanelView.create_bottom_button( panelDocuments, "trash");

				UIImage cloud_on = UIImage.FromBundle (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? "new_design/cloud_on_ipad.png" : "new_design/cloud_on.png");
				UIImage cloud_off = UIImage.FromBundle (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? "new_design/cloud_off_ipad.png" : "new_design/cloud_off.png");
				UIImage wifi_static = UIImage.FromBundle ("new_design/wifi_static.png");
				const float WIFI_SHIFT_UP = 6;
				parent_vc.panelCloud.Frame = new CGRect ((nfloat)Math.Floor ((parent_vc.View.Frame.Width - cloud_on.Size.Width) / 2), 8, cloud_on.Size.Width, cloud_on.Size.Height - WIFI_SHIFT_UP + wifi_static.Size.Height);

				btnCloudRefresh = new UIButton (UIButtonType.Custom);
				btnCloudRefresh.Frame = new CGRect (0, 0, cloud_on.Size.Width, cloud_on.Size.Height);
				btnCloudRefresh.SetImage (cloud_on, UIControlState.Normal);
				btnCloudRefresh.SetImage (cloud_off, UIControlState.Disabled);
				//imgCloudOn.Image = cloud_on;
				parent_vc.panelCloud.AddSubview (btnCloudRefresh);
				//imgCloudOff = new UIImageView (new RectangleF(0,0,cloud_off.Size.Width, cloud_off.Size.Height));
				//imgCloudOff.Image = cloud_off;
				//parent_vc.panelCloud.AddSubview (imgCloudOff);
				imgCloudWifi = new UIImageView (new CGRect((nfloat)Math.Floor((parent_vc.panelCloud.Frame.Width - wifi_static.Size.Width)/2),btnCloudRefresh.Frame.Height - WIFI_SHIFT_UP, wifi_static.Size.Width, wifi_static.Size.Height));
				imgCloudWifi.Image = wifi_static;
				parent_vc.panelCloud.AddSubview (imgCloudWifi);
				btnCloudProblem = new UIButton (UIButtonType.Custom);
				btnCloudProblem.Frame = btnCloudRefresh.Frame;
				btnCloudProblem.SetImage (AppDelegate.get_error_triangle (), UIControlState.Normal);
				parent_vc.panelCloud.AddSubview (btnCloudProblem);

				imgCloudWiFiFrames = new UIImageView (imgCloudWifi.Frame);
				imgCloudWiFiFrames.AutoresizingMask = imgCloudWifi.AutoresizingMask;
				imgCloudWiFiFrames.AnimationImages = new UIImage[] { UIImage.FromBundle("new_design/wifi_1.png"), UIImage.FromBundle("new_design/wifi_2.png"), UIImage.FromBundle("new_design/wifi_3.png") };
				imgCloudWiFiFrames.AnimationDuration = 0.9;
				imgCloudWiFiFrames.StartAnimating ();
				//imgCloudWifi.Superview.InsertSubviewBelow (imgCloudWiFiFrames, imgCloudWifi);
				parent_vc.panelCloud.AddSubview (imgCloudWiFiFrames);

				btnCloudProblem.TouchUpInside += (sender, e) => {
					show_help(true);
					//Document dd = get_doc_from_scroll_pos();
					//if( dd != null ) {
					//	dd.need_to_sync(true);
					//	AppDelegate.app.docs.sync_some_document(false);
					//}
				};

				btnCloudRefresh.TouchUpInside += (sender, e) => {
					Document dd = get_doc_from_scroll_pos();
					if( dd != null ) {
						dd.need_to_sync(true);
						AppDelegate.app.docs.sync_some_document(false, null);
					}
				};

				btnHelpDocuments.TouchUpInside += (sender, e) => {
					show_help(false);
				};

				btnCloud.TouchUpInside += (sender, e) => {
					Document update_doc = this.get_doc_from_scroll_pos();
					if( update_doc == null )
						return;
					List<string> btns = new List<string>();
					string tit;
					if( !update_doc.is_published () ) {
						tit = i18n.get("HelpNotPublished");
						btns.Add (i18n.get ("Publish20"));
					}
					else {
						tit = i18n.get("HelpPublished");
						if( !String.IsNullOrEmpty(update_doc.a_token) ) {
							btns.Add (i18n.get ("Invite20"));
						}
						btns.Add (i18n.get ("Unpublish20"));
					}
					UIActionSheet ash2 = new UIActionSheet(tit, null, i18n.get ("Cancel"), null, btns.ToArray());
					ash2.Clicked += (sender2, e2) => {
						//if( e2.ButtonIndex == ash2.DestructiveButtonIndex ){
						//}
						if(ash2.ButtonTitle(e2.ButtonIndex) == i18n.get ("Publish20")){
							if(AppDelegate.app.docs.get_shareware_progress(update_doc) >= Documents.SHAREWARE_LIMIT) {
								FlurryAnalytics.Flurry.LogEvent("Limit", NSDictionary.FromObjectsAndKeys(new object[]{"Publish"}, new object[]{"from"}));
								new UIAlertView (i18n.get ("LimitTitle"), i18n.get ("LimitText"), null, i18n.get ("Cancel")).Show ();
								return;
							}
							FlurryAnalytics.Flurry.LogEvent("Document", NSDictionary.FromObjectsAndKeys(new object[]{"Publish"}, new object[]{"action"}));
							update_doc.prepare_publish();
							AppDelegate.app.docs.send_anything_changed(null);
							AppDelegate.app.docs.sync_some_document(false, null);
						}
						if(ash2.ButtonTitle(e2.ButtonIndex) == i18n.get ("Invite20")){
							FlurryAnalytics.Flurry.LogEvent("Document", NSDictionary.FromObjectsAndKeys(new object[]{"Invite"}, new object[]{"action"}));
							byte[] invitation_data = update_doc.create_invitation(Utility.get_bundle_version(), Utility.get_system_version());
							AppDelegate.create_email(parent_vc.NavigationController, i18n.get ("ShareEMailSubj"), i18n.ReplaceFirst( i18n.get ("ShareEMailBody"), "%@", AppDelegate.itunes_link), invitation_data, "invitation.sb2backup", false);
						}
						if(ash2.ButtonTitle(e2.ButtonIndex) == i18n.get ("Unpublish20")){
							UIActionSheet ash3 = new UIActionSheet(i18n.get("UnpublishTitle20") + " " + i18n.get ("OtherDevicesWillContinue"), null, i18n.get ("Cancel"), i18n.get ("Unpublish20"));
							ash3.Clicked += (sender3, e3) => {
								if( e3.ButtonIndex == ash3.DestructiveButtonIndex ){
									FlurryAnalytics.Flurry.LogEvent("Document", NSDictionary.FromObjectsAndKeys(new object[]{"Unpublish"}, new object[]{"action"}));
									update_doc.unpublish();
									AppDelegate.app.docs.send_anything_changed(null);
									AppDelegate.app.docs.cancel_sync_if_unpublished();
								}
							};
							Utility.show_action_sheet(ash3, parent_vc.View);
						}
					};
					Utility.show_action_sheet(ash2, parent_vc.View, btnCloud);
				};
				btnExport.TouchUpInside += (sender, e) => {
					Document update_doc = this.get_doc_from_scroll_pos();
					if( update_doc == null )
						return;
					List<string> options = new List<string>{i18n.get ("BackupButtonTitle20"), i18n.get ("ExportXSLXButtonTitle"), i18n.get ("PreviewButtonTitle")};
					if (!String.IsNullOrEmpty( update_doc.get_cloud_problem_text(true) ) )
						options.Add( i18n.get ("SendToSupportTitle") );
					UIActionSheet ash2 = new UIActionSheet(i18n.get("ExportMenuTitle"), null, i18n.get ("Cancel"), null, options.ToArray() );
					ash2.Clicked += (sender2, e2) => {
						//if( e2.ButtonIndex == ash2.DestructiveButtonIndex ){
						//}
						if(ash2.ButtonTitle(e2.ButtonIndex) == i18n.get ("BackupButtonTitle20")){
							FlurryAnalytics.Flurry.LogEvent("Document", NSDictionary.FromObjectsAndKeys(new object[]{"Backup"}, new object[]{"action"}));
							string fname = Path.Combine(Documents.get_caches_path(), "backup.sb2backup"); // TODO - better name
							byte[] backup_data = update_doc.create_backup(Utility.get_bundle_version(), Utility.get_system_version());
							// TODO - exception
							File.WriteAllBytes(fname, backup_data);
							preview_vc = UIDocumentInteractionController.FromUrl( NSUrl.FromFilename(fname) );
							CGRect rr = parent_vc.View.ConvertRectFromView(btnExport.Bounds, btnExport);
							preview_vc.PresentOptionsMenu(rr, parent_vc.View, true);
						}
						if(ash2.ButtonTitle(e2.ButtonIndex) == i18n.get ("ExportXSLXButtonTitle")){
							FlurryAnalytics.Flurry.LogEvent("Document", NSDictionary.FromObjectsAndKeys(new object[]{"ExportXSLX"}, new object[]{"action"}));
							string fname = Path.Combine(Documents.get_caches_path(), "export.xlsx"); // TODO - better name
							byte[] backup_data = update_doc.export_xlsx();
							// TODO - exception
							File.WriteAllBytes(fname, backup_data);
							preview_vc = UIDocumentInteractionController.FromUrl( NSUrl.FromFilename(fname) );
							CGRect rr = parent_vc.View.ConvertRectFromView(btnExport.Bounds, btnExport);
							preview_vc.PresentOptionsMenu(rr, parent_vc.View, true);
						}
						if(ash2.ButtonTitle(e2.ButtonIndex) == i18n.get ("PreviewButtonTitle")){
							FlurryAnalytics.Flurry.LogEvent("Document", NSDictionary.FromObjectsAndKeys(new object[]{"PreviewXSLX"}, new object[]{"action"}));
							string fname = Path.Combine(Documents.get_caches_path(), "preview.xlsx"); // TODO - better name
							byte[] backup_data = update_doc.export_xlsx();
							// TODO - exception
							File.WriteAllBytes(fname, backup_data);
							preview2_vc = new QLPreviewController();
							preview2_vc.DataSource = new Utility.PreviewDataSource( NSUrl.FromFilename(fname), i18n.get ("PreviewButtonTitle") );
							parent_vc.NavigationController.PresentViewController(preview2_vc, true, null);
						}
						if(ash2.ButtonTitle(e2.ButtonIndex) == i18n.get ("SendToSupportTitle")){
							UIAlertView av = new UIAlertView (i18n.get ("SendToSupportTitle"), i18n.get ("SendToSupportText"), null, i18n.get ("Cancel"), i18n.get ("OK"));
							av.Clicked += (sender3, e3) => {
								if( e3.ButtonIndex != av.CancelButtonIndex ) {
									byte[] backup_data = update_doc.create_database_copy();
									AppDelegate.create_email(parent_vc.NavigationController, i18n.get ("EMailDatabaseProblemSubject"), i18n.get ("EMailDatabaseProblemBody"), backup_data, "corrupted.db3", true);
								}
							};
							av.Show ();
						}

					};
					Utility.show_action_sheet(ash2, parent_vc.View, btnExport);
				};
				btnTrash.TouchUpInside += (sender, e) => {
					string tit = this.get_doc_from_scroll_pos().is_published() ? i18n.get ("DeleteDatabaseTitlePublished") + " " + i18n.get ("OtherDevicesWillContinue") : i18n.get ("DeleteDatabaseTitle");
					UIActionSheet ash2 = new UIActionSheet(tit, null, i18n.get ("Cancel"), i18n.get ("Delete"));
					ash2.Clicked += (sender2, e2) => {
						if( e2.ButtonIndex == ash2.DestructiveButtonIndex ){
							UIActionSheet ash3 = new UIActionSheet(tit, null, i18n.get ("Cancel"), i18n.get ("Delete"));
							ash3.Clicked += (sender3, e3) => {
								if( e3.ButtonIndex == ash3.DestructiveButtonIndex ){
									FlurryAnalytics.Flurry.LogEvent("Document", NSDictionary.FromObjectsAndKeys(new object[]{"Delete"}, new object[]{"action"}));
									Document del_doc = this.get_doc_from_scroll_pos ();
									int ind = AppDelegate.app.docs.documents.IndexOf (del_doc);
									if (ind == -1)
										return;
									if (!AppDelegate.app.docs.remove_doc (del_doc, this))
										return;
									AppDelegate.app.docs.send_anything_changed (null);
									//layout_all_document_views(AppDelegate.app.docs.selected_doc);
								}
							};
							Utility.show_action_sheet(ash3, parent_vc.View);
						}
					};
					Utility.show_action_sheet(ash2, parent_vc.View, btnTrash);
				};
				parent_vc.scrollDocs.Scrolled += (sender, e) => {
					update_on_scroll();
				};
				CGRect fr = parent_vc.scrollDocs.Frame;
				fr.X = parent_vc.View.Bounds.X;
				fr.Width = parent_vc.View.Bounds.Width;
				Utility.UIScrollViewExtender ext = new Utility.UIScrollViewExtender (parent_vc.scrollDocs, fr);
				//ext.BackgroundColor = new UIColor (0.5f, 0, 0, 0.5f);
				parent_vc.scrollDocs.Superview.InsertSubviewBelow (ext, parent_vc.scrollDocs);
				//scrollDocs.BackgroundColor = new UIColor (0, 0.5f, 0, 0.5f);

				//update_on_scroll ();
				AppDelegate.app.docs.anything_changed += (docs, e) => {
					update_on_scroll();
				};
			}
			private void show_help(bool refresh_after)
			{
				LayoutForHelp lh = new LayoutForHelp(parent_vc.NavigationController, parent_vc.scrollDocs.Frame.Y + parent_vc.scrollDocs.Frame.Height);
				Document update_doc = get_doc_from_scroll_pos();
				if (refresh_after && update_doc != null) {
					lh.on_close = delegate {
						update_doc.need_to_sync(true);
						AppDelegate.app.docs.sync_some_document(false, null);
					};
				}
				if( update_doc == null )
					lh.create_help_label(LayoutForHelp.LARGE_WIDTH, parent_vc.scrollDocs, 0, i18n.get ("HelpCreateDatabase"), LayoutForHelp.BubleType.NO_TAILS);
				else {
					if (update_doc.is_published ()) {
						string problem = update_doc.get_cloud_problem_text(false);
						if( !String.IsNullOrEmpty(problem) )
							lh.create_help_label(LayoutForHelp.LARGE_WIDTH, btnCloudRefresh, 0, i18n.get (problem), LayoutForHelp.BubleType.BUTTON);
						else if (update_doc.need_commit()) {
							lh.create_help_label(LayoutForHelp.LARGE_WIDTH, btnCloudRefresh, 0, i18n.get ("HelpNeedCommit"), LayoutForHelp.BubleType.BUTTON);
						} else {
							lh.create_help_label(LayoutForHelp.LARGE_WIDTH, btnCloudRefresh, 0, i18n.get ("HelpPublished"), LayoutForHelp.BubleType.BUTTON);
						}
					} else {
						lh.create_help_label(LayoutForHelp.LARGE_WIDTH, btnCloudRefresh, 0, i18n.get ("HelpNotPublished"), LayoutForHelp.BubleType.BUTTON);
					}
				}
				lh.show ();
			}
			public void ViewWillLayoutSubviews ()
			{
				BottomPanelView.layout (panelDocuments, btnHelpDocuments);
				layout_scroll ();
				CGRect sfr = parent_vc.scrollDocs.Frame;
				for(int i = 0; i != document_views.Count; ++i) {
					DocumentView dv = document_views [i];
					dv.Bounds = parent_vc.currentDocViewParagonBounds();
					CGPoint ce = dv.Center;
					ce.X = sfr.Width * (i + 0.5f);
					ce.Y = sfr.Height * 0.5f;
					dv.Center = ce;
				}
				new_document_view.Bounds = parent_vc.currentDocViewParagonBounds();
				CGPoint ce2 = new_document_view.Center;
				ce2.X = sfr.Width * (document_views.Count + 0.5f);
				ce2.Y = sfr.Height * 0.5f;
				new_document_view.Center = ce2;
			}
			private void layout_scroll()
			{
				float scroll_width = (float)Math.Floor(parent_vc.currentDocViewParagonBounds().Width * 0.5 + 60); // CONSTANT in code
				CGRect sfr = parent_vc.scrollDocs.Frame;
				sfr.Width = scroll_width;
				sfr.X = (float)Math.Floor( (parent_vc.View.Bounds.Width - scroll_width) / 2 );
				sfr.Y = parent_vc.panelCloud.Frame.Y + btnCloudProblem.Frame.Height;
				sfr.Height = parent_vc.View.Frame.Height - BottomPanelView.PANEL_HEIGHT - sfr.Y;
				parent_vc.scrollDocs.Frame = sfr;
				parent_vc.scrollDocs.ContentSize = new CGSize (sfr.Width*(document_views.Count + 1), 0);
				//if (parent_vc.scrollDocs.ContentOffset.X > sfr.Width * document_views.Count)
				//	parent_vc.scrollDocs.SetContentOffset (new PointF (sfr.Width * document_views.Count, 0), true);
				//else 
				{
					int ind = AppDelegate.app.docs.documents.IndexOf (get_doc_from_scroll_pos ());
					if (ind == -1)
						ind = AppDelegate.app.docs.documents.Count;
					parent_vc.scrollDocs.SetContentOffset (new CGPoint (sfr.Width * ind, 0), true);
				}
				update_on_scroll ();
			}
			public void layout_all_document_views(Document jump_to_document)
			{
				//Console.WriteLine ("layout_all_document_views");
				layout_scroll ();
				CGRect sfr = parent_vc.scrollDocs.Frame;

				if (new_document_view == null) { // First time
					// document_views are empty
					for (int i = 0; i != AppDelegate.app.docs.documents.Count; ++i) {
						DocumentView dv = create_new_document_view_at (i + 0.5f);
						dv.setDocument (AppDelegate.app.docs.documents[i]);
						document_views.Add (dv);
					}
					new_document_view = create_new_document_view_at (AppDelegate.app.docs.documents.Count + 0.5f);
					new_document_view.setDocument (null);
					parent_vc.scrollDocs.ContentSize = new CGSize (sfr.Width*(AppDelegate.app.docs.documents.Count + 1), 0);
					int ind = AppDelegate.app.docs.documents.IndexOf (parent_vc.sheet_shower.doc);
					if (ind == -1)
						ind = AppDelegate.app.docs.documents.Count;
					parent_vc.scrollDocs.ContentOffset = new CGPoint(sfr.Width*ind, 0);
					update_on_scroll ();
					return;
				}

				List<DocumentView> removing_dvs = new List<DocumentView> ();
				for(int i = 0; i != document_views.Count; ++i) {
					var dv = document_views [i];
					int new_ind = AppDelegate.app.docs.documents.IndexOf (dv.doc);
					//float new_pos = (new_ind == -1) ? i - 0.5f : new_ind;
					//dv.Center = new PointF (sfr.Width*(new_pos + 0.5f), sfr.Height * 0.5f);
					if (new_ind == -1) {
						removing_dvs.Add (dv);
						document_views.RemoveAt (i);
						i -= 1;
					}
				}
				List<DocumentView> prev_views = document_views;
				document_views = new List<DocumentView>();
				for (int i = 0; i != AppDelegate.app.docs.documents.Count; ++i) {
					var dd = AppDelegate.app.docs.documents [i];
					int pind = prev_views.FindIndex (pv => pv.doc == dd);
					if( pind != -1 ) {
						document_views.Add(prev_views[pind]);
					}else{
						DocumentView dv = create_new_document_view_at (i);
						dv.setDocument (dd);
						dv.Alpha = 0;
						//dv.Center = new PointF (sfr.Width*(i + 0.5f), sfr.Height * 0.5f);
						document_views.Add(dv);
					}
				}
				if (removing_dvs.Count == 0 && document_views.Count == prev_views.Count) {
					for(int i = 0; i != document_views.Count; ++i){
						document_views[i].Center = new CGPoint (sfr.Width*(i + 0.5f), sfr.Height * 0.5f);
						document_views[i].Alpha = 1;
					}
					new_document_view.Center = new CGPoint (sfr.Width*(0.5f + document_views.Count), sfr.Height * 0.5f);
				}
				else {
					UIView.Animate (0.5, delegate{
						foreach(var dv in removing_dvs) {
							CGPoint prev_ce = dv.Center;
							prev_ce.X -= sfr.Width * 0.5f;
							dv.Center = prev_ce;
							dv.Alpha = 0;
						}
						for(int i = 0; i != document_views.Count; ++i){
							document_views[i].Center = new CGPoint (sfr.Width*(i + 0.5f), sfr.Height * 0.5f);
							document_views[i].Alpha = 1;
						}
						new_document_view.Center = new CGPoint (sfr.Width*(0.5f + document_views.Count), sfr.Height * 0.5f);
					}, delegate {
						foreach(var dv in removing_dvs) {
							dv.RemoveFromSuperview();
						}
					});
				}
				parent_vc.scrollDocs.ContentSize = new CGSize (sfr.Width*(document_views.Count + 1), 0);
				if( parent_vc.scrollDocs.ContentOffset.X > sfr.Width*document_views.Count )
					parent_vc.scrollDocs.SetContentOffset(new CGPoint(sfr.Width*document_views.Count, 0), true);
				update_on_scroll ();
			}
			public void send_anything_changed_to_document_views()
			{
				foreach (var dv in document_views)
					dv.anything_changed ();
			}
			public DocumentView create_new_document_view_at(float pos)
			{
				CGRect sfr = parent_vc.scrollDocs.Frame;
				DocumentView dv = DocumentView.Create ((dd)=>{
					if(dd != null){
						parent_vc.animate_back();
					}
					else {
						if( AppDelegate.app.docs.documents.Count == 0 )
							create_new_document();
						else {
							UIActionSheet ash = new UIActionSheet(i18n.get ("HelpCreateDatabase"), null, i18n.get ("Cancel"), null, i18n.get ("CreateDatabase"));
							ash.Clicked += (sender3, e3) => {
								if( ash.ButtonTitle(e3.ButtonIndex) == i18n.get ("CreateDatabase") ){
									create_new_document();
								}
							};
							Utility.show_action_sheet(ash, parent_vc.View);
						}
					}
				});
				dv.Bounds = parent_vc.currentDocViewParagonBounds();// new RectangleF (0, 0, 320, DOC_HEIGHT);
				dv.Center = new CGPoint (sfr.Width*(pos), sfr.Height * 0.5f);
				dv.Transform = CGAffineTransform.MakeScale (DOC_SCALE, DOC_SCALE);
				parent_vc.scrollDocs.Add (dv);
				return dv;
			}
			public Document get_doc_from_scroll_pos()
			{
				int pos = Math.Max (0,Math.Min (AppDelegate.app.docs.documents.Count, (int)Math.Floor(0.5f + parent_vc.scrollDocs.ContentOffset.X/parent_vc.scrollDocs.Frame.Width)));
				if (pos < AppDelegate.app.docs.documents.Count)
					return AppDelegate.app.docs.documents [pos];
				return null;
			}
			public void update_on_scroll()
			{
				Document update_doc = get_doc_from_scroll_pos();
				parent_vc.update_on_scroll_from_doc_shower (update_doc);
				if (update_doc != null) {
					// TODO - update Accounts on iPad
					btnTrash.Enabled = true;
					btnExport.Enabled = true;
					btnCloud.Enabled = true;
					//done_left.Enabled = true;
				} else {
					// TODO - disabled Accounts on iPad
					btnTrash.Enabled = false;
					btnExport.Enabled = false;
					btnCloud.Enabled = false;
					//done_left.Enabled = false;
				}
				// Cloud animation
				if (update_doc != null && update_doc.is_published ()) {
					btnCloudRefresh.Enabled = true;
					if (update_doc.need_to_sync(false)) {
						imgCloudWifi.Hidden = true;
						imgCloudWiFiFrames.Hidden = false;
						btnCloudProblem.Hidden = true;
					} else {
						imgCloudWifi.Hidden = false;
						imgCloudWiFiFrames.Hidden = true;
						string problem = update_doc.get_cloud_problem_text(false);
						btnCloudProblem.Hidden = String.IsNullOrEmpty (problem);
					}
				} else {
					btnCloudRefresh.Enabled = false;
					imgCloudWifi.Hidden = true;
					imgCloudWiFiFrames.Hidden = true;
					btnCloudProblem.Hidden = true;
				}
			}
			private void create_new_document()
			{
				FlurryAnalytics.Flurry.LogEvent("Document", NSDictionary.FromObjectsAndKeys(new object[]{"Create"}, new object[]{"action"}));
				string error_text;
				Document nd = AppDelegate.app.docs.create_standalone_doc(out error_text);
				AppDelegate.app.docs.add_standalone_doc(nd);
				//AppDelegate.app.docs.select_document (nd, null);
				AppDelegate.app.docs.send_anything_changed (null);
				//layout_all_document_views (nd);
			}
/*			public void animate_new_document()
			{
				string error_text;
				Document nd = AppDelegate.app.docs.create_standalone_doc_from_data(null, out error_text);
				AppDelegate.app.docs.add_standalone_doc(nd);
				AppDelegate.app.docs.select_document (nd, this);
				DocumentView dv = create_new_document_view_at(document_views.Count - 0.5f);
				dv.setDocument (nd);
				document_views.Add (dv);
				RectangleF sfr = parent_vc.scrollDocs.Frame;
				dv.Alpha = 0;
				parent_vc.scrollDocs.ContentSize = new SizeF (sfr.Width*(document_views.Count + 1), 0);

				parent_vc.scrollDocs.SetContentOffset( new PointF(sfr.Width*(document_views.Count - 1), 0), true);
				update_on_scroll ();
				UIView.Animate (0.5, delegate{
					dv.Alpha = 1;
					dv.Center = new PointF (sfr.Width*(0.5f + document_views.Count - 1), sfr.Height * 0.5f);
					new_document_view.Center = new PointF (sfr.Width*(0.5f + document_views.Count), sfr.Height * 0.5f);
				}, delegate {
				});
			}*/
/*			public void animate_delete_doc()
			{
				Document del_doc = this.get_doc_from_scroll_pos ();
				int ind = AppDelegate.app.docs.documents.IndexOf (del_doc);
				if (ind == -1)
					return;
				if (!AppDelegate.app.docs.remove_doc (del_doc, this))
					return;
//				del_doc.unpublish ();
				AppDelegate.app.docs.send_anything_changed (null);
				//AppDelegate.app.docs.cancel_sync_if_unpublished ();
				//AppDelegate.app.docs.save_settings ();
				DocumentView dv = document_views [ind];
				document_views.RemoveAt (ind);

				RectangleF sfr = parent_vc.scrollDocs.Frame;
				update_on_scroll ();

				UIView.Animate (0.5, delegate {
					dv.Alpha = 0;
					dv.Center = new PointF (sfr.Width * (ind), sfr.Height * 0.5f);
					for (int i = ind; i < document_views.Count; ++i) {
						document_views [i].Center = new PointF (sfr.Width * (0.5f + i), sfr.Height * 0.5f);
					}
					new_document_view.Center = new PointF (sfr.Width * (0.5f + document_views.Count), sfr.Height * 0.5f);
				}, delegate {
					dv.RemoveFromSuperview ();
					parent_vc.scrollDocs.ContentSize = new SizeF (sfr.Width * (document_views.Count + 1), 0);
				});
			}
			public void animate_load_document()
			{
				if (!AppDelegate.app.docs.show_new_documents)
					return;
				Console.WriteLine ("animate_load_document");
				AppDelegate.app.docs.show_new_documents = false;

				RectangleF sfr = parent_vc.scrollDocs.Frame;
				parent_vc.scrollDocs.SetContentOffset( new PointF(sfr.Width*(document_views.Count - 1), 0), true);
				update_on_scroll ();

							DocumentView dv = create_new_document_view_at(document_views.Count - 0.5f);
			dv.RemoveFromSuperview ();
			dv.Center = new PointF (160, -DOC_HEIGHT/4);
			View.Add (dv);
			dv.setDocument (nd);
			document_views.Add (dv);

			RectangleF sfr = scrollDocs.Frame;
			dv.Alpha = 0;
			scrollDocs.ContentSize = new SizeF (sfr.Width*(document_views.Count + 1), sfr.Height - 100);

			scrollDocs.SetContentOffset( new PointF(sfr.Width*(document_views.Count - 1), 0), true);
			update_on_scroll ();
			UIView.Animate (1, delegate{
				dv.Alpha = 1;
				dv.Center = scrollDocs.Center;
				new_document_view.Center = new PointF (sfr.Width*(0.5f + document_views.Count), sfr.Height * 0.5f);
			}, delegate {
				dv.RemoveFromSuperview();
				dv.Center = new PointF (sfr.Width*(0.5f + document_views.Count - 1), sfr.Height * 0.5f);
				scrollDocs.Add (dv);
				animate_load_document();
			});
			}*/
		};	
	}
}

