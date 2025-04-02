namespace l7c.Models;

public class L7CAFileEntry
{
    public int compressedFilesize;
    public int rawFilesize;
    public int chunkIdx;
    public int chunkCount;
    public int offset;
    public uint crc32;
}