namespace DavidDostal.FSharpScan

module Wia =
    open WIA
    
    let initialize() =
        DeviceManagerClass()

    let toSeq counter getter comObj =
        [for i in 1..(counter comObj) -> getter comObj i]

    let deviceInfos (wia: DeviceManager) =
        wia.DeviceInfos
        |> toSeq (fun x -> x.Count) (fun x i -> x.[ref (i :> obj)])

    