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
// registerSessionReadyListener, so the sessionReady callback can reach Unity.
// Called synchronously from Unity's main thread during init, so we're
// genuinely on the main actor already — assumeIsolated asserts that rather
// than hopping.
@_cdecl("_setRPCEventHandler")
public func _setRPCEventHandler(_ objectName: UnsafePointer<CChar>?) {
#if canImport(AppsFlyerRPC)
    let callbackObject = objectName.map { String(cString: $0) } ?? ""
    MainActor.assumeIsolated {
        AppsFlyerRPCBridge.shared.setEventHandler { jsonEvent in
            if !callbackObject.isEmpty {
                UnitySendMessageC(callbackObject, "onRPCEvent", jsonEvent)
            }
        }
    }
#endif
}

// Fire-and-forget: used for setter methods that need no return value.
// AppsFlyerRPCBridge is @MainActor-isolated; dispatch to main queue so the
// Swift actor isolation is respected and async callbacks (e.g. sessionReady) fire.
@_cdecl("_afFireJson")
public func _afFireJson(_ jsonRequest: UnsafePointer<CChar>?) {
#if canImport(AppsFlyerRPC)
    let requestStr = jsonRequest.map { String(cString: $0) } ?? "{}"
    DispatchQueue.main.async {
        MainActor.assumeIsolated {
            AppsFlyerRPCBridge.shared.executeJson(requestStr) { _ in }
        }
    }
#endif
}

// Synchronous: used for getter methods (e.g. getAppsFlyerUID) that return data.
// Blocks a background thread — NEVER call from the main Unity thread (doing so
// deadlocks: AppsFlyerRPCBridge's internal Task inherits @MainActor isolation,
// so its completion can only run once the main thread is free — but the main
// thread would be sitting on the semaphore below instead).
@_cdecl("_afExecuteJson")
public func _afExecuteJson(_ jsonRequest: UnsafePointer<CChar>?) -> UnsafeMutablePointer<CChar>? {
#if canImport(AppsFlyerRPC)
    let requestStr = jsonRequest.map { String(cString: $0) } ?? "{}"
    let semaphore = DispatchSemaphore(value: 0)
    var response: String?

    // Task creation is legal from any thread; the @MainActor-isolated call
    // itself always runs on the main actor's executor, regardless of which
    // thread this closure happens to execute on.
    Task { @MainActor in
        AppsFlyerRPCBridge.shared.executeJson(requestStr) { jsonResponse in
            response = jsonResponse
            semaphore.signal()
        }
    }

    _ = semaphore.wait(timeout: .now() + 5)

    return strdup(response ?? "{\"id\":\"rpc-error\",\"error\":{\"code\":-1,\"message\":\"No response from AppsFlyerRPC\"}}")
#else
    return strdup("{\"id\":\"rpc-stub\",\"error\":{\"code\":-2,\"message\":\"AppsFlyerRPC framework not linked\"}}")
#endif
}
