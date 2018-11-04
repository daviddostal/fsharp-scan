namespace DavidDostal.FSharpScan
open WIA
open System.Collections.Generic
open System.Linq

/// Provides information about connected scanners.
type DeviceManager() =
    let deviceManager = WiaInterop.initialize()
    do WiaInterop.registerKnownEvents deviceManager
    let dialogs = CommonDialogClass()
    let scanners =
        (WiaInterop.deviceInfos deviceManager
        |> Seq.map (fun deviceInfo -> deviceInfo.Connect())
        |> Seq.map Scanner).ToDictionary(fun d -> d.__WiaDevice.DeviceID)

    let deviceConnected =
        WiaInterop.addEventHandler deviceManager EventID.wiaEventDeviceConnected
            (fun deviceId itemId ->
                let scanner = Scanner((WiaInterop.deviceInfoFromId deviceManager deviceId).Connect())
                scanners.Add(deviceId, scanner))

    let deviceDisconnected =
        WiaInterop.addEventHandler deviceManager EventID.wiaEventDeviceDisconnected
            (fun deviceId itemId -> scanners.Remove(deviceId) |> ignore)

    /// Get a list of all connected scanners.
    member this.ConnectedScanners() = scanners.Values |> seq
        //WiaInterop.deviceInfos deviceManager
        //|> Seq.map (fun deviceInfo -> deviceInfo.Connect())
        //|> Seq.map (fun device -> Scanner(device))
    
    /// Register an event listener for events related to scanning.
    /// To unregister the event later save the result of this call.
    member __.RegisterEvent (eventId: string) (handler: Scanner -> ImageSource option -> unit) =
        WiaInterop.addEventHandler deviceManager eventId (fun deviceId itemId -> 
            let scanner = scanners.[deviceId]
            let item = scanner.ImageSources() |> Seq.tryFind(fun i -> i.__WiaItem.ItemID = itemId)
            handler scanner item)

    /// Unregister an event handler returned by the RegisterEvent method.
    member __.UnregisterEvent handler =
        deviceManager.remove_OnEvent handler
    
    /// Unregister an event handler reacting to multiple events.
    member this.UnregisterEvents handlers =
        Seq.iter this.UnregisterEvent handlers
    
    /// Register an event handler to be called when a scanner is connected.
    member this.RegisterScannerConnected handler =
        this.RegisterEvent EventID.wiaEventDeviceConnected (fun scanner _ -> handler scanner)
    
    /// Register an event handler to be called when a scanner gets disconnected.
    member this.RegiserScannerDisconnected handler =
        this.RegisterEvent EventID.wiaEventDeviceDisconnected (fun scanner _ -> handler scanner)
    
    /// Register an event handler to be called when a scan is initiated directly from the scanner.
    member this.RegisterIncomingScan handler =
        [ this.RegisterEvent EventID.wiaEventScanImage handler;
          this.RegisterEvent EventID.wiaEventScanImage2 handler;
          this.RegisterEvent EventID.wiaEventScanImage3 handler;
          this.RegisterEvent EventID.wiaEventScanImage4 handler; ]

    /// Show dialog with simplified scanner settings and option to scan.
    member __.ScanDialog() =
        dialogs.ShowAcquireImage(WiaDeviceType.ScannerDeviceType, CancelError=false)
        |> WiaInterop.toBitmap

    /// Show dialog for choosing between installed scanners.
    /// Returns default scanner if only one is available.
    member this.ScannerSelectDialog() =
        dialogs.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, CancelError=false)
        |> (fun device -> Scanner(device))
    
    /// Provides access to the internal WIA DeviceManager instance.
    member __.__WiaDeviceManager = deviceManager