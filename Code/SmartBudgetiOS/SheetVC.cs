using System;
using System.Drawing;
using Foundation;
using UIKit;
using System.Collections.Generic;
using SmartBudgetCommon;
using CoreGraphics;
using System.IO;
using MessageUI;
using QuickLook;

namespace SmartBudgetiOS
{
	public partial class SheetVC : UIViewController
	{
		SheetShower sheet_shower;
		DocumentsShower doc_shower;
		UIImageView expenseTableCover;
		UIView currentDocView;
		//List<Document> docs_at_load;
		bool showing_scroll;

		UIBarButtonItem plus_right;
		//UIBarButtonItem done_left;
		const float DOC_SCALE = 0.5f;
		//const int DOC_HEIGHT = 440;

		///UIPopoverController popover;
		//UIView popover_origin;

		public SheetVC () : base ("SheetVC", null)
		{
			NavigationItem.Title = i18n.get("SheetTitle"); // Should be here for early UIViewController animation
			//plus_right = new UIBarButtonItem (UIBarButtonSystemItem.Add, (sender, e) => {
			//	sheet_shower.start_new_expense();
			//});
			plus_right = new UIBarButtonItem (UIImage.FromBundle("new_design/plus_btn_wide_n.png"), UIBarButtonItemStyle.Plain, (sender, e) => {
				sheet_shower.start_new_expense();
			});
			plus_right.SetBackgroundImage (UIImage.FromBundle ("new_design/nav_plus_n.png").StretchableImage (14, 0), UIControlState.Normal, UIBarMetrics.Default);
			//plus_right.SetBackgroundImage (UIImage.FromBundle ("new_design/nav_plus_n.png").StretchableImage (14, 0), UIControlState.Highlighted, UIBarMetrics.Default);
			sheet_shower = new SheetShower (this);
			sheet_shower.doc = AppDelegate.app.docs.selected_doc;
//			this.ExtendedLayoutIncludesOpaqueBars = false;
//			this.EdgesForExtendedLayout = UIRectEdge.None;
		}
		public override bool ShouldAutorotate ()
		{
			return true;
		}
		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations ()
		{
			return UIInterfaceOrientationMask.All;
		}
/*		public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation ()
		{
			return base.PreferredInterfaceOrientationForPresentation ();
		}*/
		//[Obsolete ("Deprecated in iOS6. Replace it with both GetSupportedInterfaceOrientations and PreferredInterfaceOrientationForPresentation")]
		//public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		//{
		//	return true;
		//}
		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
			bool was_portrait = fromInterfaceOrientation == UIInterfaceOrientation.Portrait || fromInterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown;
			bool now_portrait = this.InterfaceOrientation == UIInterfaceOrientation.Portrait || this.InterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown;
			if (was_portrait != now_portrait) {
				Utility.reshow_popovers_and_action_sheets ();
				//if( popover != null )
				//	popover.PresentFromRect(popover_origin.Bounds, popover_origin, UIPopoverArrowDirection.Any, true);
				sheet_shower.DidRotate (fromInterfaceOrientation);
			}
		}
		private void set_navbar(bool animated)
		{
			if (!animated) {
				//panelDocuments.Hidden = !showing_scroll;
				//panelSheet.Hidden = showing_scroll;
				if( doc_shower != null )
					doc_shower.panelDocuments.Alpha = showing_scroll ? 1 : 0;
				sheet_shower.panelSheet.Alpha = !showing_scroll ? 1 : 0;
			} else {
				UIView.Animate (0.5f, delegate{
					if( doc_shower != null )
						doc_shower.panelDocuments.Alpha = showing_scroll ? 1 : 0;
					sheet_shower.panelSheet.Alpha = !showing_scroll ? 1 : 0;
				}, delegate {
				});
			}
			if (showing_scroll) {
				NavigationItem.TitleView = null;
				NavigationItem.Title = i18n.get("DocumentsTitle");
				NavigationItem.SetRightBarButtonItem(null, animated);
				//NavigationItem.SetLeftBarButtonItem (done_left, animated);
			} else {
				NavigationItem.TitleView = sheet_shower.title_button;
				NavigationItem.Title = i18n.get ("SheetTitle");// sheet_shower.title_button.button.Title(UIControlState.Normal); // TODO
				NavigationItem.SetRightBarButtonItem(plus_right, animated);
				//NavigationItem.SetLeftBarButtonItem (null, animated);
			}
		}
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		public override void ViewDidLoad ()
		{
			AppDelegate.ReportTime("SheetVC 1");
			base.ViewDidLoad ();

			AppDelegate.ReportTime("SheetVC 2");
			View.BackgroundColor = AppDelegate.app.dark_background_color;

			AppDelegate.ReportTime("SheetVC 3");

			scrollDocs.ScrollsToTop = false;

			AppDelegate.ReportTime("SheetVC 4");
			AppDelegate.ReportTime("SheetVC 4.5");
			AppDelegate.ReportTime("SheetVC 5");


			AppDelegate.ReportTime("SheetVC 6");
			sheet_shower.ViewDidLoad ();
			AppDelegate.ReportTime("SheetVC 7");

			set_navbar (false);
			AppDelegate.ReportTime("SheetVC 8");

//			layout_all_document_views ();
			AppDelegate.ReportTime("SheetVC 9");

			AppDelegate.app.docs.anything_changed += (docs, e) => {
				if( AppDelegate.app.docs.documents.IndexOf(sheet_shower.doc) == -1){
					if (appear_counter != 0 && !showing_scroll)
						animate_to_scroll (true);
				}
				if( showing_scroll ) {
					doc_shower.send_anything_changed_to_document_views();
					doc_shower.layout_all_document_views (null);
				}
				//if( e.originator != this ){
				//start_load_sequence();
				//}
			};
			AppDelegate.ReportTime("SheetVC 10");

			if (AppDelegate.app.docs.selected_doc == null) {
				animate_to_scroll (false);
			}
			AppDelegate.ReportTime("SheetVC 11");
		}
		private int appear_counter = 0;
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			sheet_shower.ViewWillAppear ();
			appear_counter += 1;
			Console.WriteLine ("SheetVC.appear_counter=", appear_counter);
		}
		public override void ViewWillDisappear (bool animated)
		{
			appear_counter -= 1;
			Console.WriteLine ("SheetVC.appear_counter=", appear_counter);
			base.ViewWillDisappear (animated);
		}
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			if( AppDelegate.app.docs.documents.IndexOf(sheet_shower.doc) == -1){
				if (!showing_scroll)
					animate_to_scroll (true);
			}
		}
		public void AppDidEnterForeground()
		{
			sheet_shower.AppDidEnterForeground ();
		}
		public override void ViewWillLayoutSubviews ()
		{
			//Console.WriteLine ("ViewWillLayoutSubviews 1 X={0} Width={1}", View.Frame.X, View.Frame.Width);
			base.ViewWillLayoutSubviews ();
			//Console.WriteLine ("ViewWillLayoutSubviews 2 X={0} Width={1}", View.Frame.X, View.Frame.Width);
			sheet_shower.viewWillLayoutSubviews ();

			if( doc_shower != null ) {
				doc_shower.ViewWillLayoutSubviews ();
			}
		}
