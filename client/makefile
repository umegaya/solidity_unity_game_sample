#### generate contract ABI
ABI_CONTAINER=realguess/jq
ABI_IN=../dapp/build/contracts
ABI_OUT=./Assets/Resources/Contracts
ABI_SRCS=$(addprefix $(ABI_OUT)/,World.json Inventory.json Moritapo.json History.json)
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

CSV_PROTO_IN=./Assets/Proto/Templates
CSV_PROTO_OUT=./Assets/Scripts/Proto/Client/Templates
CSV_PROTO_FILES=$(shell cd $(CSV_PROTO_IN) && ls *.proto)
CSV_PROTO_SRCS=$(addprefix $(CSV_PROTO_OUT)/,$(CSV_PROTO_FILES:%.proto=%.CSVLoader.cs))
CSV_PLUGIN=./Generators/gen_loader.py

CSV_CONTAINER_IN=./Assets/Proto
CSV_CONTAINER_OUT=$(CSV_PROTO_OUT)
CSV_CONTAINER_FILES=$(shell ls $(CSV_PROTO_IN)/*.proto)
CSV_CONTAINER_PLUGIN=./Generators/gen_container.py
CSV_CONTAINER_SRCS=$(CSV_CONTAINER_OUT)/Container.cs

EXT_PROTO_IN=./Assets/Proto
EXT_PROTO_OUT=./Assets/Proto
EXT_PROTO_FILES=$(shell cd $(EXT_PROTO_IN)/Options && ls *.proto)
EXT_PROTO_SRCS=$(addprefix $(EXT_PROTO_OUT)/Options/,$(EXT_PROTO_FILES:%.proto=%_pb2.py))

define protoc
docker run --rm -v `pwd`/$(PROTO_IN):/in -v `pwd`/$(PROTO_OUT):/out $(PROTO_CONTAINER) bash -c "cd /in && protoc -I. -I/protoc/include $1"
endef

define cprotoc
docker run --rm -v `pwd`/$(CLIENT_PROTO_IN):/in -v `pwd`/$(CLIENT_PROTO_OUT):/out $(PROTO_CONTAINER) bash -c "cd /in && protoc -I. -I./Options $1"
endef

define protocsv
docker run --rm -v `pwd`/$(CLIENT_PROTO_IN):/in -v `pwd`/$(CSV_PROTO_OUT):/out -e PB_EXT_DIR=./Options $(PROTO_CONTAINER) bash -c "cd /in && protoc -I. -I./Options $1"
endef

define protopy
docker run --rm -v `pwd`/$(EXT_PROTO_IN):/in -v `pwd`/$(EXT_PROTO_OUT):/out $(PROTO_CONTAINER) bash -c "cd /in && protoc -I. $1"
endef

define ctgen
docker run --rm -v `pwd`/$(CSV_CONTAINER_IN):/in -v `pwd`/$(CSV_CONTAINER_OUT):/out $(PROTO_CONTAINER) bash -c "cd /in && $1"
endef

$(CLIENT_PROTO_OUT)/%.g.cs: $(CLIENT_PROTO_IN)/%.proto
	$(call cprotoc,--csharp_out=/out --csharp_opt=file_extension=.g.cs $(notdir $<))

$(CSV_PROTO_OUT)/%.CSVLoader.cs: $(CSV_PROTO_IN)/%.proto
	$(call protocsv,--custom_out=/out --plugin=protoc-gen-custom=${CSV_PLUGIN} Templates/$(notdir $<))
	$(call protocsv,--csharp_out=/out --csharp_opt=file_extension=.g.cs Templates/$(notdir $<))

$(EXT_PROTO_OUT)/Options/%_pb2.py: $(EXT_PROTO_IN)/Options/%.proto
	$(call protopy,--python_out=/out ./Options/$(notdir $<))
	$(call cprotoc,--csharp_out=/out/Options --csharp_opt=file_extension=.g.cs ./Options/$(notdir $<))

$(PROTO_OUT)/Solidity.g.cs: 
	$(call protoc,--csharp_out=/out --csharp_opt=file_extension=.g.cs /protoc/include/Solidity.proto)

$(PROTO_OUT)/%.g.cs: $(PROTO_IN)/%.proto
	$(call protoc,--csharp_out=/out --csharp_opt=file_extension=.g.cs $(notdir $<))

$(CSV_CONTAINER_SRCS): $(CSV_CONTAINER_FILES)
	$(call ctgen,python $(CSV_CONTAINER_PLUGIN) /in/Templates /out/$(notdir $@))

proto: $(PROTO_OUT)/Solidity.g.cs $(PROTO_SRCS) $(CLIENT_PROTO_SRCS)

csv: $(CSV_PROTO_SRCS) $(CSV_CONTAINER_SRCS)

ext: $(EXT_PROTO_SRCS)

code: ext csv proto
