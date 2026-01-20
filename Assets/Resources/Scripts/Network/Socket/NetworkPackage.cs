
// [bodySize][Key][Body]

using System;

public class NetworkPackage
{
    public const int HeaderSize = 8;

    public int BodySize { get; set; }
    public int Key { get; set; }
    public byte[] Body { get; set; }

    public bool SetData(byte[] data, int startIndex, int size)
    {
        if (size > TCPCommon.MaxReceivePacketSize)
        {
            return false;
        }
        
        if (size <= 0)
        {
            return false;
        }

        Body = new byte[size];
        Array.Copy(data, startIndex, Body, 0, size);
        return true;
    }
}