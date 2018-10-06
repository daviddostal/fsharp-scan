namespace DavidDostal.FSharpScan
open WIA

/// Represent an image source of a scanner, such as flatbed or document feeder.
type ImageSource(item: Item) =
    let dialogs = CommonDialogClass()

    /// Get a property value from the image source.
    member __.GetProperty propId =
        WiaInterop.propValue item.Properties propId
    
    /// Get the minimum allowed value of a property.
    member __.PropertyMinimum propId =
        WiaInterop.propMin item.Properties propId
    
    /// Get the maximum allowed value of a property.
    member __.PropertyMaximum propId =
        WiaInterop.propMax item.Properties propId
    
    /// Get a list of possible values of a property.
    member __.PropertyPossibleValues propId =
        WiaInterop.propRange item.Properties propId
    
    /// Set the value of a property for this image source.
    member __.SetProperty propId value =
        WiaInterop.setProp item.Properties propId value
    
    /// Set the value of a flags property.
    member __.SetPropertyFlag propId value =
        WiaInterop.setPropFlags item.Properties propId value
    
    /// Common properties of the image source.
    member this.Properties =
        { name = this.GetProperty PropertyId.ItemName;
          horizontalResolution = this.GetProperty PropertyId.HorizontalResolution;
          horizontalResolutions = this.PropertyPossibleValues PropertyId.HorizontalResolution;
          minHorizontalResolution = this.PropertyPossibleValues PropertyId.HorizontalResolution |> Seq.min;
          maxHorizontalResolution = this.PropertyPossibleValues PropertyId.HorizontalResolution |> Seq.max;
          verticalResolution = this.GetProperty PropertyId.VerticalResolution;
          verticalResolutions = this.PropertyPossibleValues PropertyId.VerticalResolution;
          minVerticalResolution = this.PropertyPossibleValues PropertyId.VerticalResolution |> Seq.min;
          maxVerticalResolution = this.PropertyPossibleValues PropertyId.VerticalResolution |> Seq.max;
          colorMode = this.GetProperty PropertyId.CurrentIntent;
          colorModes = this.PropertyPossibleValues PropertyId.CurrentIntent;
          contrast = this.GetProperty PropertyId.Contrast;
          minContrast = this.PropertyMinimum PropertyId.Contrast;
          maxContrast = this.PropertyMaximum PropertyId.Contrast;
          brightness = this.GetProperty PropertyId.Brightness;
          minBrightness = this.PropertyMinimum PropertyId.Brightness;
          maxBrightness = this.PropertyMaximum PropertyId.Brightness; }
    
    /// Current configuration of this image source.
    member this.Settings =
        { horizontalResolution = this.GetProperty PropertyId.HorizontalResolution;
          verticalResolution = this.GetProperty PropertyId.VerticalResolution;
          colorMode = this.GetProperty PropertyId.CurrentIntent;
          contrast = this.GetProperty PropertyId.Contrast;
          brightness = this.GetProperty PropertyId.Brightness; }
    
    /// Set new settings of this image source.
    member this.ApplySettings settings =
        this.SetProperty PropertyId.HorizontalResolution settings.horizontalResolution
        this.SetProperty PropertyId.VerticalResolution settings.verticalResolution
        this.SetPropertyFlag PropertyId.CurrentIntent settings.colorMode
        this.SetProperty PropertyId.Contrast settings.contrast
        this.SetProperty PropertyId.Brightness settings.brightness
    
    /// Acquire an image from this source.
    member __.Scan() =
        WiaInterop.scan item |> WiaInterop.toBitmap
    
    /// Perform a scan with the given settings.
    member this.ScanWithSettings settings =
        this.ApplySettings settings
        this.Scan()

    /// Scan an image while showing progress dialog.
    member __.ScanProgressDialog() =
        dialogs.ShowTransfer(item) :?> ImageFile
        |> WiaInterop.toBitmap

    /// Show dialog with image properties (resolution, brightness, contrast, color mode).
    member __.ImagePropertiesDialog() =
        dialogs.ShowItemProperties(item)

/// The color mode used to scan an image.
and ColorMode = Unspecified = 0 | Color = 1 | Grayscale = 2 | BlackAndWhite = 4

/// Common settings for an image source.
and ImageSourceSettings =
    { horizontalResolution: int;
      verticalResolution: int;
      colorMode: ColorMode;
      contrast: int;
      brightness: int; }

/// Common properties of an image source.
and ImageSourceProperties =
    { name: string;
      horizontalResolution: int; horizontalResolutions: int seq; minHorizontalResolution: int; maxHorizontalResolution: int;
      verticalResolution: int; verticalResolutions: int seq; minVerticalResolution: int; maxVerticalResolution: int;
      colorMode: ColorMode; colorModes: ColorMode seq;
      contrast: int; minContrast: int; maxContrast: int;
      brightness: int; minBrightness: int; maxBrightness: int; }