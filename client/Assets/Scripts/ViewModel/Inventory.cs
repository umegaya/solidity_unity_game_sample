using System.Collections;
using System.Collections.Generic;
using System.Numerics;

using UnityEngine;

namespace Game.ViewModel {
public class Inventory {
    public Dictionary<BigInteger, Neko.Cat> Cats {
        get; private set;
    }

    public Inventory() {
        Cats = new Dictionary<BigInteger, Neko.Cat>();
    }
}
}
