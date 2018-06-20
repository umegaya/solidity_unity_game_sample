using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.FunctionEncoding.Attributes;

using BigInteger = System.Numerics.BigInteger;

namespace Game.Eth.Event {
class Transfer {
    public Transfer() {}
    [ParameterAttribute("address", null, 1, true)]
    public string From { get; set; }

    [ParameterAttribute("address", null, 2, true)]
    public string To { get; set; }

    [ParameterAttribute("uint", null, 3)]
    public BigInteger Value { get; set; }

    public override string ToString() {
        return "Transfer " + From + " => " + To + "(" + Value.ToString() + ")";
    }
}
class Exchange {
    public Exchange() {}
    [ParameterAttribute("uint", null, 1)]
    public BigInteger Value { get; set; }

    [ParameterAttribute("uint", null, 2)]
    public BigInteger Rate { get; set; }

    [ParameterAttribute("uint", null, 3)]
    public BigInteger TokenSold { get; set; }

    [ParameterAttribute("uint", null, 4)]
    public BigInteger Result { get; set; }

    public override string ToString() {
        return "Exchange " + Value.ToString() + 
            " => " + Result.ToString() + 
            "@" + Rate.ToString() + 
            "(total sold:" + TokenSold + ")";
    }

} 
class Approval {
    public Approval() {}
    [ParameterAttribute("address", null, 1, true)]
    public string Owner { get; set; }

    [ParameterAttribute("address", null, 2, true)]
    public string Spender { get; set; }

    [ParameterAttribute("uint", null, 3)]
    public BigInteger Value { get; set; }
    public override string ToString() {
        return "Approval " + Owner + " => " + Spender + "(" + Value.ToString() + ")";
    }
}
class AddCard {
    public AddCard() {}
    [ParameterAttribute("address", null, 1, true)]
    public string User { get; set; }

    [ParameterAttribute("uint", null, 2)]
    public BigInteger Id { get; set; }

    [ParameterAttribute("bytes", null, 3)]
    public byte[] Value { get; set; }

    public override string ToString() {
        return "AddCat " + Id + " => " + User;
    }
}
}
