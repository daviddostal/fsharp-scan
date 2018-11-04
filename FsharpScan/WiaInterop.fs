namespace DavidDostal.FSharpScan

module WiaInterop =
    open WIA
    open System.IO
    open System
    open System.Linq
    
    let initialize() = DeviceManagerClass()

    // Events

    let registerEvent (deviceManager: DeviceManagerClass) eventId =
        deviceManager.RegisterEvent(eventId)
    
    let registerKnownEvents (deviceManager: DeviceManagerClass) =
        [ EventID.wiaEventDeviceConnected;
          EventID.wiaEventDeviceDisconnected;
          EventID.wiaEventScanImage;
          EventID.wiaEventScanImage2;
          EventID.wiaEventScanImage3;
          EventID.wiaEventScanImage4;
          EventID.wiaEventScanEmailImage; 
          EventID.wiaEventScanFaxImage;
          EventID.wiaEventScanFilmImage;
          EventID.wiaEventScanOCRImage;
          EventID.wiaEventScanPrintImage;
          EventID.wiaEventItemCreated;
          EventID.wiaEventItemDeleted; ]
        |> List.iter (registerEvent deviceManager)

    let addEventHandler (deviceManager: DeviceManager) eventId handler =
        let eventHandler =
            new _IDeviceManagerEvents_OnEventEventHandler(
                fun event device item -> if event = eventId then handler device item else ())
        deviceManager.add_OnEvent(eventHandler)
        eventHandler

    
    // Wia item tree

    let isScanner (deviceInfo: DeviceInfo) =
        deviceInfo.Type = WiaDeviceType.ScannerDeviceType

    let deviceInfos (wia: DeviceManager) =
        seq { for i in 1..(wia.DeviceInfos.Count) -> wia.DeviceInfos.Item(ref (i :> obj)) }

    let deviceInfo (deviceManager: DeviceManager) deviceId =
        deviceInfos deviceManager
        |> Seq.find (fun device -> device.DeviceID = deviceId)
    
    let itemsToSeq (items: Items) =
        seq { for i in 1..items.Count -> items.Item(i) }

    let items (scanner: Device) = 
        itemsToSeq scanner.Items


    // Properties

    let property (props: Properties) (propId: int) =
        seq { for i in 1..props.Count -> props.Item(ref (i :> obj)) }
        |> Seq.find (fun p -> p.PropertyID = propId)    

    let getValue<'a> (props: Properties) (propId: int) =
        (property props propId).Value :?> 'a

    let setValue (props: Properties) (propId: int) value =
        (property props propId).Value <- (ref (value :> obj))

    let getRange<'a> (props: Properties) (propId: int) =
        let range = (property props propId).SubTypeValues
        seq { for i in 1..range.Count -> range.Item(i) :?> 'a }

    let getMin (props: Properties) (propId: int) =
        (property props propId).SubTypeMin

    let getMax (props: Properties) (propId: int) =
        (property props propId).SubTypeMax

    let withFlags flags enum =
        let clearFlags flags enum = enum &&& (~~~ flags)
        let allFlags enum = Enum.GetValues(enum.GetType()).Cast<int>() |> Seq.reduce (|||)
        clearFlags (allFlags flags) enum // clear flags in case flags are only a subset of enum
        |> (|||) (LanguagePrimitives.EnumToValue flags) // set value

    let setFlags (props: Properties) (propId: int) value =
        getValue props propId
        |> withFlags value
        |> setValue props propId


    // Scanning

    let scan (item: Item) =
        item.Transfer() :?> ImageFile

    let toBitmap (imageFile: ImageFile) =
        let bytes = imageFile.FileData.get_BinaryData() :?> byte array
        new MemoryStream(bytes) |> System.Drawing.Image.FromStream

    let extractFrame (imageFile: ImageFile) id =
        imageFile.ActiveFrame <- id
        imageFile.ARGBData.get_ImageFile(imageFile.Width, imageFile.Height);

    let extractImages (imageFile: ImageFile) =
        seq { for i in 0..imageFile.FrameCount -> extractFrame imageFile i |> toBitmap }