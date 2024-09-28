using IfcLibrary.Domain;
using System;

namespace IfcLibrary.Ifc
{
    public interface IIfcAdapter
    {
        void PatchFile(string originalPath, string patchedPath, IfcManipulations ifcManipulations);

        event EventHandler<PatchProgress> PatchProgressUpdated;
    }
}
