#!/usr/bin/env python

import os, sys, itertools, json, re, glob
import pprint
pp = pprint.PrettyPrinter(indent=4, stream=sys.stderr)

from google.protobuf.compiler import plugin_pb2 as plugin
from google.protobuf.descriptor_pb2 import FieldOptions, DescriptorProto, EnumDescriptorProto

def traverse(proto_file):
    def _traverse(package, items):
        for item in items:
            yield item, package

            if isinstance(item, DescriptorProto):
                for enum in item.enum_type:
                    yield enum, package

                for nested in item.nested_type:
                    nested_package = package + item.name

                    for nested_item in _traverse(nested, nested_package):
                        yield nested_item, nested_package

    return itertools.chain(
        _traverse(proto_file.package, proto_file.enum_type),
        _traverse(proto_file.package, proto_file.message_type),
    )

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

def castexpr(field):
    if field.type == 14:
        return 'static_cast<{0}>({1})'.format(field.type_name.replace('.', '::'), field.name.lower())
    return field.name.lower()

def setexpr(field):
    fname = field.name.lower()
    if field.type == 14:
        enumtype = field.type_name.replace('.', '::')
        return "TO_ENUM(row,{0},{1});\n".format(fname, enumtype)
    else:
        return "row.set_{0}({0});\n".format(fname)

def camel2snake(name):
    s1 = re.sub('(.)([A-Z][a-z]+)', r'\1_\2', name)
    return re.sub('([a-z0-9])([A-Z])', r'\1_\2', s1).lower()

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

def generate_code(request, response):
    loadernames = []
    incfile_list = []
    sysmsgs = []
    container_msg = None
    container_proto_file = None
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
                if field_count > 0:
#public class CardSpecLoader {
#	static public IEnumerator Load(string path, Dictionary<uint, Ch.CardSpec> map) {
#		return CSVIO.Load<uint, Ch.CardSpec>(path, map, r => r.Id);
#	}
#}

                    csignature = "public class {0}Loader {{".format(msg.name)
                    fsignature = "static public IEnumerator Load(string path, Dictionary<{1}, {0}> map) {{".format(
                        typename, fieldtype(id_field) if id_field else "uint")
                    fbody = "return CSVIO.Load<uint, {0}>(path, map, r => r.{1});".format(
                        typename, (id_field.name[0:1].upper() + id_field.name[1:]) if id_field else "Id")
                    output.append(csignature)
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

    # generate container loader
    if container_msg != None:
        ct_output = []
        for incfile in incfile_list:
            ct_output.append('#include "{0}"'.format(incfile))
        ct_output.append('namespace mgo {')
                
        ct_typename = gen_typename(container_proto_file, container_msg)
        fsignature = "void LoadAll({0}& container, const std::string &dirname)".format(ct_typename)
        ct_output.append(fsignature + " {")
        ct_output.append("#if !defined(IN_CLIENT_SERVER)")
        for msg in loadernames:
            typename = gen_typename(proto_file, msg)
            ct_output.append(I+("Load{1}(dirname + \"/{1}.csv\", *container.mutable_{2}_data(), false);").format(
                    typename, msg.name, camel2snake(msg.name)))
        ct_output.append("#else")
        for msg in loadernames:
            typename = gen_typename(proto_file, msg)
            ct_output.append(I+("extern const std::string {0}_csv;").format(msg.name))
            ct_output.append(I+("Load{1}({1}_csv, *container.mutable_{2}_data(), true);").format(
                    typename, msg.name, camel2snake(msg.name)))
        ct_output.append("#endif")
        ct_output.append("}")

        ct_output.append("} //namespace mgo")
        f = response.file.add()
        f.name = 'Container.CSVLoader.cs'
        f.content = '\n'.join(ct_output)

if __name__ == '__main__':
    # if extension dirs given, import them first
    extdir = os.getenv("PB_EXT_DIR")
    modules = None
    if extdir:
        sys.path.append(extdir)
        mods = []
        for py in glob.glob(extdir + "/*.py"):
            mods.append(os.path.basename(py)[0:-3])
        pp.pprint("import extensions {0}".format(mods))
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
