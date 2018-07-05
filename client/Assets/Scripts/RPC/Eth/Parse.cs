using System.Collections;
using System.Collections.Generic;

using Nethereum.Hex.HexTypes;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding;

namespace Game.Eth.Util {
public static class EthResultParseHelper
{
    public static HexBigInteger HexZero = new HexBigInteger(System.Numerics.BigInteger.Zero);
    public static HexBigInteger AsInt(this string ret) {
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

    public static ParameterDecoder decoder_ = new ParameterDecoder();
    static System.Type FunctionType = typeof(Function);
    public static List<ParameterOutput> DecodeResponse(this Function fn, string data) {
        System.Reflection.PropertyInfo pi = FunctionType.GetProperty(
            "FunctionBuilder", 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.GetProperty);
        var fb = (FunctionBuilder)pi.GetValue(fn, null);
        return decoder_.DecodeDefaultData(data, fb.FunctionABI.OutputParameters);
    }
}
}