/*		void start_load_sequence()
		{
			//if (AppDelegate.app.docs.show_new_documents) {
				if (!showing_scroll)
					animate_to_scroll (true);
				else
					doc_shower.layout_all_document_views (null);
			//}
		}*/
		CGRect currentDocViewParagonBounds()
		{
			CGRect rr = View.Bounds;
			rr.Height += AppDelegate.NOTEBOOK_HEADER_HEIGHT - BottomPanelView.PANEL_HEIGHT;
			return rr;
		}
		CGPoint currentDocViewParagonCenter()
		{
			CGRect rr = View.Bounds;
			rr.Y -= AppDelegate.NOTEBOOK_HEADER_HEIGHT;
			rr.Height += AppDelegate.NOTEBOOK_HEADER_HEIGHT - BottomPanelView.PANEL_HEIGHT;
//			rr.Height -= BottomPanelView.PANEL_HEIGHT;
			return new CGPoint (rr.X + rr.Width/2, rr.Y + rr.Height/2);
		}
		void animate_back()
		{
			if (!showing_scroll)
				return;
			Console.WriteLine ("animate_back");
			showing_scroll = false;
			set_navbar (true);

			AppDelegate.app.docs.select_document(doc_shower.get_doc_from_scroll_pos(), null);
			sheet_shower.select_doc( AppDelegate.app.docs.selected_doc );
			if (AppDelegate.app.accounts_vc != null)
				AppDelegate.app.accounts_vc.select_doc ( AppDelegate.app.docs.selected_doc );
			AppDelegate.app.docs.send_anything_changed (null);

			CGPoint new_c = scrollDocs.Frame.Location + new CGSize (scrollDocs.Frame.Width * 0.5f, doc_shower.new_document_view.Center.Y);;

			currentDocView.Bounds = currentDocViewParagonBounds();
			currentDocView.Center = new_c;
			currentDocView.Transform = CGAffineTransform.MakeScale (DOC_SCALE, DOC_SCALE);

			//float delta = 0;//currentDocViewParagon.Bounds.Height - currentDocView.Bounds.Height;

			int ind = AppDelegate.app.docs.documents.IndexOf (AppDelegate.app.docs.selected_doc);
			DocumentView dv = doc_shower.document_views [ind];
			UIView dvTP = dv.getTablePlace ();
			UIGraphics.BeginImageContextWithOptions(dvTP.Bounds.Size, dvTP.Opaque, 0);
			dvTP.Layer.RenderInContext(UIGraphics.GetCurrentContext());
			UIImage img = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			sheet_shower.expenseTable.Hidden = true;
			expenseTableCover.Hidden = false;
			expenseTableCover.Image = img;

			currentDocView.Hidden = false;

			dv.Hidden = true;
			UIView.Animate (0.5, delegate{
//				expenseTableCover.Alpha = 0;
				currentDocView.Transform = CGAffineTransform.MakeIdentity();
				currentDocView.Bounds = currentDocViewParagonBounds();
				currentDocView.Center = currentDocViewParagonCenter();
			}, delegate{
				expenseTableCover.RemoveFromSuperview();
				expenseTableCover = null;
				dv.Hidden = false;
			});
			NSTimer.CreateScheduledTimer (0.05, delegate {
				UIView.Transition (this.expenseTableCover, this.sheet_shower.expenseTable, 0.4, UIViewAnimationOptions.TransitionCurlUp | UIViewAnimationOptions.ShowHideTransitionViews,
				                   null);
			});
			//AppDelegate.app.docs.sync_some_document(true, null);
		}
		void update_on_scroll_from_doc_shower(Document update_doc)
		{
			if (!showing_scroll)
				return;
			if (AppDelegate.app.accounts_vc != null)
				AppDelegate.app.accounts_vc.select_doc (update_doc);
			//if (AppDelegate.docs.selected_doc == update_doc)
			//	return;
			//AppDelegate.docs.select_document(update_doc, null);
		}
		void animate_to_scroll(bool animated)
		{
			if (showing_scroll)
				return;
			if( doc_shower == null ){
				doc_shower = new DocumentsShower (this);
				doc_shower.ViewDidLoad ();
			}
			doc_shower.send_anything_changed_to_document_views();
			doc_shower.layout_all_document_views (AppDelegate.app.docs.selected_doc);
			Console.WriteLine ("animate_to_scroll");
			showing_scroll = true;
			set_navbar (animated);
			int ind = AppDelegate.app.docs.documents.IndexOf (AppDelegate.app.docs.selected_doc);
//			scrollDocs.ContentOffset = new PointF(scrollDocs.Frame.Width*ind, 0);

			expenseTableCover = new UIImageView(sheet_shower.expenseTable.Frame);
			float delta = 0;//currentDocViewParagon.Bounds.Height - currentDocView.Bounds.Height;
			//expenseTableCover.Frame = currentDocViewParagon.Bounds;
			CGPoint new_c = scrollDocs.Frame.Location + new CGSize (scrollDocs.Frame.Width * 0.5f, doc_shower.new_document_view.Center.Y - delta * 0.25f);;
			DocumentView dv = null;
			if( ind != -1 )
				dv = doc_shower.document_views [ind];
			{
				//new_c = scrollDocs.Frame.Location + new SizeF (scrollDocs.Frame.Width * 0.5f, dv.Center.Y - delta * 0.25f); // TODO - the same pos, remove line
				//RectangleF r = dv.Bounds;
				UIView dvTP = dv != null ? dv.getTablePlace () : sheet_shower.expenseTable;
				UIGraphics.BeginImageContextWithOptions(dvTP.Bounds.Size, dvTP.Opaque, 0);
				dvTP.Layer.RenderInContext(UIGraphics.GetCurrentContext());
				UIImage img = UIGraphics.GetImageFromCurrentImageContext();
				UIGraphics.EndImageContext();
				expenseTableCover.Image = img;
			}
			//r = currentDocView.Frame;
			//expenseTableCover = new UIImageView (img);
			//			bim.Center = View.Center;
			expenseTableCover.ContentMode = UIViewContentMode.Top;
			expenseTableCover.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			expenseTableCover.Hidden = true;
			currentDocView.Add (expenseTableCover);
//			expenseTableCover.Alpha = 0;

			if (animated) {
				if( dv != null )
					dv.Hidden = true;
				UIView.Animate (0.5, delegate {
					if( ind != -1 ) {
						currentDocView.Center = new_c;
						currentDocView.Transform = CGAffineTransform.MakeScale (DOC_SCALE, DOC_SCALE);
					}else{
						CGPoint trash_pos = View.ConvertPointFromView(doc_shower.btnTrash.Center, doc_shower.btnTrash.Superview);
						currentDocView.Center = trash_pos;
						currentDocView.Transform = CGAffineTransform.MakeScale (0.05f, 0.05f);
					}
					//				expenseTableCover.Alpha = 1;
				}, delegate {
					currentDocView.Hidden = true;
					if( dv != null )
						dv.Hidden = false;
					//doc_shower.layout_all_document_views ();
					//scrollDocs.ShowsHorizontalScrollIndicator = true;
					//scrollDocs.FlashScrollIndicators();
				});
				if( ind != -1 )
					NSTimer.CreateScheduledTimer (0.05, delegate {
						UIView.Transition (this.sheet_shower.expenseTable, this.expenseTableCover, 0.4, UIViewAnimationOptions.TransitionCurlDown | UIViewAnimationOptions.ShowHideTransitionViews, 
					                   null);
					});
			} else {
				//currentDocView.Center = new_c;
				//currentDocView.Transform = CGAffineTransform.MakeScale (DOC_SCALE, DOC_SCALE);
				currentDocView.Hidden = true;
				//tablePlace.Hidden = true;
				//expenseTableCover.Hidden = false;
				//doc_shower.layout_all_document_views ();
			}
		}
	}
}

