#### generate contract ABI
abi:
	@make -C ../dapp compile_if_not
	@docker run --rm \
		-v `pwd`/../dapp/build/contracts:/in -v `pwd`/Assets/Resources/Contracts:/out \
		realguess/jq sh -c "cat /in/World.json | jq '.abi' > /out/World.json"



#### generate csharp source from proto
DOCKERIMG=umegaya/pb3sol
PROTO_IN=../dapp/proto
PROTO_OUT=./Assets/Scripts/Proto
PROTO_FILES=$(shell cd $(PROTO_IN) && ls *.proto)
PROTO_SRCS=$(addprefix $(PROTO_OUT)/,$(PROTO_FILES:%.proto=%.g.cs))

define call_protoc
docker run --rm -v `pwd`/$(PROTO_IN):/in -v `pwd`/$(PROTO_OUT):/out $(DOCKERIMG) bash -c "cd /in && protoc -I. -I/protoc/include $1"
endef

$(PROTO_OUT)/Solidity.g.cs: 
	$(call call_protoc,--csharp_out=/out --csharp_opt=file_extension=.g.cs /protoc/include/Solidity.proto)

$(PROTO_OUT)/%.g.cs: $(PROTO_IN)/%.proto
	$(call call_protoc,--csharp_out=/out --csharp_opt=file_extension=.g.cs $(notdir $<))

proto: $(PROTO_OUT)/Solidity.g.cs $(PROTO_SRCS)

