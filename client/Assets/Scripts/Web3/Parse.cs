using System;
using System.Diagnostics;

using Nethereum.Hex.HexTypes;

namespace Game.Web3Util {
public static class Web3ResultParseHelper
{
    public static HexBigInteger AsInt(this string ret)
    {
        return new HexBigInteger(ret);
    }
}
}
