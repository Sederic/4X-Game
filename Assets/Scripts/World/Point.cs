using System;

public class Point 
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(object obj)
    {
        if (obj is Point other)
        {
            return other.x == x && other.y == y;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y);
    }

    public string ToString()
    {
        return "(" + x + "," + y + ")";
    }
}
