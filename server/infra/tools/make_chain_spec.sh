#!/bin/bash

ROOT=$(cd $(dirname $0) && pwd)/..
source ${ROOT}/tools/common.sh ${ROOT}

SECRET_ROOT=${ROOT}/volume/secret/${K8S_PLATFORM}

# ----------------------------------
# create genesis account setting
# ----------------------------------
USER_ADDRESS=`cat ${SECRET_ROOT}/user.addr`
# this big number means 2 ^ 200
GENESIS_ACCOUNTS="\"${USER_ADDRESS}\": { \"balance\": \"1606938044258990275541962092341162602522202993782792835301376\" }"

# ----------------------------------
# create validators account setting
# ----------------------------------
VALIDATOR_ADDRESSES=()
for n in $(ls ${SECRET_ROOT}/node-*.addr) ; do
	VALIDATOR_ADDRESSES+=("\"`cat $n`\"")
done
VALIDATORS="$(IFS=,; echo "${VALIDATOR_ADDRESSES[*]}")"

# ----------------------------------
# create passwords file
# ----------------------------------
NODE_PWDS=${SECRET_ROOT}/node.pwds
rm -f ${NODE_PWDS}
for n in $(ls ${SECRET_ROOT}/node-*.pass) ; do
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
> ${ROOT}/volume/config/${K8S_PLATFORM}.json
