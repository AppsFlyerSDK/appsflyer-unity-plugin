//
//  AppsFlyerRPCWrapper.swift
//  Bridges C# DllImport _afExecuteJson -> AppsFlyerRPCBridge (Swift).
//  Uses a blocking semaphore so the P/Invoke boundary stays synchronous.
//  Gracefully stubs when AppsFlyerRPC framework is not linked.
//

import Foundation

#if canImport(AppsFlyerRPC)
import AppsFlyerRPC
#endif

// Unity does not expose UnitySendMessage to Swift via a bridging header,
// so bind directly to the C symbol declared in UnityInternalInterface.h.
@_silgen_name("UnitySendMessage")
private func UnitySendMessageC(_ obj: UnsafePointer<CChar>?, _ method: UnsafePointer<CChar>?, _ arg: UnsafePointer<CChar>?)

// AppsFlyerRPCBridge's own thread-safety under concurrent calls is undocumented (it's a plain,
// non-actor-isolated NSObject in the closed-source AppsFlyerRPC framework - see the comment on
// _setRPCEventHandler below). Serializing all calls into it here restores the incidental
// serialization that the old (deadlocking) `Task { @MainActor in ... }` wrapping used to provide,
// without reintroducing the deadlock: this is a private queue, not the main run loop, so it's
// never blocked waiting on Unity's main thread.
private let rpcQueue = DispatchQueue(label: "com.appsflyer.rpcbridge")

// Wires the RPC -> Unity event channel. Must be called during SDK init, before
// registerSessionReadyListener/start, so the sessionReady/conversion-data callback can reach
// Unity. AppsFlyerRPCBridge (verified against AppsFlyerRPC.xcframework's shipped
// .swiftinterface: `@objcMembers final public class AppsFlyerRPCBridge : NSObject`, no actor
// isolation on the class or on setEventHandler/executeJson) is a plain, non-isolated NSObject -
// no MainActor hop is required or was ever needed here. Calling it synchronously, directly on
// whichever thread this is invoked from, also preserves ordering with the immediately-following
// blocking init RPC/registerSessionReadyListener/start on the same stack frame.
@_cdecl("_setRPCEventHandler")
public func _setRPCEventHandler(_ objectName: UnsafePointer<CChar>?) {
#if canImport(AppsFlyerRPC)
    let callbackObject = objectName.map { String(cString: $0) } ?? ""
    rpcQueue.sync {
        AppsFlyerRPCBridge.shared.setEventHandler { jsonEvent in
            if !callbackObject.isEmpty {
                UnitySendMessageC(callbackObject, "onRPCEvent", jsonEvent)
            }
        }
    }
#endif
}

// Fire-and-forget: used for setter methods that need no return value. Routed through rpcQueue to
// serialize entry into AppsFlyerRPCBridge (see rpcQueue's declaration above).
@_cdecl("_afFireJson")
public func _afFireJson(_ jsonRequest: UnsafePointer<CChar>?) {
#if canImport(AppsFlyerRPC)
    let requestStr = jsonRequest.map { String(cString: $0) } ?? "{}"
    rpcQueue.async {
        AppsFlyerRPCBridge.shared.executeJson(requestStr) { jsonResponse in
#if DEBUG
            if jsonResponse.contains("\"error\"") {
                print("AppsFlyer: fire-and-forget RPC call returned an error: \(jsonResponse)")
            }
#endif
        }
    }
#endif
}

// Synchronous: used for getter methods (e.g. getAppsFlyerUID) that return data. Blocks the
// calling thread on a semaphore until the completion handler fires. Previously this wrapped the
// call in `Task { @MainActor in ... }`, which - if called from Unity's main thread - could never
// start (the Task needs the main run loop free to begin executing, but the main thread would be
// sitting on the semaphore.wait() below instead), stalling for the full timeout. That MainActor
// hop was never actually required: AppsFlyerRPCBridge.executeJson is not actor-isolated (see
// _setRPCEventHandler), so it's safe to call directly, synchronously, from any thread including
// Unity's main thread. The call itself is routed through rpcQueue (see its declaration above) to
// serialize concurrent entry into the bridge; only the executeJson call is queued, not the
// semaphore.wait() below, so concurrent *Async getters queue briefly on entry but still wait on
// their own thread, not each other's.
@_cdecl("_afExecuteJson")
public func _afExecuteJson(_ jsonRequest: UnsafePointer<CChar>?) -> UnsafeMutablePointer<CChar>? {
#if canImport(AppsFlyerRPC)
    let requestStr = jsonRequest.map { String(cString: $0) } ?? "{}"
    let semaphore = DispatchSemaphore(value: 0)
    let lock = NSLock()
    var response: String?

    rpcQueue.async {
        AppsFlyerRPCBridge.shared.executeJson(requestStr) { jsonResponse in
            lock.lock()
            response = jsonResponse
            lock.unlock()
            semaphore.signal()
        }
    }

    let waitResult = semaphore.wait(timeout: .now() + 5)
    if waitResult == .timedOut {
        // Not a cancellation — the underlying executeJson call keeps running and will still write to
        // `response` and signal `semaphore` whenever it eventually completes; both are simply
        // orphaned at that point. Logged so a native-side latency spike is distinguishable from a
        // genuine bridge failure instead of surfacing as an identical generic error to the caller.
        print("AppsFlyer: RPC call timed out waiting for native response — \(requestStr)")
    }

    lock.lock()
    let result = response
    lock.unlock()
    return strdup(result ?? "{\"id\":\"rpc-error\",\"error\":{\"code\":-1,\"message\":\"No response from AppsFlyerRPC\"}}")
#else
    return strdup("{\"id\":\"rpc-stub\",\"error\":{\"code\":-2,\"message\":\"AppsFlyerRPC framework not linked\"}}")
#endif
}

// Frees a buffer returned by _afExecuteJson. The C# side must call this after marshaling the
// response to a managed string — strdup's allocation is otherwise never released.
@_cdecl("_afFreeCString")
public func _afFreeCString(_ ptr: UnsafeMutablePointer<CChar>?) {
    free(ptr)
}
