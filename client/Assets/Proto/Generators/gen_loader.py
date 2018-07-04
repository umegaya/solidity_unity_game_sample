#!/usr/bin/env python

import os, sys, glob
#import pprint
#pp = pprint.PrettyPrinter(indent=4, stream=sys.stderr)

from google.protobuf.compiler import plugin_pb2 as plugin
from google.protobuf.descriptor_pb2 import FieldOptions, DescriptorProto, EnumDescriptorProto

Num2Type = {
    1: "double",
    2: "float",
    3: "long",
    4: "ulong",
    5: "int",
    6: "uint",
    7: "ulong",
    8: "bool",
    9: "string",
    12: "byte[]", # bytes
    13: "uint",
    14: "string",
    15: "int",
    16: "long",
    17: "int",
    18: "long",
}
def fieldtype(field):
    val = Num2Type.get(field.type, None)
    if val != None:
        return val
    return field.type

def gen_typename(proto_file, msg):
    return proto_file.package[0:1].upper() + proto_file.package[1:] + "." + msg.name

class ExtensionFinder:
    def __init__(self, proto_files):
        self.protos = proto_files
    
    def find(self, field_opts, specifier):
        parts = specifier.split(".")
        h = None
        for tpl in field_opts.ListFields():
            f = tpl[0] 
            if f.name == parts[-2]:
                h = f
        if h and field_opts.HasExtension(h):
            e = field_opts.Extensions[h]
            for tpl in e.ListFields():
                f = tpl[0]
                if f.name == parts[-1]:
                    #pp.pprint('find value of {0}/{1}'.format(parts[-1], specifier))
                    return tpl[1]
        return None

'''
//generated code sample
public class CardSpecLoader {
    public Dictionary<uint, Ch.CardSpec> Records = new Dictionary<uint, Ch.CardSpec>();
	public IEnumerator Load(string path) {
		return CSVIO.Load<uint, Ch.CardSpec>(path, Records, r => r.Id);
	}
}
'''
def generate_code(request, response):
    sysmsgs = []
    ef = ExtensionFinder(request.proto_file)
    for proto_file in request.proto_file:
        output = []

        # generate header
        output.append('using System.Collections;')
        output.append('using System.Collections.Generic;')
        output.append('namespace Game.CSV {');

        # generate loader
        I="\t"
        processed = 0
        for msg in proto_file.message_type:
            typename = gen_typename(proto_file, msg)
            if proto_file.package == "google.protobuf" or proto_file.package == "suntomi.pb":
                sysmsgs.append(msg)
            elif isinstance(msg, DescriptorProto):
                processed = processed + 1
                field_count = 0
                id_field = None
                for f in msg.field:
                    field_count = field_count + 1
                    opts = f.options
                    if ef.find(opts, "suntomi.pb.csv_schema.id") == True:
                        id_field = f
                key_type = fieldtype(id_field) if id_field else "uint"
                dict_type = "Dictionary<{0}, {1}>".format(key_type, typename)
                if field_count > 0:
                    csignature = "public class {0}Loader {{".format(msg.name)
                    member = "public {0} Records = new {0}();".format(dict_type)
                    fsignature = "public IEnumerator Load(string path) {"
                    fbody = "return CSVIO.Load<{0}, {1}>(path, Records, r => r.{2});".format(
                        key_type, typename, 
                        (id_field.name[0:1].upper() + id_field.name[1:]) if id_field else "Id")
                    output.append(csignature)
                    output.append(I+member)
                    output.append(I+fsignature)
                    output.append(I+I+fbody)
                    output.append(I+"}")
                    output.append("}")
        output.append('} //namespace Game.CSV')
        
        if processed > 0:
            # Fill response
            basepath = os.path.basename(proto_file.name)
            f = response.file.add()
            f.name = basepath.replace('.proto', '.CSVLoader.cs')
            f.content = '\n'.join(output)

if __name__ == '__main__':
    # if extension dirs given, import them first
    extdir = os.getenv("PB_EXT_DIR")
    modules = None
    if extdir:
        sys.path.append(extdir)
        mods = []
        for py in glob.glob(extdir + "/*.py"):
            mods.append(os.path.basename(py)[0:-3])
        #pp.pprint("import extensions {0}".format(mods))
        modules = map(__import__, mods)

    # Read request message from stdin
    data = sys.stdin.read()

    # Parse request
    request = plugin.CodeGeneratorRequest()
    request.ParseFromString(data)

    # Create response
    response = plugin.CodeGeneratorResponse()

    # Generate code
    generate_code(request, response)

    # Serialise response message
    output = response.SerializeToString()

    # Write to stdout
    sys.stdout.write(output)
