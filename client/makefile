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
CSV_PLUGIN=./gen_loader.py

define protoc
docker run --rm -v `pwd`/$(PROTO_IN):/in -v `pwd`/$(PROTO_OUT):/out $(PROTO_CONTAINER) bash -c "cd /in && protoc -I. -I/protoc/include $1"
endef

define cprotoc
docker run --rm -v `pwd`/$(CLIENT_PROTO_IN):/in -v `pwd`/$(CLIENT_PROTO_OUT):/out $(PROTO_CONTAINER) bash -c "cd /in && protoc -I. $1"
endef

define protocsv
docker run --rm -v `pwd`/$(CSV_PROTO_IN):/in -v `pwd`/$(CSV_PROTO_OUT):/out $(PROTO_CONTAINER) bash -c "cd /in && protoc -I. $1"
endef

$(CLIENT_PROTO_OUT)/%.g.cs: $(CLIENT_PROTO_IN)/%.proto
	$(call cprotoc,--csharp_out=/out --csharp_opt=file_extension=.g.cs $(notdir $<))

$(CSV_PROTO_OUT)/%.CSVLoader.cs: $(CSV_PROTO_IN)/%.proto
	$(call protocsv,--custom_out=/out --plugin=protoc-gen-custom=${CSV_PLUGIN} $(notdir $<))
	$(call protocsv,--csharp_out=/out --csharp_opt=file_extension=.g.cs $(notdir $<))

$(PROTO_OUT)/Solidity.g.cs: 
	$(call protoc,--csharp_out=/out --csharp_opt=file_extension=.g.cs /protoc/include/Solidity.proto)

$(PROTO_OUT)/%.g.cs: $(PROTO_IN)/%.proto
	$(call protoc,--csharp_out=/out --csharp_opt=file_extension=.g.cs $(notdir $<))

proto: $(PROTO_OUT)/Solidity.g.cs $(PROTO_SRCS) $(CLIENT_PROTO_SRCS)

csv: $(CSV_PROTO_SRCS)

code: csv proto
