using System;

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
}