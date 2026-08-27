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
    AppsFlyerRPCBridge.shared.setEventHandler { jsonEvent in
        if !callbackObject.isEmpty {
            UnitySendMessageC(callbackObject, "onRPCEvent", jsonEvent)
        }
    }
#endif
}

// Fire-and-forget: used for setter methods that need no return value. AppsFlyerRPCBridge is not
// actor-isolated (see _setRPCEventHandler), so no dispatch/hop is needed - call directly.
@_cdecl("_afFireJson")
public func _afFireJson(_ jsonRequest: UnsafePointer<CChar>?) {
#if canImport(AppsFlyerRPC)
    let requestStr = jsonRequest.map { String(cString: $0) } ?? "{}"
    AppsFlyerRPCBridge.shared.executeJson(requestStr) { _ in }
#endif
}

// Synchronous: used for getter methods (e.g. getAppsFlyerUID) that return data. Blocks the
// calling thread on a semaphore until the completion handler fires. Previously this wrapped the
// call in `Task { @MainActor in ... }`, which - if called from Unity's main thread - could never
// start (the Task needs the main run loop free to begin executing, but the main thread would be
// sitting on the semaphore.wait() below instead), stalling for the full timeout. That MainActor
// hop was never actually required: AppsFlyerRPCBridge.executeJson is not actor-isolated (see
// _setRPCEventHandler), so it's safe to call directly, synchronously, from any thread including
// Unity's main thread.
@_cdecl("_afExecuteJson")
public func _afExecuteJson(_ jsonRequest: UnsafePointer<CChar>?) -> UnsafeMutablePointer<CChar>? {
#if canImport(AppsFlyerRPC)
    let requestStr = jsonRequest.map { String(cString: $0) } ?? "{}"
    let semaphore = DispatchSemaphore(value: 0)
    var response: String?

    AppsFlyerRPCBridge.shared.executeJson(requestStr) { jsonResponse in
        response = jsonResponse
        semaphore.signal()
    }

    _ = semaphore.wait(timeout: .now() + 5)

    return strdup(response ?? "{\"id\":\"rpc-error\",\"error\":{\"code\":-1,\"message\":\"No response from AppsFlyerRPC\"}}")
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
