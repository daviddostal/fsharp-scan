#r ".\obj\Debug\Interop.WIA.dll"
#r ".\obj\Debug\FSharpScan.dll"
open WIA
open DavidDostal.FSharpScan

module Scripts =
    type Result<'a, 'b> = Success of 'a | Failure of 'b

    let onEvent eventId deviceId itemId =
        printfn "Event a: %s, b: %s, c: %s" eventId deviceId itemId

    let createWia () =
        let wia = WIA.DeviceManagerClass()
        wia.RegisterEvent(EventID.wiaEventDeviceConnected)
        wia.RegisterEvent(EventID.wiaEventDeviceDisconnected)
        wia

    let connect (device: DeviceInfo) = device.Connect()

    let toSeq counter getter comObj =
        seq { for i in 1..(counter comObj) -> getter comObj i }

    let deviceInfos (wia: DeviceManagerClass) =
        wia.DeviceInfos
        |> toSeq (fun x -> x.Count) (fun x i -> x.[ref (i :> obj)])

    let items (scanner: Device) = 
        scanner.Items
        |> toSeq (fun (x: Items) -> x.Count) (fun x i -> x.[i])

    let itemProperties (item: Item) =
        item.Properties
        |> toSeq (fun (x: Properties) -> x.Count) (fun x i -> x.[ref (i :> obj)])

    let scanners (wia: DeviceManagerClass) =
        deviceInfos wia

    let scannerProperties (scanner: Device) =
        scanner.Properties
        |> toSeq (fun (x: Properties) -> x.Count) (fun x i -> x.[ref (i :> obj)])

    let propsMap props =
        props
        |> Seq.map (fun (prop: Property) -> prop.PropertyID, prop)
        |> Map.ofSeq

    let scan (scanner: Device) =
        scanner.Items.[1].Transfer()

    let wia = createWia ()

    let scanner =
        wia
        |> scanners
        |> Seq.map (fun d -> d.Connect())
        |> Seq.head

    let scannerItems = items scanner
    let scannerProps = scannerProperties scanner |> propsMap
    let picture = Seq.head scannerItems
    let pictureProps =  scannerItems |> Seq.head |> itemProperties |> propsMap

    let prop propsMap (prop: int) = Map.tryFind prop propsMap
    let scannerProp = prop scannerProps
    let pictureProp = prop pictureProps

    let setDeviceProp value (propId: int) =
        let prop = prop scannerProps propId
        match prop with
        | Some p ->
            if p.IsReadOnly
            then Failure("Property is readonly")
            else p.Value <- ref value
                 Success("Property successfuly updated.")
        | None -> Failure("Property not found.")
   
    let setItemProp value (propId: int) =
        let prop = prop pictureProps propId
        match prop with
        | Some p ->
            if p.IsReadOnly
            then Failure("Property is readonly")
            else p.Value <- ref value
                 Success("Property successfuly updated.")
        | None -> Failure("Property not found.")
   
    let setColorMode mode =
        setItemProp mode PropertyId.CurrentIntent

    let vectorToSeq (vec: Vector) =
        Seq.init vec.Count (fun i -> vec.[i+1])

    let getResolutions () =
        prop pictureProps PropertyId.HorizontalResolution
        |> Option.map (fun (p: Property) -> p.SubTypeValues)
        |> Option.map vectorToSeq

    let setResolution res =
        setItemProp res PropertyId.VerticalResolution |> ignore
        setItemProp res PropertyId.HorizontalResolution |> ignore