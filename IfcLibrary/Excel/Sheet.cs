using System.Collections.Generic;

namespace IfcLibrary.Excel
{
    internal class Sheet
    {
        public string Name { get; set; }
        public List<List<string>> Cells { get; set; }
    }
}
