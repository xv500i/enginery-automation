using IfcLibrary.Domain;

namespace IfcLibrary.Ifc
{
    public interface IIfcAdapter
    {
        void PatchFile(string originalPath, string patchedPath, AutomatedChanges automatedChanges);
    }
}
