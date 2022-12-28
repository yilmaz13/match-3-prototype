using System;

namespace Utilities
{
    public static class Utility
    {
        public static bool NoTileNeighbor(int x1, int y1, int x2, int y2)
        {
            if (Math.Abs(x1 - x2) > 1 || Math.Abs(y1 - y2) > 1)
                return true;

            if (Math.Abs(x1 - x2) != 0 && Math.Abs(y1 - y2) != 0)
                return true;

            return false;
        }
    }
}