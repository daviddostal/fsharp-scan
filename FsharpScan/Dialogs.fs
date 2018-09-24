namespace DavidDostal.FSharpScan
module Dialogs =
    open WIA

    /// Show dialog with simplified scanner settings and option to scan.
    let scanDialog () = CommonDialogClass().ShowAcquireImage(WiaDeviceType.ScannerDeviceType)

    /// Show dialog for choosing between installed scanners. Returns default scanner if only one is available.
    let scannerSelectDialog () = CommonDialogClass().ShowSelectDevice(WiaDeviceType.ScannerDeviceType)

    /// Scan an image while showing progress dialog.
    let scanProgressDialog source = CommonDialogClass().ShowTransfer(source)

    /// Show detailed scanner settings and import images after scan.
    let scanWizard device = CommonDialogClass().ShowAcquisitionWizard(device)

    /// Show dialog with scanner properties.
    let scannerPropsDialog scanner = CommonDialogClass().ShowDeviceProperties(scanner)

    /// Show dialog with image properties (resolution, brightness, contrast, color mode).
    let imagePropsDialog source = CommonDialogClass().ShowItemProperties(source)

    /// Select scanner item before scanning. Same as scanDialog if only single source is present.
    let itemSelectDialog scanner = CommonDialogClass().ShowSelectItems(scanner)