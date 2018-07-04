using System.Collections;
using System.Collections.Generic;
namespace Game.CSV {
public class CardSpecLoader {
	static public IEnumerator Load(string path, Dictionary<uint, Ch.CardSpec> map) {
		return CSVIO.Load<uint, Ch.CardSpec>(path, map, r => r.Id);
	}
}
} //namespace Game.CSV