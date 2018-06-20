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
PROTO_OUT=./Assets/Scripts/Proto
PROTO_FILES=$(shell cd $(PROTO_IN) && ls *.proto)
PROTO_SRCS=$(addprefix $(PROTO_OUT)/,$(PROTO_FILES:%.proto=%.g.cs))

define protoc
docker run --rm -v `pwd`/$(PROTO_IN):/in -v `pwd`/$(PROTO_OUT):/out $(PROTO_CONTAINER) bash -c "cd /in && protoc -I. -I/protoc/include $1"
endef

$(PROTO_OUT)/Solidity.g.cs: 
	$(call protoc,--csharp_out=/out --csharp_opt=file_extension=.g.cs /protoc/include/Solidity.proto)

$(PROTO_OUT)/%.g.cs: $(PROTO_IN)/%.proto
	$(call protoc,--csharp_out=/out --csharp_opt=file_extension=.g.cs $(notdir $<))

proto: $(PROTO_OUT)/Solidity.g.cs $(PROTO_SRCS)

