#!/bin/bash

NODES=$(kubectl get node -o json | jq -r .items[].status.addresses[0].address)
if [ ${#NODES[@]} -le 1 ]; then
	echo "no need to create mesh"
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
	curl --stderr /dev/null --data ${body} -H "Content-Type: application/json" -X POST $node:30545 | jq -r .result
}

register_enode_to() {
	local node=$1
	local enode=$2
	local body=$(cat << BODY
{"jsonrpc":"2.0","method":"parity_addReservedPeer","params":["${enode}"],"id":0}
BODY
)
	# will write json output like {"jsonrpc":"2.0","result":"0x00bd138abd70e2f00903268f3db08f2d25677c9e","id":0}
	curl --stderr /dev/null --data ${body} -H "Content-Type: application/json" -X POST $node:30545 | jq -r .result
}

# create mesh
for a in ${NODES} ; do 
	local enode=`get_enode_of ${a}`
	for b in ${NODES} ; do 
		register_enode_to $b $enode
	done
done
