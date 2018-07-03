#!/usr/bin/env python

import os, sys, itertools, json, re

from google.protobuf.compiler import plugin_pb2 as plugin
from google.protobuf.descriptor_pb2 import DescriptorProto, EnumDescriptorProto

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
    3: "int64_t",
    4: "uint64_t",
    5: "int32_t",
    6: "uint32_t",
    7: "uint64_t",
    8: "bool",
    9: "std::string",
    12: "std::string", # bytes
    13: "uint32_t",
    14: "std::string",
    15: "int32_t",
    16: "int64_t",
    17: "int32_t",
    18: "int64_t",
}
def fieldtype(field):
    val = Num2Type.get(field.type, None)
    if val != None:
        return val
    return field.type

def cpp_typename(proto_file, msg):
    return "::" + proto_file.package.replace(".", "::") + "::" + msg.name

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

def generate_code(request, response):
    loadernames = []
    incfile_list = []
    container_msg = None
    container_proto_file = None
    for proto_file in request.proto_file:
        output = []
        incfile = os.path.basename(proto_file.name).replace('.proto', '.loader.h')

        # generate header
        output.append('#include "{0}"'.format(incfile))
        output.append('#include "LoaderConfig.h"')
        output.append('namespace mgo {')

        # generate loader
        I="\t"
        processed = 0
        for msg in proto_file.message_type:
            if msg.name == "Container":
                container_msg = msg
                container_proto_file = proto_file
            elif isinstance(msg, DescriptorProto):
                processed = processed + 1
                typename = cpp_typename(proto_file, msg)
                field_count = 0
                for f in msg.field:
                    field_count = field_count + 1
                if field_count > 0:
                    fsignature = "void Load{1}(const std::string &filename, google::protobuf::Map<uint32_t, {0}> &store, bool ischunk)".format(
                        typename, msg.name)
                    output.append(fsignature + " {")
                    output.append(I+"std::istringstream strm(filename);")
                    output.append(I+("MyReader<{0}> *csv = ischunk ? new MyReader<{0}>(\"{1}\", strm) : new MyReader<{0}>(filename);".format(field_count, msg.name)))
                    line = ("\"{0}\"".format(msg.field[0].name))
                    for f in msg.field[1:]:
                        line = line + (", \"{0}\"".format(f.name))
                    output.append(I+"csv->read_header(io::ignore_extra_column, {0});".format(line))
                    line = I+("{0} {1}".format(fieldtype(msg.field[0]), msg.field[0].name.lower()))
                    for f in msg.field[1:]: 
                        field_name = f.name.lower()
                        line = line + (";\n"+I+"{0} {1}".format(fieldtype(f), field_name))
                    line = line + ";"
                    output.append(line)
                    args = msg.field[0].name.lower()
                    sets = setexpr(msg.field[0])#("row.set_{0}({1});\n".format(msg.field[0].name.lower(), castexpr(msg.field[0])))
                    for f in msg.field[1:]:
                        field_name = f.name.lower()
                        args = args + (", {0}".format(field_name))
                        sets = sets + I+I + setexpr(f) #("row.set_{0}({1});\n".format(field_name, castexpr(f)))
                    output.append(I+("while(csv->read_row({0}))".format(args) + " {"))
                    output.append(I+I+("{0} row;".format(typename)))
                    output.append(I+I+sets)
                    output.append(I+I+"store[row.id()] = row;")
                    output.append(I+"}")
                    output.append(I+"delete csv;")
                    output.append("}")
                    loadernames.append(msg)

        output.append('} //namespace mgo')
        
        if processed > 0:
            incfile_list.append(incfile)
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
                
        ct_typename = cpp_typename(container_proto_file, container_msg)
        fsignature = "void LoadAll({0}& container, const std::string &dirname)".format(ct_typename)
        ct_output.append(fsignature + " {")
        ct_output.append("#if !defined(IN_CLIENT_SERVER)")
        for msg in loadernames:
            typename = cpp_typename(proto_file, msg)
            ct_output.append(I+("Load{1}(dirname + \"/{1}.csv\", *container.mutable_{2}_data(), false);").format(
                    typename, msg.name, camel2snake(msg.name)))
        ct_output.append("#else")
        for msg in loadernames:
            typename = cpp_typename(proto_file, msg)
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
