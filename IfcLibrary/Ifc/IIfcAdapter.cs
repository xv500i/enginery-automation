using IfcLibrary.Domain;
using System.Collections.Generic;

namespace IfcLibrary.Ifc
{
    public interface IIfcAdapter
    {
        void PatchFile(string originalPath, string patchedPath, List<EntityChangeInfo> entityChangeInfos);
    }
}
