INPUT_DIR=/in
OUTPUT_DIR=/out
CONTAINER=umegaya/pb3sol
CONTAINER_RUNARG=docker run --rm -v `pwd`/contracts/libs/pb:${OUTPUT_DIR} -v `pwd`/proto:${INPUT_DIR} ${CONTAINER}
SOLIDITY_PLUGIN=/protoc/plugin/gen_sol.py

LOCAL_PROTO_SRC_DIR=./proto
LOCAL_PROTO_DST_DIR=./contracts/libs/pb
PROTO_SRC=$(notdir $(shell ls $(LOCAL_PROTO_SRC_DIR)/*.proto))
PROTO_TEMPLATE_SRC=$(notdir $(shell ls $(LOCAL_PROTO_SRC_DIR)/templates/*.proto))
PROTO_SOL=$(addprefix $(LOCAL_PROTO_DST_DIR)/,$(PROTO_SRC:%.proto=%_pb.sol))
PROTO_TEMPLATE_SOL=$(addprefix $(LOCAL_PROTO_DST_DIR)/templates/,$(PROTO_TEMPLATE_SRC:%.proto=%_pb.sol))

K8S_PLATFORM?=


define soliditize
${CONTAINER_RUNARG} protoc -I/ -I/in -I/in/templates -I/in/options -I/protoc/include --plugin=protoc-gen-sol=${SOLIDITY_PLUGIN} --sol_out=$2 $1
endef

$(LOCAL_PROTO_DST_DIR)/runtime.sol: $(LOCAL_PROTO_SRC_DIR)/User.proto
	$(call soliditize,$(INPUT_DIR)/$(subst $(LOCAL_PROTO_SRC_DIR),,./$<),gen_runtime=True:$(OUTPUT_DIR)/$(dir $(subst $(LOCAL_PROTO_DST_DIR),,./$@)))

$(LOCAL_PROTO_DST_DIR)/%_pb.sol: $(LOCAL_PROTO_SRC_DIR)/%.proto
	$(call soliditize,$(INPUT_DIR)/$(subst $(LOCAL_PROTO_SRC_DIR),,./$<),gen_runtime=False:$(OUTPUT_DIR)/$(dir $(subst $(LOCAL_PROTO_DST_DIR),,./$@)))

.PHONY: proto
proto: $(LOCAL_PROTO_DST_DIR)/runtime.sol $(PROTO_SOL) $(PROTO_TEMPLATE_SOL)

test: proto
	truffle develop --log
	truffle test

compile_if_not: proto
	@[[ -e ./build/contracts ]] || truffle compile

migrate: proto
	truffle migrate --reset

deploy_code: migrate
	CONFIG_NAME=$(K8S_PLATFORM) truffle exec ./tools/admin/make_address_map.js

deploy_records: proto
	truffle compile
	CONFIG_NAME=$(K8S_PLATFORM) truffle exec ./tools/admin/make_records.js
