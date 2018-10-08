namespace DavidDostal.FSharpScan
open WIA

/// Provides information about connected scanners.
type DeviceManager() =
    let deviceManager = WiaInterop.initialize()
    let dialogs = CommonDialogClass()

    /// Get a list of all connected scanners.
    member this.ConnectedScanners() =
        WiaInterop.deviceInfos deviceManager
        |> Seq.map (fun deviceInfo -> deviceInfo.Connect())
        |> Seq.map (fun device -> Scanner(device, this))
    
    /// Register an event listener for events related to scanning.
    /// To unregister the event later save the result of this call.
    member __.RegisterEvent (eventId: string) (handler: string -> string -> unit) =
        let eventHandler =
            new _IDeviceManagerEvents_OnEventEventHandler(
                fun event device item -> if event = eventId then handler device item else ())
        deviceManager.add_OnEvent(eventHandler);
        eventHandler

    /// Unregister an event handler returned by the RegisterEvent method.
    member __.UnregisterEvent handler =
        deviceManager.remove_OnEvent handler
    
    /// Unregister an event handler reacting to multiple events.
    member this.UnregisterEvents handlers =
        Seq.iter this.UnregisterEvent handlers
    
    /// Register an event handler to be called when a scanner is connected.
    member this.RegisterScannerConnected handler =
        this.RegisterEvent EventID.wiaEventDeviceConnected handler
    
    /// Register an event handler to be called when a scanner gets disconnected.
    member this.RegiserScannerDisconnected handler =
        this.RegisterEvent EventID.wiaEventDeviceDisconnected handler
    
    /// Register an event handler to be called when a scan is initiated directly from the scanner.
    member this.RegisterIncomingScan handler =
        [ this.RegisterEvent EventID.wiaEventScanImage handler;
          this.RegisterEvent EventID.wiaEventScanImage2 handler;
          this.RegisterEvent EventID.wiaEventScanImage3 handler;
          this.RegisterEvent EventID.wiaEventScanImage4 handler; ]

    /// Show dialog with simplified scanner settings and option to scan.
    member __.ScanDialog() =
        dialogs.ShowAcquireImage(WiaDeviceType.ScannerDeviceType)
        |> WiaInterop.toBitmap

    /// Show dialog for choosing between installed scanners.
    /// Returns default scanner if only one is available.
    member this.ScannerSelectDialog() =
        dialogs.ShowSelectDevice(WiaDeviceType.ScannerDeviceType)
        |> (fun device -> Scanner(device, this))
    
    /// Provides access to the internal WIA DeviceManager instance.
    member __.__WiaDeviceManager = deviceManager

    interface WiaEventRegisterer with
        member this.RegisterEvent eventId handler = this.RegisterEvent eventId handler
        member this.UnregisterEvent handler = this.UnregisterEvent handler