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
        deviceManager

    let addEventHandler (deviceManager: DeviceManager) eventId handler =
        deviceManager.add_OnEvent(
            new _IDeviceManagerEvents_OnEventEventHandler(
                fun event device item -> if event = eventId then handler device item else ()))

    let toSeq counter getter comObj =
        seq { for i in 1..(counter comObj) -> getter comObj i }

    let deviceInfos (wia: DeviceManager) =
        wia.DeviceInfos
        |> toSeq (fun x -> x.Count) (fun x i -> x.[ref (i :> obj)])
        |> Seq.filter (fun x -> x.Type = WiaDeviceType.ScannerDeviceType)

    let connect (device: DeviceInfo) = device.Connect()

    let itemsSeq (items: Items) =
        toSeq (fun (x: Items) -> x.Count) (fun x i -> x.[i]) items

    let items (scanner: Device) = 
        scanner.Items
        |> itemsSeq

    let enumVal enum =
        LanguagePrimitives.EnumToValue enum

    let getProp (props: Properties) (propId: PropertyId) =
        toSeq (fun (p: Properties) -> p.Count) (fun p i -> p.Item(ref (i :> obj))) props
        |> Seq.find (fun p -> p.PropertyID = enumVal propId)    

    let propValue<'a> (props: Properties) (propId: PropertyId) =
        (getProp props propId).Value :?> 'a

    let setProp (props: Properties) (propId: PropertyId) value =
        (getProp props propId).Value <- (ref (value :> obj))

    let propRange<'a> (props: Properties) (propId: PropertyId) =
        (getProp props propId).SubTypeValues
        |> toSeq (fun (x: Vector) -> x.Count )
                 (fun x i -> x.Item(i) :?> 'a)

    let propMin (props: Properties) (propId: PropertyId) =
        (getProp props propId).SubTypeMin

    let propMax (props: Properties) (propId: PropertyId) =
        (getProp props propId).SubTypeMax

    // let getFlags<'a> (enum: Enum) =
    //     Enum.GetValues(enum.GetType()).Cast<Enum>() |> Seq.filter (fun x -> enum.HasFlag(x))

    let withFlags flags enum =
        let clearFlags flags enum = enum &&& (~~~ flags)
        let allFlags enum = Enum.GetValues(enum.GetType()).Cast<int>() |> Seq.reduce (|||)
        clearFlags (allFlags flags) enum // clear flags in case flags are only a subset of enum
        |> (|||) (enumVal flags)         // set value

    let setPropFlags (props: Properties) (propId: PropertyId) value =
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
