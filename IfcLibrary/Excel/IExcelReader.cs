using IfcLibrary.Domain;

namespace IfcLibrary.Excel
{
    public interface IExcelReader
    {
        AutomatedChanges GetAutomatedChanges(string path);
    }
}