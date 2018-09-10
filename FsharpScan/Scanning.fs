namespace DavidDostal.FSharpScan
open WIA

module Scanning =
    
    // Manages available scanners
    type DeviceManager() =
        let deviceManager = Wia.initialize()

        // Get information about available devices
        member __.DeviceInfos =
            Wia.deviceInfos deviceManager


    // Represents a scanner device connected to your computer.
    type Scanner internal (device: Device) =

        // Get information about scanner properties.
        member __.Properties =
            failwith "Not implemented yet"
        
        // Get current scanner settings
        member __.Settings =
            failwith "Not implemented yet"
        
        // set new scanner settings
        member __.Configure settings =
            failwith "Not implemented yet"
        
        // Get all scanner image sources (flatbed, feeder, ...)
        member __.ImageSources =
            failwith "Not implemented yet"

        type ScannerProperties =
            { name: string;
              (*...*) }


    // Provides information about an available scanner and allows to connect to it.
    type ScannerInfo internal (deviceInfo: DeviceInfo) =

        // Connect to the device
        member __.Connect =
            Scanner(Wia.connect deviceInfo)
        
        // Basic information about the scanner
        member __.Properties =
            failwith "Not implemented yet"

        type ScannerInfoProperties =
            { name: string;
              manufacturer: string;
              (*...*) }


    // Represents an image source (such as flatbed scanner or document feeder) of a scanning device.
    type ImageSource internal (item: Item) =

        // Acquire an image from this source
        member __.Scan =
            failwith "Not implemented yet"
        
        // Get the properties of this image source
        member __.Properties =
            failwith "Not implemented yet"
        
        // Get the current settings of this image source
        member __.Settings =
            failwith "Not implemented yet"
        
        // Set new settings for this scanner
        member __.Configure settings =
            failwith "Not implemented yet"

        type ColorMode = Unspecified | Color | Grayscale | BlackAndWhite

        type ImageSourceSettings =
            { resolution: int;
              colorMode: ColorMode;
              contrast: int;
              brightness: int;
              showProgress: bool;
              (*...*) }

        type ImageSourceProperties =
            { name: string;
              resolution: int; resolutions: int seq; minResolution: int; maxResolution: int;
              colorMode: ColorMode; colorModes: ColorMode seq;
              contrast: int; minContrast: int; maxContrast: int;
              brightness: int; minBrightness: int; maxBrightness: int;
              showProgress: bool;
              (*...*)}
