# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: CSVSchema.proto

import sys
_b=sys.version_info[0]<3 and (lambda x:x) or (lambda x:x.encode('latin1'))
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from google.protobuf import reflection as _reflection
from google.protobuf import symbol_database as _symbol_database
from google.protobuf import descriptor_pb2
# @@protoc_insertion_point(imports)

_sym_db = _symbol_database.Default()

from google.protobuf import descriptor_pb2 as google_dot_protobuf_dot_descriptor__pb2


DESCRIPTOR = _descriptor.FileDescriptor(
  name='CSVSchema.proto',
  package='suntomi.pb',
  syntax='proto3',
  serialized_pb=_b('\n\x0f\x43SVSchema.proto\x12\nsuntomi.pb\x1a google/protobuf/descriptor.proto\"\x17\n\tCSVSchema\x12\n\n\x02id\x18\x01 \x01(\x08:J\n\ncsv_schema\x12\x1d.google.protobuf.FieldOptions\x18\xd0\x86\x03 \x01(\x0b\x32\x15.suntomi.pb.CSVSchemab\x06proto3')
  ,
  dependencies=[google_dot_protobuf_dot_descriptor__pb2.DESCRIPTOR,])


CSV_SCHEMA_FIELD_NUMBER = 50000
csv_schema = _descriptor.FieldDescriptor(
  name='csv_schema', full_name='suntomi.pb.csv_schema', index=0,
  number=50000, type=11, cpp_type=10, label=1,
  has_default_value=False, default_value=None,
  message_type=None, enum_type=None, containing_type=None,
  is_extension=True, extension_scope=None,
  options=None, file=DESCRIPTOR)


_CSVSCHEMA = _descriptor.Descriptor(
  name='CSVSchema',
  full_name='suntomi.pb.CSVSchema',
  filename=None,
  file=DESCRIPTOR,
  containing_type=None,
  fields=[
    _descriptor.FieldDescriptor(
      name='id', full_name='suntomi.pb.CSVSchema.id', index=0,
      number=1, type=8, cpp_type=7, label=1,
      has_default_value=False, default_value=False,
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      options=None, file=DESCRIPTOR),
  ],
  extensions=[
  ],
  nested_types=[],
  enum_types=[
  ],
  options=None,
  is_extendable=False,
  syntax='proto3',
  extension_ranges=[],
  oneofs=[
  ],
  serialized_start=65,
  serialized_end=88,
)

DESCRIPTOR.message_types_by_name['CSVSchema'] = _CSVSCHEMA
DESCRIPTOR.extensions_by_name['csv_schema'] = csv_schema
_sym_db.RegisterFileDescriptor(DESCRIPTOR)

CSVSchema = _reflection.GeneratedProtocolMessageType('CSVSchema', (_message.Message,), dict(
  DESCRIPTOR = _CSVSCHEMA,
  __module__ = 'CSVSchema_pb2'
  # @@protoc_insertion_point(class_scope:suntomi.pb.CSVSchema)
  ))
_sym_db.RegisterMessage(CSVSchema)

csv_schema.message_type = _CSVSCHEMA
google_dot_protobuf_dot_descriptor__pb2.FieldOptions.RegisterExtension(csv_schema)

# @@protoc_insertion_point(module_scope)
