using IfcLibrary.Domain;
using System.Collections.Generic;

namespace IfcLibrary.Excel
{
    public interface IExcelReader
    {
        List<EntityChangeInfo> GetChanges(string path);
    }
}