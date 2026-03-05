using System;
using Foundation;
using StoreKit;
using UIKit;
using SmartBudgetCommon;
using System.Text;
using Security;
using System.Threading;

namespace SmartBudgetiOS
{
	public partial class AppDelegate
	{
		//public bool full_version { get {
		//		NSData d = NSUserDefaults.StandardUserDefaults.DataForKey(full_version_receipt_key);
		//		return d != null && d.Length > 0;
		//	}
		//}
		// TODO - remove in production
		public bool processing_restore { get; private set; }
		public bool processing_payment { get {
				return SKPaymentQueue.DefaultQueue.Transactions.Length != 0;
			}
		}
//		const string full_version_receipt_key = "FullVersionReceipt";
//		const string full_version_receipt_restoration_key = "FullVersionReceiptRestoration";
		const string full_version_product_name = "com.smartbudgetapp.sb2.full_version";
		const string full_version_sale_product_name = "com.smartbudgetapp.sb2.full_version_sale";
		private SKProduct full_version_product;
		private SKProduct full_version_sale_product;
		public bool ok_for_sale { get; private set; }
		public string get_full_version_price(bool sale)
		{
			//return sale ? "$0.99" : "$2.99";
			SKProduct prod = sale ? full_version_sale_product : full_version_product;
			if (prod == null)
				return null; // Will hide price in this case
			NSNumberFormatter nf = new NSNumberFormatter();
			nf.NumberStyle = NSNumberFormatterStyle.Currency;
			nf.Locale = prod.PriceLocale;
			if(prod.Price.NIntValue == prod.Price.NFloatValue)
				nf.MinimumFractionDigits = 0;
			return nf.StringFromNumber( prod.Price );
		}
		private SKProductsRequest full_version_req;
		public void refresh_price() {
			if (docs.full_version)
				return;
			if (full_version_req != null) {
				full_version_req.Cancel ();
				full_version_req = null;
				//return; // In progress
			}
			if( custom_product_observer == null )
				custom_product_observer = new CustomProductObserver ();
			full_version_req = new SKProductsRequest (new NSSet(full_version_product_name, full_version_sale_product_name));
			full_version_req.Delegate = custom_product_observer;
			full_version_req.Start ();
		}
		public void refresh_ok_for_sale()
		{
			if (docs.full_version)
				return;
			//ok_for_sale = true;
			//return;
			var rec = new SecRecord (SecKind.GenericPassword){
				Generic = NSData.FromString ("full_version"),
				Account = "full_version",
				Service = "CommonMoney"
			};

			SecStatusCode res;
			var match = SecKeyChain.QueryAsRecord(rec, out res);
			if (res == SecStatusCode.Success) {
				string string_data = Encoding.UTF8.GetString (Utility.from_nsdata(match.ValueData));
//				Console.WriteLine ("Key found, password is: ({0}) {1}", match.ValueData, string_data);
				ok_for_sale = string_data == "1"; // TODO - check Keychain*/
			}
//			else
//				Console.WriteLine ("Key not found: {0}", res);

			/*					var s = new SecRecord (SecKind.GenericPassword) {
						Account = "EvilTwin",
						Service = "EvilTwin",
						ValueData = NSData.FromString ("my-secret-password"),
						Generic = NSData.FromString ("full_version")
					};

					var err = SecKeyChain.Add (s);
					Console.WriteLine ("Add: {0}", err);*/
		}
		public void startRestoreFullVersion(string from)
		{
			if (processing_restore || processing_payment)
				return;
			if (!SKPaymentQueue.CanMakePayments) {
				new UIAlertView(i18n.get("PURCHASE_FAILED_TITLE"), i18n.get("CANNOT_MAKE_PAYMENTS_TEXT"), null, i18n.get("OK"), null).Show();
				return;
			}
			processing_restore = true;
			FlurryAnalytics.Flurry.LogEvent("Restore", NSDictionary.FromObjectsAndKeys(new object[]{"Start"}, new object[]{"Step"}));
			SKPaymentQueue.DefaultQueue.RestoreCompletedTransactions ();
			AppDelegate.app.docs.send_anything_changed (null);
		}
		public void startPurchaseFullVersion(string from)
		{
			if (processing_restore || processing_payment)
				return;
			if (!SKPaymentQueue.CanMakePayments) {
				new UIAlertView(i18n.get("PURCHASE_FAILED_TITLE"), i18n.get("CANNOT_MAKE_PAYMENTS_TEXT"), null, i18n.get("OK"), null).Show();
				FlurryAnalytics.Flurry.LogEvent(ok_for_sale ? "PurchaseSale" : "Purchase", NSDictionary.FromObjectsAndKeys(new object[]{"Disabled"}, new object[]{"Step"}));
				return;
			}
			SKProduct prod = ok_for_sale ? full_version_sale_product : full_version_product;
			if (prod == null) {
				new UIAlertView (i18n.get("PURCHASE_FAILED_TITLE"), i18n.get("ERROR_INTERNET_TEXT").Replace("%@", i18n.get ("ERROR_NO_PRODUCT")), null, i18n.get("OK"), null).Show ();
				FlurryAnalytics.Flurry.LogEvent(ok_for_sale ? "PurchaseSale" : "Purchase", NSDictionary.FromObjectsAndKeys(new object[]{"NoProduct"}, new object[]{"Step"}));
				return;
			}
			FlurryAnalytics.Flurry.LogEvent(ok_for_sale ? "PurchaseSale" : "Purchase", NSDictionary.FromObjectsAndKeys(new object[]{"Start"}, new object[]{"Step"}));
			SKPaymentQueue.DefaultQueue.AddPayment (SKPayment.PaymentWithProduct(prod));
			AppDelegate.app.docs.send_anything_changed (null);
		}
		private CustomProductObserver custom_product_observer;
		private CustomPaymentObserver custom_payment_observer;
		class CustomPaymentObserver : SKPaymentTransactionObserver {
			public override void UpdatedTransactions (SKPaymentQueue queue, SKPaymentTransaction[] transactions)
			{
				Console.WriteLine ("UpdatedTransactions");
				foreach (SKPaymentTransaction transaction in transactions)
				{
					switch (transaction.TransactionState)
					{
					case SKPaymentTransactionState.Purchased:
						AppDelegate.app.docs.set_full_version_receipt (Utility.from_nsdata(transaction.TransactionReceipt));
						//NSUserDefaults.StandardUserDefaults.SetValueForKey (transaction.TransactionReceipt, new NSString(AppDelegateFullVersion.full_version_receipt_key));
						//NSUserDefaults.StandardUserDefaults.Synchronize ();
						SKPaymentQueue.DefaultQueue.FinishTransaction (transaction);
						FlurryAnalytics.Flurry.LogEvent(AppDelegate.app.ok_for_sale ? "PurchaseSale" : "Purchase", NSDictionary.FromObjectsAndKeys(new object[]{"Success"}, new object[]{"Step"}));
						break;
					case SKPaymentTransactionState.Failed:
						if (transaction.Error.Code != (int)SKError.PaymentCancelled) {
							new UIAlertView (i18n.get("PURCHASE_FAILED_TITLE"), i18n.get("ERROR_INTERNET_TEXT").Replace ("%@", transaction.Error.LocalizedDescription), null, i18n.get("OK"), null).Show ();
							FlurryAnalytics.Flurry.LogEvent(AppDelegate.app.ok_for_sale ? "PurchaseSale" : "Purchase", NSDictionary.FromObjectsAndKeys(new object[]{"Failed"}, new object[]{"Step"}));
						} else {
							FlurryAnalytics.Flurry.LogEvent(AppDelegate.app.ok_for_sale ? "PurchaseSale" : "Purchase", NSDictionary.FromObjectsAndKeys(new object[]{"Cancel"}, new object[]{"Step"}));
						}
						SKPaymentQueue.DefaultQueue.FinishTransaction (transaction);
						break;
					case SKPaymentTransactionState.Restored:
						AppDelegate.app.docs.set_full_version_receipt (Utility.from_nsdata(transaction.TransactionReceipt));
						//NSUserDefaults.StandardUserDefaults.SetValueForKey (transaction.TransactionReceipt, new NSString(AppDelegateFullVersion.full_version_receipt_restoration_key));
						//NSUserDefaults.StandardUserDefaults.SetValueForKey (transaction.OriginalTransaction.TransactionReceipt, new NSString(AppDelegateFullVersion.full_version_receipt_key));
						//NSUserDefaults.StandardUserDefaults.Synchronize ();
						FlurryAnalytics.Flurry.LogEvent("Restore", NSDictionary.FromObjectsAndKeys(new object[]{"Success"}, new object[]{"Step"}));
						SKPaymentQueue.DefaultQueue.FinishTransaction (transaction);
						break;
					}
				}
			}
			public override void RemovedTransactions (SKPaymentQueue queue, SKPaymentTransaction[] transactions)
			{
				// Our state depends on queue size
				AppDelegate.app.docs.send_anything_changed (null);
			}
			public override void PaymentQueueRestoreCompletedTransactionsFinished (SKPaymentQueue queue)
			{
				Console.WriteLine(" ** RESTORE PaymentQueueRestoreCompletedTransactionsFinished ");
				AppDelegate.app.processing_restore = false;
				AppDelegate.app.docs.send_anything_changed (null);
				if (AppDelegate.app.docs.full_version) {
					FlurryAnalytics.Flurry.LogEvent("Restore", NSDictionary.FromObjectsAndKeys(new object[]{"Success"}, new object[]{"Step"}));
				} else {
					new UIAlertView (i18n.get("RESTORE_NOTHING_TITLE"), i18n.get("RESTORE_NOTHING_TEXT"), null, i18n.get("OK"), null).Show ();
					FlurryAnalytics.Flurry.LogEvent("Restore", NSDictionary.FromObjectsAndKeys(new object[]{"FinishNothing"}, new object[]{"Step"}));
				}
			}
			public override void RestoreCompletedTransactionsFailedWithError (SKPaymentQueue queue, NSError error)
			{
				Console.WriteLine(" ** RESTORE RestoreCompletedTransactionsFailedWithError " + error.LocalizedDescription);
				AppDelegate.app.processing_restore = false;
				AppDelegate.app.docs.send_anything_changed (null);
				if (error.Code != (int)SKError.PaymentCancelled) {
					new UIAlertView (i18n.get("RESTORE_FAILED_TITLE"), i18n.get("ERROR_INTERNET_TEXT").Replace("%@", error.LocalizedDescription), null, i18n.get("OK"), null).Show ();
					FlurryAnalytics.Flurry.LogEvent("Restore", NSDictionary.FromObjectsAndKeys(new object[]{"Failed"}, new object[]{"Step"}));
				} else {
					FlurryAnalytics.Flurry.LogEvent("Restore", NSDictionary.FromObjectsAndKeys(new object[]{"Cancel"}, new object[]{"Step"}));
				}
			}
		}
		class CustomProductObserver : SKProductsRequestDelegate {
			// received response to RequestProductData - with price,title,description info
			public override void ReceivedResponse (SKProductsRequest request, SKProductsResponse response)
			{
				foreach (SKProduct p in response.Products) {
					Console.WriteLine("Product id: " + p.ProductIdentifier );
					if (p.ProductIdentifier == AppDelegate.full_version_product_name)
					{
						AppDelegate.app.full_version_product = p;
					}
					if (p.ProductIdentifier == AppDelegate.full_version_sale_product_name)
					{
						AppDelegate.app.full_version_sale_product = p;
					}
				}
				foreach (string invalidProductId in response.InvalidProducts) {
					Console.WriteLine("Invalid product id: " + invalidProductId );
				}
				AppDelegate.app.full_version_req = null;
				AppDelegate.app.docs.send_anything_changed (null);
			}
		}
	}
}

