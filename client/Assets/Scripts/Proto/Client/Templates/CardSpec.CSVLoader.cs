using System.Collections;
using System.Collections.Generic;
namespace Game.CSV {
public class CardSpecLoader {
	public Dictionary<uint, Ch.CardSpec> Records = new Dictionary<uint, Ch.CardSpec>();
	public IEnumerator Load(string path) {
		return CSVIO.Load<uint, Ch.CardSpec>(path, Records, r => r.Id);
	}
}
} //namespace Game.CSV