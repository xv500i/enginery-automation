﻿using System.Collections.Generic;

namespace IfcLibrary.Domain
{
    public class EntityChange
    {
        public string Identifier { get; set; }
        public List<PropertyChange> PropertyChanges { get; set; }
    }
}
