using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Game.CSV {
public class Container : CSVIO.Loader {
    public Dictionary<uint, Ch.CardSpec> CardSpecs = new Dictionary<uint, Ch.CardSpec>();
    public override string[] CSVNames() {
        return new string[] { "CardSpec" };
    }
	public override IEnumerator Load(string basepath) {
        yield return StartCoroutine(CardSpecLoader.Load(basepath + "CardSpecs.csv", CardSpecs));
        //...
    }
}
}