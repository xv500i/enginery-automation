using System.Collections.Generic;

namespace IfcLibrary.Ifc
{
    public class AutomatedChanges
    {
        public AutomatedChanges()
        {
            AddPropertySetWithPropertyAndValues = new List<AddPropertySetWithPropertyAndValue>();
            AddPropertySetWithRelativePropertyAndValues = new List<AddPropertySetWithRelativePropertyAndValue>();
            UpdatePropertySetByValues = new List<UpdatePropertySetByValue>();
            UpdatePropertySetByRelativeValues = new List<UpdatePropertySetByRelativeValue>();
        }

        public List<AddPropertySetWithPropertyAndValue> AddPropertySetWithPropertyAndValues { get; }
        public List<AddPropertySetWithRelativePropertyAndValue> AddPropertySetWithRelativePropertyAndValues { get; }
        public List<UpdatePropertySetByValue> UpdatePropertySetByValues { get; }
        public List<UpdatePropertySetByRelativeValue> UpdatePropertySetByRelativeValues { get; }
    }
}