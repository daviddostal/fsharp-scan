namespace DavidDostal.FSharpScan
module DeviceManager =

    // Initialize a new deviceManager.
    let initialize () = Wia.createWia()

    // Get all connected scanners.
    let scanners devManager = Wia.scanners devManager

    // Get first of connected scanners.
    let defaultScanner devManager = scanners devManager |> Seq.tryHead

    //let watchDeviceConnected watcher (devManager: DeviceManagerClass) =
    //    Wia.registerEvent EventId.DeviceConnected watcher
        
    //let watchDeviceDisconnected watcher devManager =