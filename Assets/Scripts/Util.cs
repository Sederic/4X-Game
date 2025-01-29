using System;

public class Util {
    public static double GetUnixTimeMilliseconds() {
        return (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
    }
}