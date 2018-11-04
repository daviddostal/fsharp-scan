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



    let deviceInfos (wia: DeviceManager) =
        seq { for i in 1..(wia.DeviceInfos.Count) -> wia.DeviceInfos.Item(ref (i :> obj)) }
        |> Seq.filter (fun x -> x.Type = WiaDeviceType.ScannerDeviceType)

    let deviceInfoFromId (deviceManager: DeviceManager) deviceId =
        deviceInfos deviceManager
        |> Seq.find (fun device -> device.DeviceID = deviceId)
    
    let itemsSeq (items: Items) =
        seq { for i in 1..items.Count -> items.Item(i) }

    let items (scanner: Device) = 
        scanner.Items
        |> itemsSeq

    let getProp (props: Properties) (propId: int) =
        seq { for i in 1..props.Count -> props.Item(ref (i :> obj)) }
        |> Seq.find (fun p -> p.PropertyID = propId)    

    let propValue<'a> (props: Properties) (propId: int) =
        (getProp props propId).Value :?> 'a

    let setProp (props: Properties) (propId: int) value =
        (getProp props propId).Value <- (ref (value :> obj))

    let propRange<'a> (props: Properties) (propId: int) =
        let range = (getProp props propId).SubTypeValues
        seq { for i in 1..range.Count -> range.Item(i) :?> 'a }

    let propMin (props: Properties) (propId: int) =
        (getProp props propId).SubTypeMin

    let propMax (props: Properties) (propId: int) =
        (getProp props propId).SubTypeMax

    let withFlags flags enum =
        let clearFlags flags enum = enum &&& (~~~ flags)
        let allFlags enum = Enum.GetValues(enum.GetType()).Cast<int>() |> Seq.reduce (|||)
        clearFlags (allFlags flags) enum // clear flags in case flags are only a subset of enum
        |> (|||) (LanguagePrimitives.EnumToValue flags) // set value

    let setPropFlags (props: Properties) (propId: int) value =
        propValue props propId
        |> withFlags value
        |> setProp props propId

    let scan (item: Item) =
        item.Transfer() :?> ImageFile


    // Images

    let toBitmap (imageFile: ImageFile) =
        let bytes = imageFile.FileData.get_BinaryData() :?> byte array
        new MemoryStream(bytes) |> System.Drawing.Image.FromStream

    let extractFrame (imageFile: ImageFile) id =
        imageFile.ActiveFrame <- id
        imageFile.ARGBData.get_ImageFile(imageFile.Width, imageFile.Height);

    let extractImages (imageFile: ImageFile) =
        seq { for i in 0..imageFile.FrameCount -> extractFrame imageFile i |> toBitmap }