using System.Collections.Generic;

namespace IfcLibrary.Domain
{
    public class PropertySetCleanup
    {
        public string PropertySetName { get; set; }
        public List<string> PropertyNamesToKeep { get; set; }
    }
}
