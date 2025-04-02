namespace l7c.Models;

public class L7CAHeader
{
    public uint magic = 0x4143374c; // L7CA
    public uint unk = 0x00010000; // Version? Must be 0x00010000
    public int archiveSize;
    public int metadataOffset;
    public int metadataSize;
    public uint unk2 = 0x00010000; // Chunk max size?
    public int filesystemEntries;
    public int folders;
    public int files;
    public int chunks;
    public int stringTableSize;
    public int unk4 = 5; // Number of sections??
}