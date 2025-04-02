namespace l7c.Models;

public class L7CAFilesystemEntry
{
    public uint id;
    public uint hash; // Hash of what?
    public int folderOffset;
    public int filenameOffset;
    public long timestamp;
    public string filename;
}