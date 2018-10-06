namespace DavidDostal.FSharpScan
open WIA

type DeviceManager() =
    let deviceManager = WiaInterop.initialize()
    let dialogs = CommonDialogClass()

    member __.ConnectedScanners() =
        WiaInterop.deviceInfos deviceManager
        |> Seq.map WiaInterop.connect
        |> Seq.map Scanner

    member __.RegisterEvent eventId handler =
        let eventHandler =
            new _IDeviceManagerEvents_OnEventEventHandler(
                fun event device item -> if event = eventId then handler device item else ())
        deviceManager.add_OnEvent(eventHandler);
        eventHandler

    member __.UnregisterEvent handler =
        deviceManager.remove_OnEvent handler

    member this.UnregisterEvents handlers =
        Seq.iter this.UnregisterEvent handlers

    member this.RegisterScannerConnected handler =
        this.RegisterEvent EventID.wiaEventDeviceConnected handler

    member this.RegiserScannerDisconnected handler =
        this.RegisterEvent EventID.wiaEventDeviceDisconnected handler

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
    member __.ScannerSelectDialog() =
        dialogs.ShowSelectDevice(WiaDeviceType.ScannerDeviceType)
        |> Scanner