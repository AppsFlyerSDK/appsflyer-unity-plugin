import Foundation
import StoreKit

#if canImport(PurchaseConnector)
import PurchaseConnector

@available(iOS 15.0, *)
@objc
public class AFUnityStoreKit2Bridge: NSObject {
    @objc
    public static func fetchAFSDKTransactionSK2(withTransactionId transactionId: String, completion: @escaping (AFSDKTransactionSK2?) -> Void) {
        guard let transactionIdUInt64 = UInt64(transactionId) else {
            print("Invalid transaction ID format.")
            completion(nil)
            return
        }
        Task {
            for await result in StoreKit.Transaction.all {
                if case .verified(let transaction) = result, transaction.id == transactionIdUInt64 {
                    let afTransaction = AFSDKTransactionSK2(transaction: transaction)
                    DispatchQueue.main.async {
                        completion(afTransaction)
                    }
                    return
                }
            }
            DispatchQueue.main.async {
                completion(nil)
            }
        }
    }

    @objc
    public static func extractSK2ProductInfo(_ products: [AFSDKProductSK2]) -> NSArray {
        var result: [[String: Any]] = []

        for product in products {
            if let swiftProduct = Mirror(reflecting: product).children.first(where: { $0.label == "product" })?.value {
                let productId = (swiftProduct as? NSObject)?.value(forKey: "id") as? String ?? ""
                let title = (swiftProduct as? NSObject)?.value(forKey: "displayName") as? String ?? ""
                let desc = (swiftProduct as? NSObject)?.value(forKey: "description") as? String ?? ""
                let price = (swiftProduct as? NSObject)?.value(forKey: "price") as? NSDecimalNumber ?? 0

                result.append([
                    "productIdentifier": productId,
                    "localizedTitle": title,
                    "localizedDescription": desc,
                    "price": price
                ])
            }
        }

        return result as NSArray
    }
    
    @objc
    public static func extractSK2TransactionInfo(_ transactions: [AFSDKTransactionSK2]) -> NSArray {
        var result: [[String: Any]] = []

        for txn in transactions {
            guard let mirrorChild = Mirror(reflecting: txn).children.first(where: { $0.label == "transaction" }),
                  let swiftTxn = mirrorChild.value as? StoreKit.Transaction else {
                continue
            }

            let transactionId = "\(swiftTxn.id)"
            let date = NSNumber(value: swiftTxn.purchaseDate.timeIntervalSince1970)

            result.append([
                "transactionIdentifier": transactionId,
                "transactionState": "verified", // or skip this line
                "transactionDate": date
            ])
        }

        return result as NSArray
    }
}
#endif
