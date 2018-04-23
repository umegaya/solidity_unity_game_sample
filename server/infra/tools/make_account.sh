#!/bin/bash

ROOT=$(cd $(dirname $0) && pwd)/..
source ${ROOT}/tools/common.sh ${ROOT}

SECRET_ROOT=${ROOT}/volume/secret/${K8S_PLATFORM}

if [ "${K8S_PLATFORM}" = "dev" ]; then
	NODE_ADDR=$1
	bash ${ROOT}/tools/wait_node.sh ${NODE_ADDR}
fi

ROOT=`dirname $0`/..
rm -f ${SECRET_ROOT}/*.addr
rm -f ${SECRET_ROOT}/*.pass

create_account() {
	local phrase=$1
	local pass=$2
	local out=
	if [ ! -z "$4" ]; then
		out="${SECRET_ROOT}/node-$4"
	else
		out="${SECRET_ROOT}/user"
	fi
	local body=$(cat << BODY
{"jsonrpc":"2.0","method":"parity_newAccountFromPhrase","params":["${phrase}","${pass}"],"id":0}
BODY
)
	# will write json output like {"jsonrpc":"2.0","result":"0x00bd138abd70e2f00903268f3db08f2d25677c9e","id":0}
	jsonrpc ${body} $3 | jq -r .result > ${out}.addr
	echo ${pass} > ${out}.pass
}


NODE_LIST=$(node_list)
NODES=($(echo ${NODE_LIST} | jq -r .address))
MACHINES=($(echo ${NODE_LIST} | jq -r .machineID))

echo "NODES=(${NODES[@]}), MACHINES=(${MACHINES[@]})"

VALIDATOR_ADDRESSES=()
USER_CREATION_NODE=


# ----------------------------------
# authority address for each nodes
# ----------------------------------
for idx in ${!NODES[@]} ; do 
	a=${NODES[$idx]}
	m=${MACHINES[$idx]}
	create_account `ps auwx | md5sum | awk '{print $1}'` `ps auwx | sha1sum | awk '{print $1}'` $a $m
	VALIDATOR_ADDRESSES+=(`cat ${SECRET_ROOT}/node-$m.addr`)
	# get user creation node
	if [ -z "${USER_CREATION_NODE}" ]; then
		USER_CREATION_NODE=$a
	fi
done


# ----------------------------------
# genesis user
# ----------------------------------
create_account `ps auwx | md5sum | awk '{print $1}'` `ps auwx | sha1sum | awk '{print $1}'` $USER_CREATION_NODE


# ----------------------------------
# create node settings
# ----------------------------------
for idx in ${!MACHINES[@]} ; do
	cat ${ROOT}/volume/config/path1.toml.tmpl \
		| sed -e "s/__SIGNER__/${VALIDATOR_ADDRESSES[$idx]}/" \
		| sed -e "s/__PLATFORM__/${K8S_PLATFORM}/" \
		| sed -e "s/__EXTIP__/${NODES[$idx]}/" \
	> ${ROOT}/volume/config/${MACHINES[$idx]}.toml
done
