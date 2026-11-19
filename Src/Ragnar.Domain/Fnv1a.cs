namespace Ragnar.Domain;

public static class Fnv1a
{
    public static uint Hash32(ReadOnlySpan<byte> data)
    {
        uint hash = 0x811C9DC5;
        foreach (byte b in data)
        {
            hash = (hash ^ b) * 0x01000193;
        }
        return hash;
    }

    public static ulong Hash64(ReadOnlySpan<byte> data)
    {
        ulong hash = 0xCBF29CE484222325;
        foreach (byte b in data)
        {
            hash = (hash ^ b) * 0x100000001B3;
        }
        return hash;
    }
}
