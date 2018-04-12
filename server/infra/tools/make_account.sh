#!/bin/bash

NODE_ADDR=$1
OUT=$2
source `dirname $0`/wait_node.sh ${NODE_ADDR}

ROOT=`dirname $0`/..
rm -f ${ROOT}/volume/secret/*.addr
rm -f ${ROOT}/volume/secret/*.pass

create_account() {
	local phrase=$1
	local pass=$2
	local out=
	if [ -z "$4" ]; then
		out="${OUT}/node-$3"
	else
		out="${OUT}/user"
	fi
	local body=$(cat << BODY
{"jsonrpc":"2.0","method":"parity_newAccountFromPhrase","params":["${phrase}","${pass}"],"id":0}
BODY
)
	# will write json output like {"jsonrpc":"2.0","result":"0x00bd138abd70e2f00903268f3db08f2d25677c9e","id":0}
	curl --stderr /dev/null --data ${body} -H "Content-Type: application/json" -X POST ${NODE_ADDR}:30545 | jq -r .result > ${out}.addr
	echo ${pass} > ${out}.pass
}

# authority address for each nodes
USER_CREATION_NODE=
for a in $(kubectl get node -o json | jq -r .items[].status.addresses[0].address) ; do 
	create_account `ps auwx | md5sum | awk '{print $1}'` `ps auwx | sha1sum | awk '{print $1}'` $a
	if [ -z "${USER_CREATION_NODE}" ]; then
		USER_CREATION_NODE=$a
	fi
done
# genesis user
create_account `ps auwx | md5sum | awk '{print $1}'` `ps auwx | sha1sum | awk '{print $1}'` $USER_CREATION_NODE 1
