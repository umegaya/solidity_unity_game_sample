import os, sys, itertools, json, re, glob

"""
//generated code overview
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.CSV {
public class Container : CSVIO.Loader {
    public CardSpecLoader CardSpec = new CardSpecLoader();
    public override string[] CSVNames() {
        return new string[] { "CardSpec" };
    }
	public override IEnumerator Load(string basepath) {
        yield return StartCoroutine(CardSpec.Load(basepath + "CardSpecs.csv"));
        //...
    }
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
    output.append("public class Container : CSVIO.Loader {");

    # generate loader instances
    for p in ps:
        output.append(I+("public {0}Loader {0} = new {0}Loader();").format(p))

    # generate CSVNames
    output.append(I+"public override string[] CSVNames() {")
    output.append(I+I+("return new string[] {{ \"{0}\" }};").format("\",\"".join(ps)))
    output.append(I+"}")

    # generate loader
    output.append(I+"public override IEnumerator Load(CSVIO.Loader loader, string basepath) {")
    for p in ps:
        output.append(I+I+(
            "yield return StartCoroutine({0}.Load(loader, basepath + \"{0}.csv\"));" + 
            "if (loader.Error != null) {{ yield break; }}"
        ).format(p))
    output.append(I+"}")
    output.append("}")
    output.append("}")

    return '\n'.join(output)

if __name__ == "__main__":
    output = generate_code(sys.argv[1])

    with open(sys.argv[2],"w+") as f:
        f.write(output)