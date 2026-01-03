using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Services;

public interface IPrintService
{
    void PrintCabinetList(string companyName, string jobName, string dimensionFormat, IReadOnlyList<CabinetModel> cabinets);
    void PrintDoorList(string companyName, string jobName, IReadOnlyList<FrontPartRow> doors);
    void PrintDrawerBoxList(string companyName, string jobName, IReadOnlyList<DrawerBoxRow> drawerBoxes);
}