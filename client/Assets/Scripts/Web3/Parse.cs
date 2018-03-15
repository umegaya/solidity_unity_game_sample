using System;
using System.Diagnostics;

using Nethereum.Hex.HexTypes;

namespace Game.Web3Util {
public static class Web3ResultParseHelper
{
    public static HexBigInteger HexZero = new HexBigInteger(System.Numerics.BigInteger.Zero);
    public static HexBigInteger AsInt(this string ret)
    {
        //because 0x000........0 string causes following crash (IndexOutOfRangeException)
        //in System.Numerics.BigInteger in dotnet 3.5 of Unity3d... so strange
        var i = 0;
        if (ret.Substring(0, 2) == "0x") {
            i = 2;
        }
        for (; i < ret.Length; i++) {
            if (ret[i] != '0') {
                break;
            }
        }
        return i == ret.Length ? HexZero : new HexBigInteger(ret);
    }
}
}
