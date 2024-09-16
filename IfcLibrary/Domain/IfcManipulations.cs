using System.Collections.Generic;

namespace IfcLibrary.Domain
{
    public class IfcManipulations
    {
        public List<PropertySetCleanup> PropertySetCleanups { get; set; }
        public List<EntityChange> EntityChanges { get; set; }
    }
}
