using IfcLibrary.Domain;

namespace IfcLibrary.Excel
{
    public interface IExcelReader
    {
        IfcManipulations GetChanges(string path);
    }
}