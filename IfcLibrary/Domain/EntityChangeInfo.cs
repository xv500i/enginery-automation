using System.Collections.Generic;

namespace IfcLibrary.Domain
{
    public class EntityChangeInfo
    {
        public string Identifier { get; set; }
        public string Entity { get; set; }
        public List<PropertyChangeInfo> PropertyChangeInfos { get; set; }
    }
}
