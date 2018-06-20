using System.Collections;
using System.Collections.Generic;
using System.Numerics;

using UnityEngine;

namespace Game.ViewModel {
public class Inventory {
    public List<KeyValuePair<BigInteger, Ch.Card>> Cards {
        get; private set;
    }

    public int debugDupCount = 0;

    public Inventory() {
        Cards = new List<KeyValuePair<BigInteger, Ch.Card>>();
    }

    protected void Sort() {
		Cards.Sort(delegate (KeyValuePair<BigInteger, Ch.Card> a, KeyValuePair<BigInteger, Ch.Card> b) {
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
    public void AddCard(BigInteger id, Ch.Card cat) {
    	Cards.Add(new KeyValuePair<BigInteger, Ch.Card>(id, cat));
#if UNITY_EDITOR
        for (int i = 0; i < debugDupCount; i++) {
            Cards.Add(new KeyValuePair<BigInteger, Ch.Card>(id + i, cat));
        }
#endif
    	Sort();
    }
    public void RemoveCard(BigInteger id) {
    	foreach (var kv in Cards) {
    		if (kv.Key == id) {
    			Cards.Remove(kv);
    			Sort();
    			return;
    		}
    	}
    }
}
}
