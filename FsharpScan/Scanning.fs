﻿namespace DavidDostal.FSharpScan
open WIA

module Scanning =
    open Wia
   
     /// Represents an image source (such as flatbed scanner or document feeder) of a scanning device.
    type ImageSource internal (item: Item) =

        let propValue propId = Wia.propValue item.Properties propId
        let setProp propId = Wia.setProp item.Properties propId
        let propRange propId = Wia.propRange item.Properties propId
        let propMin propId = Wia.propMin item.Properties propId
        let propMax propId = Wia.propMax item.Properties propId
        let setFlags propId = Wia.setPropFlags item.Properties propId

        /// Acquire an image from this source.
        member __.Scan =
            Wia.scan >> Wia.toBitmap
        
        /// Get the properties of this image source.
        member __.Properties: ImageSourceProperties =
            { name = propValue PropertyId.ItemName;
              horizontalResolution = propValue PropertyId.HorizontalResolution;
              horizontalResolutions = propRange PropertyId.HorizontalResolution;
              minHorizontalResolution = propRange PropertyId.HorizontalResolution |> Seq.min;
              maxHorizontalResolution = propRange PropertyId.HorizontalResolution |> Seq.max;
              verticalResolution = propValue PropertyId.VerticalResolution;
              verticalResolutions = propRange PropertyId.VerticalResolution;
              minVerticalResolution = propRange PropertyId.VerticalResolution |> Seq.min;
              maxVerticalResolution = propRange PropertyId.VerticalResolution |> Seq.max;
              colorMode = propValue PropertyId.CurrentIntent;
              colorModes = propRange PropertyId.CurrentIntent;
              contrast = propValue PropertyId.Contrast;
              minContrast = propMin PropertyId.Contrast;
              maxContrast = propMax PropertyId.Contrast;
              brightness = propValue PropertyId.Brightness;
              minBrightness = propMin PropertyId.Brightness;
              maxBrightness = propMax PropertyId.Brightness; }
        
        /// Get the current settings of this image source.
        member __.Settings =
            { horizontalResolution = propValue PropertyId.HorizontalResolution;
              verticalResolution = propValue PropertyId.VerticalResolution;
              colorMode = propValue PropertyId.CurrentIntent;
              contrast = propValue PropertyId.Contrast;
              brightness = propValue PropertyId.Brightness; }
        
        /// Set new settings for this scanner.
        member __.Configure settings =
            setProp PropertyId.HorizontalResolution settings.horizontalResolution
            setProp PropertyId.VerticalResolution settings.verticalResolution
            setFlags PropertyId.CurrentIntent settings.colorMode
            setProp PropertyId.Contrast settings.contrast
            setProp PropertyId.Brightness settings.brightness

    and ColorMode = Unspecified = 0 | Color = 1 | Grayscale = 2 | BlackAndWhite = 4

    and ImageSourceSettings =
        { horizontalResolution: int;
          verticalResolution: int;
          colorMode: ColorMode;
          contrast: int;
          brightness: int; }

    and ImageSourceProperties =
        { name: string;
          horizontalResolution: int; horizontalResolutions: int seq; minHorizontalResolution: int; maxHorizontalResolution: int;
          verticalResolution: int; verticalResolutions: int seq; minVerticalResolution: int; maxVerticalResolution: int;
          colorMode: ColorMode; colorModes: ColorMode seq;
          contrast: int; minContrast: int; maxContrast: int;
          brightness: int; minBrightness: int; maxBrightness: int; }


    /// Represents a scanner device connected to your computer.
    type Scanner internal (device: Device) =

        let propValue propId = Wia.propValue device.Properties propId
        let setProp propId = Wia.setProp device.Properties propId

        /// Get information about scanner properties.
        member __.Properties =
            { deviceId = propValue PropertyId.DeviceID;
              name = propValue PropertyId.Name;
              manufacturer = propValue PropertyId.Manufacturer;
              paperSources = failwith "Not implemented yet";
              paperSource = failwith "Not implemented yet";
              horizontalOpticalResolution = propValue PropertyId.HorizontalOpticalResolution
              verticalOpticalResolution = propValue PropertyId.VerticalOpticalResolution
              scanMode = propValue PropertyId.Preview;
              canPreview = propValue PropertyId.ShowPreviewControl; }
        
        /// Get current scanner settings.
        member __.Settings =
            { paperSource = failwith "Not implemented yet";
              scanMode = failwith "Not implemented yet"; }
        
        /// Set new scanner settings.
        member __.Configure settings =
            //setProp paperSource
            setProp PropertyId.Preview settings.scanMode
            failwith "Not implemented yet"
        
        /// Get all scanner image sources.
        member __.ImageSources =
            Wia.items device |> Seq.map (fun d -> ImageSource d)

    and ScannerProperties =
        { deviceId: string;
          name: string;
          manufacturer: string;
          paperSources: PaperSource seq;
          paperSource: PaperSource;
          scanMode: ScanMode;
          canPreview: bool;
          horizontalOpticalResolution: int;
          verticalOpticalResolution: int; }

     and ScannerSettings =
         { paperSource: PaperSource;
           scanMode: ScanMode }

     and PaperSource =
         | Flatbed
         | Feeder
         | Duplexer

     and ScanMode = FinalScan = 0 | Preview = 1


    /// Provides information about an available scanner and allows to connect to it.
    type ScannerInfo internal (deviceInfo: DeviceInfo) =

        let propValue propertyId = Wia.propValue deviceInfo.Properties propertyId

        /// Connect to the device.
        member __.Connect =
            Scanner(Wia.connect deviceInfo)
        
        /// Basic information about the scanner.
        member __.Properties =
            { deviceId = propValue PropertyId.DeviceID
              name = propValue PropertyId.Name;
              manufacturer = propValue PropertyId.Manufacturer; }

     and ScannerInfoProperties =
         { deviceId: string;
           name: string;
           manufacturer: string; }


    /// Manages available scanners.
    type DeviceManager() =
        let deviceManager = Wia.initialize()

        /// Get information about available devices.
        member __.DeviceInfos =
            Wia.deviceInfos deviceManager
            |> Seq.map ScannerInfo
        
        /// Listen to a new scanner being connected.
        member __.listenScannerConnected handler =
            Wia.addEventHandler deviceManager EventID.wiaEventDeviceConnected (fun device _ -> handler device)

        /// Listen to a scanner being disconnected.
        member __.listenScannerDisconnected handler =
            Wia.addEventHandler deviceManager EventID.wiaEventDeviceDisconnected (fun device _ -> handler device)
        
        /// Listen for a scan initiated from a scanner.
        member __.listenIncomingScan handler =
            Wia.addEventHandler deviceManager EventID.wiaEventScanImage (fun device item -> handler device item)
            Wia.addEventHandler deviceManager EventID.wiaEventScanImage2 (fun device item -> handler device item)
            Wia.addEventHandler deviceManager EventID.wiaEventScanImage3 (fun device item -> handler device item)
            Wia.addEventHandler deviceManager EventID.wiaEventScanImage4 (fun device item -> handler device item)
