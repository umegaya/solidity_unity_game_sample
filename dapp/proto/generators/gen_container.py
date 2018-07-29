import os, sys, itertools, json, re, glob

"""
//generated code overview
public class Container : UnityLoader {
	public CardSpecLoader CardSpec = new CardSpecLoader();
	public ArenaLoader Arena = new ArenaLoader();
	public override IEnumerator Load(Dictionary<string, string> locs) {
		yield return CardSpec.Load(this, locs["CardSpec"]);if (Error != null) { yield break; }
		yield return Arena.Load(this, locs["Arena"]);if (Error != null) { yield break; }
	}
}
"""
# returns code
def generate_code(dir):
    ps = []
    output = []
    I="\t"
    for f in glob.glob(dir + "/*.proto"):
        ps.append(os.path.basename(f)[0:-6])

    # generate header
    output.append("using System.Collections;")
    output.append("using System.Collections.Generic;")
    output.append("using UnityEngine;")
    output.append("namespace Game.CSV {");
    output.append("public class Container : UnityLoader {");

    # generate loader instances
    for p in ps:
        output.append(I+("public {0}Loader {0} = new {0}Loader();").format(p))

    # generate loader
    output.append(I+"public override IEnumerator Load(Dictionary<string, string> locs) {")
    for p in ps:
        output.append(I+I+(
            "yield return {0}.Load(this, locs[\"{0}\"]);" + 
            "if (Error != null) {{ yield break; }}"
        ).format(p))
    output.append(I+"}")
    output.append("}")
    output.append("}")

    return '\n'.join(output)

if __name__ == "__main__":
    output = generate_code(sys.argv[1])

    with open(sys.argv[2],"w+") as f:
        f.write(output)