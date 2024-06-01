using IfcLibrary.Ifc;

namespace IfcLibrary.Excel
{
    public interface IExcelReader
    {
        AutomatedChanges GetAutomatedChanges(string path);
    }
}