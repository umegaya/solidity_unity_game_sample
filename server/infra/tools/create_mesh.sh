#!/bin/bash

ROOT=$(cd $(dirname $0) && pwd)/..
source ${ROOT}/tools/common.sh ${ROOT}

NODES=($(node_list | jq -r .address))
if [ ${#NODES[@]} -le 1 ]; then
	echo "no need to create mesh for [${NODES}]"
	exit 0
fi

echo "create mesh of ${#NODES[@]} nodes"

get_enode_of() {
	local node=$1
	local body=$(cat << BODY
{"jsonrpc":"2.0","method":"parity_enode","params":[],"id":0}
BODY
)
	# will write json output like {"jsonrpc":"2.0","result":"0x00bd138abd70e2f00903268f3db08f2d25677c9e","id":0}
	jsonrpc ${body} $node | jq -r .result | sed -e "s/@[\.0-9]*:/@${node}:/"
}

register_enode_to() {
	local node=$1
	local enode=$2
	local body=$(cat << BODY
{"jsonrpc":"2.0","method":"parity_addReservedPeer","params":["${enode}"],"id":0}
BODY
)
	# will write json output like {"jsonrpc":"2.0","result":"0x00bd138abd70e2f00903268f3db08f2d25677c9e","id":0}
	jsonrpc ${body} $node
}

# create mesh
for a in ${NODES[@]} ; do 
	bash ${ROOT}/tools/wait_node.sh ${a}
	enode=`get_enode_of ${a}`
	echo "enode=${enode}"
	for b in ${NODES[@]} ; do 
		if  [ "$a" != "$b" ]; then
			echo "register $a => $b"
			register_enode_to $b $enode > /dev/null
		fi
	done
done
