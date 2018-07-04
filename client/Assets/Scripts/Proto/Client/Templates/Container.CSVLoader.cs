using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.CSV {
public class Container : CSVIO.Loader {
	public CardSpecLoader CardSpec = new CardSpecLoader();
	public ArenaLoader Arena = new ArenaLoader();
	public override string[] CSVNames() {
		return new string[] { "CardSpec","Arena" };
	}
	public override IEnumerator Load(string basepath) {
		yield return StartCoroutine(CardSpec.Load(basepath + "CardSpec.csv"));
		yield return StartCoroutine(Arena.Load(basepath + "Arena.csv"));
	}
}
}