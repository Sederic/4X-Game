using System;
using System.IO;
using System.IO.Compression;
using System.Text;

public class Util
{
    public static double GetUnixTimeMilliseconds()
    {
        return (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
    }

    public static double eucDist(Point p1, Point p2)
    {
        int x = p2.x - p1.x;
        int y = p2.y - p1.y;
        return Math.Abs(Math.Sqrt(x * x + y * y));
    }

    public static byte[] CompressJson(string json) {
        byte[] jsonData = Encoding.UTF8.GetBytes(json);
        using (MemoryStream output = new MemoryStream())
        using (GZipStream compressionStream = new GZipStream(output, CompressionMode.Compress)) {
            compressionStream.Write(jsonData, 0, jsonData.Length);
            compressionStream.Close();
            return output.ToArray(); // Convert compressed bytes to string
        }
    }

    public static string DecompressJson(byte[] compressedJson) {
        byte[] compressedData = compressedJson;
        using (MemoryStream input = new MemoryStream(compressedData))
        using (GZipStream decompressionStream = new GZipStream(input, CompressionMode.Decompress))
        using (MemoryStream output = new MemoryStream()) {
            decompressionStream.CopyTo(output);
            return Encoding.UTF8.GetString(output.ToArray());
        }
    }
}