using System;
using System.Numerics;

namespace Solidity {
    public sealed partial class uint8 {
        public uint ToNumber() {
            return (uint)(byte)Data[0];
        }
    }

    public sealed partial class uint16 {
        public uint ToNumber() {
            UnityEngine.Debug.Log("dlen:" + Data.Length);
            var ba = Data.ToByteArray();
            Array.Reverse(ba);
            return (uint)BitConverter.ToUInt16(ba, 0);
        }
    }

    public sealed partial class uint128 {
        public BigInteger ToNumber() {
            return new BigInteger(Data.ToByteArray());
        }
    }
}
