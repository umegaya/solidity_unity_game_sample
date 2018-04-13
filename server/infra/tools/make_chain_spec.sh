#!/bin/bash

ROOT=$(cd $(dirname $0) && pwd)/..
source ${ROOT}/tools/common.sh ${ROOT}

# ----------------------------------
# create genesis account setting
# ----------------------------------
USER_ADDRESS=`cat ${ROOT}/volume/secret/user.addr`
# this big number means 2 ^ 200
GENESIS_ACCOUNTS="\"${USER_ADDRESS}\": { \"balance\": \"1606938044258990275541962092341162602522202993782792835301376\" }"

# ----------------------------------
# create validators account setting
# ----------------------------------
VALIDATOR_ADDRESSES=()
for n in $(ls ${ROOT}/volume/secret/node-*.addr) ; do
	VALIDATOR_ADDRESSES+=("\"`cat $n`\"")
done
VALIDATORS="$(IFS=,; echo "${VALIDATOR_ADDRESSES[*]}")"

# ----------------------------------
# create passwords file
# ----------------------------------
NODE_PWDS=${ROOT}/volume/secret/node.pwds
rm -f ${NODE_PWDS}
for n in $(ls ${ROOT}/volume/secret/node-*.pass) ; do
	cat $n >> ${NODE_PWDS}
done

echo "GENESIS_ACCOUNTS:${GENESIS_ACCOUNTS}"
echo "VALIDATORS:${VALIDATORS}"
cat ${NODE_PWDS}

# ----------------------------------
# create chain json
# ----------------------------------
cat ${ROOT}/volume/config/chain1.json.tmpl \
	| sed -e "s/__VALIDATORS__/${VALIDATORS}/" \
	| sed -e "s/__GENESIS_ACCOUNTS__/${GENESIS_ACCOUNTS}/" \
>  ${ROOT}/volume/config/chain1.json

