﻿namespace DavidDostal.FSharpScan

module Wia =
    open WIA
    
    let initialize() =
        DeviceManagerClass()

    let toSeq counter getter comObj =
        seq { for i in 1..(counter comObj) -> getter comObj i }
        
    let deviceInfos (wia: DeviceManager) =
        wia.DeviceInfos
        |> toSeq (fun x -> x.Count) (fun x i -> x.[ref (i :> obj)])

    let connect (device: DeviceInfo) = device.Connect()

    let items (scanner: Device) = 
        scanner.Items
        |> toSeq (fun (x: Items) -> x.Count) (fun x i -> x.[i])

    type PropertyId =
        | DeviceID = 2
        | Manufacturer = 3
        | Description = 4
        | Type = 5
        | Port = 6
        | Name = 7
        | Server = 8
        | RemoteDevID = 9
        | UIClassID = 10
        | FirmwareVersion = 1026
        | ConnectStatus = 1027
        | DeviceTime = 1028
        | PicturesTaken = 2050
        | PicturesRemaining = 2051
        | ExposureMode = 2052
        | ExposureCompensation = 2053
        | ExposureTime = 2054
        | FNumber = 2055
        | FlashMode = 2056
        | FocusMode = 2057
        | FocusManualDist = 2058
        | ZoomPosition = 2059
        | PanPosition = 2060
        | TiltPostion = 2061
        | TimerMode = 2062
        | TimerValue = 2063
        | PowerMode = 2064
        | BatteryStatus = 2065
        | Dimension = 2070
        | HorizontalBedSize = 3074
        | VerticalBedSize = 3075
        | HorizontalSheetFeedSize = 3076
        | VerticalSheetFeedSize = 3077
        | SheetFeederRegistration = 3078
        | HorizontalBedRegistration = 3079
        | VerticalBedRegistraion = 3080
        | PlatenColor = 3081
        | PadColor = 3082
        | FilterSelect = 3083
        | DitherSelect = 3084
        | DitherPatternData = 3085
        | DocumentHandlingCapabilities = 3086
        | DocumentHandlingStatus = 3087
        | DocumentHandlingSelect = 3088
        | DocumentHandlingCapacity = 3089
        | HorizontalOpticalResolution = 3090
        | VerticalOpticalResolution = 3091
        | EndorserCharacters = 3092
        | EndorserString = 3093
        | ScanAheadPages = 3094
        | MaxScanTime = 3095
        | Pages = 3096
        | PageSize = 3097
        | PageWidth = 3098
        | PageHeight = 3099
        | Preview = 3100
        | TransparencyAdapter = 3101
        | TransparecnyAdapterSelect = 3102
        | ItemName = 4098
        | FullItemName = 4099
        | ItemTimeStamp = 4100
        | ItemFlags = 4101
        | AccessRights = 4102
        | DataType = 4103
        | BitsPerPixel = 4104
        | PreferredFormat = 4105
        | Format = 4106
        | Compression = 4107
        | MediaType = 4108
        | ChannelsPerPixel = 4109
        | BitsPerChannel = 4110
        | Planar = 4111
        | PixelsPerLine = 4112
        | BytesPerLine = 4113
        | NumberOfLines = 4114
        | GammaCurves = 4115
        | ItemSize = 4116
        | ColorProfiles = 4117
        | BufferSize = 4118
        | RegionType = 4119
        | ColorProfileName = 4120
        | ApplicationAppliesColorMapping = 4121
        | StreamCompatibilityID = 4122
        | ThumbData = 5122
        | ThumbWidth = 5123
        | ThumbHeight = 5124
        | AudioAvailable = 5125
        | AudioFormat = 5126
        | AudioData = 5127
        | PicturesPerRow = 5128
        | SequenceNumber = 5129
        | TimeDelay = 5130
        | CurrentIntent = 6146
        | HorizontalResolution = 6147
        | VerticalResolution = 6148
        | HorizontalStartPostion = 6149
        | VerticalStartPosition = 6150
        | HorizontalExtent = 6151
        | VerticalExtent = 6152
        | PhotometricInterpretation = 6153
        | Brightness = 6154
        | Contrast = 6155
        | Orientation = 6156
        | Rotation = 6157
        | Mirror = 6158
        | Threshold = 6159
        | Invert = 6160
        | LampWarmUpTime = 6161
    
    let enumVal enum =
        LanguagePrimitives.EnumToValue enum

    let deviceInfoProps (deviceInfo: DeviceInfo) =
        deviceInfo.Properties
        |> toSeq (fun (x: Properties) -> x.Count)
                 (fun x i -> let prop = x.[ref (i :> obj)]
                             (prop.PropertyID, prop))
        |> Map.ofSeq

    let propValue<'a> (prop: Property) =
        prop.Value :?> 'a

    let deviceInfoProp (propId: PropertyId) (deviceInfo: DeviceInfo) =
        deviceInfoProps deviceInfo
        |> Map.find (enumVal propId)
        |> propValue
