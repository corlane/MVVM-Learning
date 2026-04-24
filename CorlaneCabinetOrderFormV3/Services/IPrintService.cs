using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Services;

// IPrintService.cs
// Defines the contract for the print service that sends formatted job reports to a
// physical printer via the WPF PrintDialog.
//
// The concrete implementation (PrintService) builds a WPF FlowDocument for each report
// type, paginated automatically by WPF's DocumentPaginator. Each document includes a
// company name / job name header, a section title, and a bordered table of data.
//
// Three report types are supported:
//   - PrintCabinetList: prints a summary table of all cabinets in the job (qty, type,
//     style, name, and W×H×D dimensions formatted per the current dimension format setting)
//   - PrintDoorList: prints the door/drawer front cut list (cabinet number, name, type,
//     height, width, species, grain direction)
//   - PrintDrawerBoxList: prints the drawer box cut list (cabinet number, name, type,
//     height, width, length)
//
// All three methods show the system PrintDialog before printing, allowing the user to
// select a printer and adjust settings. Cancelling the dialog aborts the print silently.

public interface IPrintService
{
    void PrintCabinetList(string companyName, string jobName, string dimensionFormat, IReadOnlyList<CabinetModel> cabinets);
    void PrintDoorList(string companyName, string jobName, IReadOnlyList<FrontPartRow> doors);
    void PrintDrawerBoxList(string companyName, string jobName, IReadOnlyList<DrawerBoxRow> drawerBoxes);
}