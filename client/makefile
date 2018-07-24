#### generate contract ABI
ABI_CONTAINER=realguess/jq
ABI_IN=../dapp/build/contracts
ABI_OUT=./Assets/Resources/Contracts
ABI_SRCS=$(addprefix $(ABI_OUT)/,World.json Inventory.json Moritapo.json History.json DataContainer.json)
ADDR_IN=../dapp/build/addresses
ADDR_OUT=./Assets/Resources/Contracts
ADDR_SRCS=$(addprefix $(ADDR_OUT)/,dev.json)

define strip_abi
	docker run --rm \
		-v `pwd`/$(ABI_IN):/in -v `pwd`/$(ABI_OUT):/out \
		$(ABI_CONTAINER) sh -c "cat /in/$1.json | jq '.abi' > /out/$1.json"
endef

$(ABI_OUT)/%.json: $(ABI_IN)/%.json
	$(call strip_abi,$(basename $(notdir $<)))

$(ADDR_OUT)/%.json: $(ADDR_IN)/%.json
	cp $< $@

compile_json: 
	@make -C ../dapp compile_if_not

abi: compile_json $(ABI_SRCS) $(ADDR_SRCS)


#### generate csharp source from proto
PROTO_CONTAINER=umegaya/pb3sol
PROTO_IN=../dapp/proto
PROTO_OUT=./Assets/Scripts/Proto/Dapp
PROTO_FILES=$(shell cd $(PROTO_IN) && ls *.proto)
PROTO_SRCS=$(addprefix $(PROTO_OUT)/,$(PROTO_FILES:%.proto=%.g.cs))

CLIENT_PROTO_IN=./Assets/Proto
CLIENT_PROTO_OUT=./Assets/Scripts/Proto/Client
CLIENT_PROTO_FILES=$(shell cd $(CLIENT_PROTO_IN) && ls *.proto)
CLIENT_PROTO_SRCS=$(addprefix $(CLIENT_PROTO_OUT)/,$(CLIENT_PROTO_FILES:%.proto=%.g.cs))

CSV_PROTO_IN=../dapp/proto
CSV_PROTO_OUT=./Assets/Scripts/Proto/Client/Templates
CSV_PROTO_FILES=$(shell cd $(CSV_PROTO_IN)/templates/ && ls *.proto)
CSV_LOADER_SRCS=$(addprefix $(CSV_PROTO_OUT)/,$(CSV_PROTO_FILES:%.proto=%.Loader.cs))
CSV_PLUGIN=./generators/gen_loader.py

CSV_CONTAINER_IN=../dapp/proto
CSV_CONTAINER_OUT=$(CSV_PROTO_OUT)
CSV_CONTAINER_FILES=$(shell ls $(CSV_PROTO_IN)/*.proto)
CSV_CONTAINER_PLUGIN=./Generators/gen_container.py
CSV_CONTAINER_SRCS=$(CSV_CONTAINER_OUT)/Container.cs

EXT_PROTO_IN=../dapp/proto/options
EXT_PY_OUT=../dapp/proto/options
EXT_PROTO_OUT=./Assets/Scripts/Proto/Client/Options
EXT_PROTO_FILES=$(shell cd $(EXT_PROTO_IN) && ls *.proto)
EXT_PY_SRCS=$(addprefix $(EXT_PY_OUT)/,$(EXT_PROTO_FILES:%.proto=%_pb2.py))
EXT_PROTO_SRCS=$(addprefix $(EXT_PROTO_OUT)/,$(EXT_PROTO_FILES:%.proto=%.g.opt.cs))

define proto_out
docker run --rm -v `pwd`/$(PROTO_IN):/in -v `pwd`/$(PROTO_OUT):/out $(PROTO_CONTAINER) bash -c "cd /in && protoc -I. -I/protoc/include $1"
endef

define client_proto_out
docker run --rm -v `pwd`/$(CLIENT_PROTO_IN):/in -v `pwd`/$(CLIENT_PROTO_OUT):/out $(PROTO_CONTAINER) bash -c "cd /in && protoc -I. $1"
endef

define csv_proto_out
docker run --rm -v `pwd`/$(CSV_PROTO_IN):/in -v `pwd`/$(CSV_PROTO_OUT):/out -e PB_EXT_DIR=./options $(PROTO_CONTAINER) bash -c "cd /in && protoc -I. -I./options $1"
endef

define ext_py_out
docker run --rm -v `pwd`/$(EXT_PROTO_IN):/in -v `pwd`/$(EXT_PY_OUT):/out $(PROTO_CONTAINER) bash -c "cd /in && protoc -I. $1"
endef

define ext_proto_out
docker run --rm -v `pwd`/$(EXT_PROTO_IN):/in -v `pwd`/$(EXT_PROTO_OUT):/out $(PROTO_CONTAINER) bash -c "cd /in && protoc -I. $1"
endef

define csv_container_src
docker run --rm -v `pwd`/$(CSV_CONTAINER_IN):/in -v `pwd`/$(CSV_CONTAINER_OUT):/out $(PROTO_CONTAINER) bash -c "cd /in && $1"
endef

$(CLIENT_PROTO_OUT)/%.g.cs: $(CLIENT_PROTO_IN)/%.proto
	$(call client_proto_out,--csharp_out=/out --csharp_opt=file_extension=.g.cs $(notdir $<))

$(CSV_PROTO_OUT)/%.Loader.cs: $(CSV_PROTO_IN)/templates/%.proto
	$(call csv_proto_out,--custom_out=/out --plugin=protoc-gen-custom=${CSV_PLUGIN} templates/$(notdir $<))
	$(call csv_proto_out,--csharp_out=/out --csharp_opt=file_extension=.g.cs templates/$(notdir $<))

$(EXT_PY_OUT)/%_pb2.py: $(EXT_PROTO_IN)/%.proto
	$(call ext_py_out,--python_out=/out $(notdir $<))

$(EXT_PROTO_OUT)/%.g.opt.cs: $(EXT_PROTO_IN)/%.proto
	$(call ext_proto_out,--csharp_out=/out --csharp_opt=file_extension=.g.opt.cs $(notdir $<))

$(PROTO_OUT)/Solidity.g.cs: 
	$(call proto_out,--csharp_out=/out --csharp_opt=file_extension=.g.cs /protoc/include/Solidity.proto)

$(PROTO_OUT)/%.g.cs: $(PROTO_IN)/%.proto
	$(call proto_out,--csharp_out=/out --csharp_opt=file_extension=.g.cs $(notdir $<))

$(CSV_CONTAINER_SRCS): $(CSV_CONTAINER_FILES)
	$(call csv_container_src,python $(CSV_CONTAINER_PLUGIN) /in/templates /out/$(notdir $@))

proto: $(PROTO_OUT)/Solidity.g.cs $(PROTO_SRCS) $(CLIENT_PROTO_SRCS)

csv: $(CSV_LOADER_SRCS) $(CSV_CONTAINER_SRCS)

ext: $(EXT_PROTO_SRCS) $(EXT_PY_SRCS)

code: ext csv proto
