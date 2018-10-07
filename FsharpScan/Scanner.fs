namespace DavidDostal.FSharpScan
open WIA

/// Represents a scanner device.
/// A scanner can have one or more image sources (flatbed, feeder, ...).
type Scanner(device: Device) =
    let dialogs = CommonDialogClass()

    /// Get a property value from the scanner.
    member __.GetProperty propId =
        WiaInterop.propValue device.Properties propId
    
    /// Set the value of a scanner property.
    member __.SetProperty propId value =
        WiaInterop.setProp device.Properties propId value
    
    /// Common scanner properties.
    member this.Properties =
        { deviceId = this.GetProperty PropertyId.DeviceID;
          name = this.GetProperty PropertyId.Name;
          manufacturer = this.GetProperty PropertyId.Manufacturer;
          horizontalOpticalResolution = this.GetProperty PropertyId.HorizontalOpticalResolution
          verticalOpticalResolution = this.GetProperty PropertyId.VerticalOpticalResolution }
    
    /// Get all image sources (such as flatbed or feeder) of this device.
    member __.ImageSources() =
        WiaInterop.items device
        |> Seq.map ImageSource

    /// Show detailed scanner settings and import images after scan.
    member __.ScanWizardDialog() =
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

    /// Provides access to the internal WIA Device instance.
    member __.__WiaDevice = device

/// Common properties of a scanner device.
and ScannerProperties =
    { deviceId: string;
      name: string;
      manufacturer: string;
      horizontalOpticalResolution: int;
      verticalOpticalResolution: int; }