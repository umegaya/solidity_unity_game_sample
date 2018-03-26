using System.Collections;
using System.Collections.Generic;
using System.Numerics;

using UnityEngine;

namespace Game.ViewModel {
public class Inventory {
    public List<KeyValuePair<BigInteger, Neko.Cat>> Cats {
        get; private set;
    }

    public int debugDupCount = 0;

    public Inventory() {
        Cats = new List<KeyValuePair<BigInteger, Neko.Cat>>();
    }

    protected void Sort() {
		Cats.Sort(delegate (KeyValuePair<BigInteger, Neko.Cat> a, KeyValuePair<BigInteger, Neko.Cat> b) {
			//newest come first
			if (a.Key < b.Key) {
				return 1;
			} else if (a.Key > b.Key) {
				return -1;
			} else {
				return 0;
			}
		});
    }
    public void AddCat(BigInteger id, Neko.Cat cat) {
    	Cats.Add(new KeyValuePair<BigInteger, Neko.Cat>(id, cat));
#if UNITY_EDITOR
        for (int i = 0; i < debugDupCount; i++) {
            Cats.Add(new KeyValuePair<BigInteger, Neko.Cat>(id + i, cat));
        }
#endif
    	Sort();
    }
    public void RemoveCat(BigInteger id) {
    	foreach (var kv in Cats) {
    		if (kv.Key == id) {
    			Cats.Remove(kv);
    			Sort();
    			return;
    		}
    	}
    }
}
}
