namespace DavidDostal.FSharpScan
open WIA

type Scanner(device: Device) =
    let dialogs = CommonDialogClass()

    member __.GetProperty propId =
        WiaInterop.propValue device.Properties propId

    member __.SetProperty propId value =
        WiaInterop.setProp device.Properties propId value

    member this.Properties =
        { deviceId = this.GetProperty PropertyId.DeviceID;
          name = this.GetProperty PropertyId.Name;
          manufacturer = this.GetProperty PropertyId.Manufacturer;
          horizontalOpticalResolution = this.GetProperty PropertyId.HorizontalOpticalResolution
          verticalOpticalResolution = this.GetProperty PropertyId.VerticalOpticalResolution }

    member __.ImageSources() =
        WiaInterop.items device
        |> Seq.map ImageSource

    /// Show detailed scanner settings and import images after scan.
    member __.ScanWizard() =
        dialogs.ShowAcquisitionWizard(device)
        |> ignore

    /// Show dialog with scanner properties.
    member __.ScannerPropsDialog() =
        dialogs.ShowDeviceProperties(device)

    /// Select scanner item before scanning. Same as scanDialog if only single source is present.
    member __.ItemSelectDialog =
        dialogs.ShowSelectItems(device)
        |> WiaInterop.itemsSeq
        |> Seq.map ImageSource

and ScannerProperties =
    { deviceId: string;
      name: string;
      manufacturer: string;
      horizontalOpticalResolution: int;
      verticalOpticalResolution: int; }