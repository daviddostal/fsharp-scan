#r ".\obj\Debug\Interop.WIA.dll"
#r ".\obj\Debug\FsharpScan.dll"
open DavidDostal.FSharpScan

/// Basic scanning with custom settings.
let example1() =
    let scanner = DeviceManager().ConnectedScanners() |> Seq.head
    let imageSource = scanner.ImageSources() |> Seq.head
    let settings =  { imageSource.Settings with
                          colorMode = ColorMode.BlackAndWhite;
                          verticalResolution = 300;
                          horizontalResolution = 300; }
    imageSource.ScanWithSettings settings


/// Listen for events related to scanning.
let example2() =
    let deviceManager = DeviceManager()
    let scannerConnected =
        deviceManager.RegisterScannerConnected (
            fun scanner -> printfn "A new scanner was connected: %s" scanner.Properties.name)
    // ...
    deviceManager.UnregisterEvent scannerConnected


/// Set additional WIA properties yourself.
let example3() =
    let scanner = DeviceManager().ConnectedScanners() |> Seq.head
    scanner.SetProperty 3100 1


/// Show system dialogs.
let example4 = 
    let scanner = DeviceManager().ScannerSelectDialog()
    let source = scanner.ItemSelectDialog() |> Seq.head
    let image = source.ScanProgressDialog()
    image


/// Access WIA objects directly.
let example5 =
    let wiaManager = DeviceManager().__WiaDeviceManager
    let deviceInfo = wiaManager.DeviceInfos.[ref (1 :> obj)]
    if deviceInfo.Type = WIA.WiaDeviceType.ScannerDeviceType
        then deviceInfo.DeviceID
        else "Device is not a scanner."