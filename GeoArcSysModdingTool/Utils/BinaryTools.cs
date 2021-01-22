﻿namespace GeoArcSysModdingTool.Utils
{
    public static class BinaryTools
    {
        public static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }
    }
}