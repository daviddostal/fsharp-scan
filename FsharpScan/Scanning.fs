namespace DavidDostal.FSharpScan
open WIA

module Scanning =
    open Wia
   

     // Represents an image source (such as flatbed scanner or document feeder) of a scanning device.
    type ImageSource internal (item: Item) =

        let propValue propId = Wia.propValue item.Properties propId
        let propRange propId = Wia.propRange item.Properties propId
        let propMin propId = Wia.propMin item.Properties propId
        let propMax propId = Wia.propMax item.Properties propId

        // Acquire an image from this source
        member __.Scan =
            Wia.scan >> Wia.toBitmap
        
        // Get the properties of this image source
        member __.Properties: ImageSourceProperties =
            { name = propValue PropertyId.ItemName;
              resolution = propValue PropertyId.HorizontalOpticalResolution;
              resolutions = propRange PropertyId.HorizontalResolution;
              //minResolution = Wia.propMin PropertyId.Ho;
              //maxResolution = 0;
              colorMode = propValue PropertyId.CurrentIntent;
              colorModes = propRange PropertyId.CurrentIntent;
              contrast = propValue PropertyId.Contrast;
              minContrast = propMin PropertyId.Contrast;
              maxContrast = propMax PropertyId.Contrast;
              brightness = propValue PropertyId.Brightness;
              minBrightness = propMin PropertyId.Brightness;
              maxBrightness = propMax PropertyId.Brightness;
              //showProgress = getProp PropertyId.;
              }
        
        // Get the current settings of this image source
        member __.Settings =
            failwith "Not implemented yet"
        
        // Set new settings for this scanner
        member __.Configure settings =
            failwith "Not implemented yet"

    and ColorMode = Unspecified=0 | Color=1 | Grayscale=2 | BlackAndWhite=4

    and ImageSourceSettings =
        { resolution: int;
          colorMode: ColorMode;
          contrast: int;
          brightness: int;
          showProgress: bool;
          (*...*) }

    and ImageSourceProperties =
        { name: string;
          resolution: int; resolutions: int seq; //minResolution: int; maxResolution: int;
          colorMode: ColorMode; colorModes: ColorMode seq;
          contrast: int; minContrast: int; maxContrast: int;
          brightness: int; minBrightness: int; maxBrightness: int;
          //showProgress: bool;
          (*...*)}


    // Represents a scanner device connected to your computer.
    type Scanner internal (device: Device) =

        let propValue propId = Wia.propValue device.Properties propId

        // Get information about scanner properties.
        member __.Properties =
            { name = propValue PropertyId.Name;
              manufacturer = propValue PropertyId.Manufacturer;
              paperSources = Seq.empty;
              paperSource = PaperSource.Flatbed;
              //horizontalResolution = Wia.propValue PropertyId.HorizontalOpticalResolution device.Properties
              (*...*)}
        
        // Get current scanner settings
        member __.Settings =
            { paperSource = PaperSource.Flatbed }
        
        // set new scanner settings
        member __.Configure settings =
            failwith "Not implemented yet"
        
        // Get all scanner image sources
        member __.ImageSources =
            Wia.items device |> Seq.map (fun d -> ImageSource d)

    and ScannerProperties =
        { name: string;
          manufacturer: string;
          paperSources: PaperSource seq;
          paperSource: PaperSource;
          //horizontalResolution: int;
          (*...*) }

     and ScannerSettings =
         { paperSource: PaperSource}

     and PaperSource =
         | Flatbed
         | Feeder
         | Duplexer


    // Provides information about an available scanner and allows to connect to it.
    type ScannerInfo internal (deviceInfo: DeviceInfo) =

        let propValue propertyId = Wia.propValue deviceInfo.Properties propertyId

        // Connect to the device
        member __.Connect =
            Scanner(Wia.connect deviceInfo)
        
        // Basic information about the scanner
        member __.Properties =
            { name = propValue Wia.PropertyId.Name;
              manufacturer = propValue PropertyId.Manufacturer }

     and ScannerInfoProperties =
         { name: string;
           manufacturer: string;
           (*...*) }

    // Manages available scanners
    type DeviceManager() =
        let deviceManager = Wia.initialize()

        // Get information about available devices
        member __.DeviceInfos =
            Wia.deviceInfos deviceManager
            |> Seq.map ScannerInfo
   
