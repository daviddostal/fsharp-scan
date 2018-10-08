namespace DavidDostal.FSharpScan
open WIA

type WiaEventRegisterer =
    abstract member RegisterEvent: string -> (string -> string -> unit) -> _IDeviceManagerEvents_OnEventEventHandler
    abstract member UnregisterEvent: _IDeviceManagerEvents_OnEventEventHandler -> unit

/// Represents a scanner device.
/// A scanner can have one or more image sources (flatbed, feeder, ...).
type Scanner(device: Device, eventRegisterer: WiaEventRegisterer) =
    let dialogs = CommonDialogClass()

    /// Get a property value from the scanner.
    member __.GetProperty propId =
        WiaInterop.propValue device.Properties propId
    
    /// Set the value of a scanner property.
    member __.SetProperty propId value =
        WiaInterop.setProp device.Properties propId value

    /// Listen for events related to this scanner.
    member __.RegisterEvent (eventId: string) (handler: string -> unit) =
        let handler = (fun deviceId itemId -> if deviceId = device.DeviceID then handler itemId else ())
        eventRegisterer.RegisterEvent eventId handler
    
    /// Stop listening for an event created by calling RegisterEvent.
    member __.UnregisterEvent event =
        eventRegisterer.UnregisterEvent event
    
    /// Common scanner properties.
    member this.Properties =
        { deviceId = this.GetProperty PropertyId.DeviceID;
          name = this.GetProperty PropertyId.Name;
          manufacturer = this.GetProperty PropertyId.Manufacturer;
          horizontalOpticalResolution = this.GetProperty PropertyId.HorizontalOpticalResolution
          verticalOpticalResolution = this.GetProperty PropertyId.VerticalOpticalResolution }
    
    /// Get all image sources (such as flatbed or feeder) of this device.
    member this.ImageSources() =
        WiaInterop.items device
        |> Seq.map (fun item -> ImageSource(item, this))

    /// Show detailed scanner settings and import images after scan.
    member __.ScanWizardDialog() =
        dialogs.ShowAcquisitionWizard(device)
        |> ignore

    /// Show dialog with scanner properties.
    member __.ScannerPropsDialog() =
        dialogs.ShowDeviceProperties(device)

    /// Select scanner item before scanning. Same as scanDialog if only single source is present.
    member this.ItemSelectDialog() =
        dialogs.ShowSelectItems(device)
        |> WiaInterop.itemsSeq
        |> Seq.map (fun item -> ImageSource(item, this))

    /// Provides access to the internal WIA Device instance.
    member __.__WiaDevice = device

    interface DeviceEventRegisterer with
        member this.RegisterEvent eventId handler = this.RegisterEvent eventId handler
        member this.UnregisterEvent handler = this.UnregisterEvent handler

/// Common properties of a scanner device.
and ScannerProperties =
    { deviceId: string;
      name: string;
      manufacturer: string;
      horizontalOpticalResolution: int;
      verticalOpticalResolution: int; }