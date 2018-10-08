namespace DavidDostal.FSharpScan

module WiaInterop =
    open WIA
    open System.IO
    open System
    open System.Linq
    
    let initialize() =
        let deviceManager = DeviceManagerClass()
        deviceManager.RegisterEvent(EventID.wiaEventDeviceConnected)
        deviceManager.RegisterEvent(EventID.wiaEventDeviceDisconnected)
        deviceManager.RegisterEvent(EventID.wiaEventScanImage)
        deviceManager.RegisterEvent(EventID.wiaEventScanImage2)
        deviceManager.RegisterEvent(EventID.wiaEventScanImage3)
        deviceManager.RegisterEvent(EventID.wiaEventScanImage4)
        deviceManager.RegisterEvent(EventID.wiaEventScanEmailImage)
        deviceManager.RegisterEvent(EventID.wiaEventScanFaxImage)
        deviceManager.RegisterEvent(EventID.wiaEventScanFilmImage)
        deviceManager.RegisterEvent(EventID.wiaEventScanOCRImage)
        deviceManager.RegisterEvent(EventID.wiaEventScanPrintImage)
        deviceManager.RegisterEvent(EventID.wiaEventItemCreated)
        deviceManager.RegisterEvent(EventID.wiaEventItemDeleted)
        deviceManager

    let addEventHandler (deviceManager: DeviceManager) eventId handler =
        let eventHandler =
            new _IDeviceManagerEvents_OnEventEventHandler(
                fun event device item -> if event = eventId then handler device item else ())
        deviceManager.add_OnEvent(eventHandler)
        eventHandler

    let toSeq counter getter comObj =
        seq { for i in 1..(counter comObj) -> getter comObj i }

    let deviceInfos (wia: DeviceManager) =
        wia.DeviceInfos
        |> toSeq (fun x -> x.Count) (fun x i -> x.[ref (i :> obj)])
        |> Seq.filter (fun x -> x.Type = WiaDeviceType.ScannerDeviceType)

    let deviceInfoFromId (deviceManager: DeviceManager) deviceId =
        deviceInfos deviceManager
        |> Seq.find (fun device -> device.DeviceID = deviceId)
    
    let itemsSeq (items: Items) =
        toSeq (fun (x: Items) -> x.Count) (fun x i -> x.[i]) items

    let items (scanner: Device) = 
        scanner.Items
        |> itemsSeq

    let getProp (props: Properties) (propId: int) =
        toSeq (fun (p: Properties) -> p.Count) (fun p i -> p.Item(ref (i :> obj))) props
        |> Seq.find (fun p -> p.PropertyID = propId)    

    let propValue<'a> (props: Properties) (propId: int) =
        (getProp props propId).Value :?> 'a

    let setProp (props: Properties) (propId: int) value =
        (getProp props propId).Value <- (ref (value :> obj))

    let propRange<'a> (props: Properties) (propId: int) =
        (getProp props propId).SubTypeValues
        |> toSeq (fun (x: Vector) -> x.Count )
                 (fun x i -> x.Item(i) :?> 'a)

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

    let toBitmap (imageFile: ImageFile) =
        let bytes = imageFile.FileData.get_BinaryData() :?> byte array
        new MemoryStream(bytes) |> System.Drawing.Image.FromStream

    let extractFrame (imageFile: ImageFile) id =
        imageFile.ActiveFrame <- id
        imageFile.ARGBData.get_ImageFile(imageFile.Width, imageFile.Height);

    let extractImages (imageFile: ImageFile) =
        seq { for i in 0..imageFile.FrameCount -> extractFrame imageFile i |> toBitmap }