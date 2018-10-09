# FSharpScan
Simple scanning library for F# built on top of WIA.
The goal of this library is to make basic scanning operations simple,
but still allow easy access to the underlying WIA interface.

Keep in mind this is still work in progress.

## Example usage

Basic scanning with custom settings:
```fsharp
let scanner = DeviceManager().ConnectedScanners() |> Seq.head
let imageSource = scanner.ImageSources() |> Seq.head
let settings =  { imageSource.Settings with
                        colorMode = ColorMode.BlackAndWhite;
                        verticalResolution = 300;
                        horizontalResolution = 300; }
imageSource.ScanWithSettings settings
```

Listen for events related to scanning:
```fsharp
let deviceManager = DeviceManager()
let scannerConnected =
    deviceManager.RegisterScannerConnected (
        fun scanner -> 
            printfn "A new scanner was connected: %s"
                    scanner.Properties.name)
// ...
deviceManager.UnregisterEvent scannerConnected
```

Set additional WIA properties yourself:
```fsharp
let scanner = DeviceManager().ConnectedScanners() |> Seq.head
scanner.SetProperty 3100 1
```

Show system dialogs:
```fsharp
let scanner = DeviceManager().ScannerSelectDialog()
let source = scanner.ItemSelectDialog() |> Seq.head
let image = source.ScanProgressDialog()
image
```

Access WIA objects directly:
```fsharp
let wiaManager = DeviceManager().__WiaDeviceManager
let deviceInfo = wiaManager.DeviceInfos.[ref (1 :> obj)]
if deviceInfo.Type = WIA.WiaDeviceType.ScannerDeviceType
    then deviceInfo.DeviceID
    else "Device is not a scanner."
```