#!/bin/bash

set -e

ROOT=$(cd $(dirname $0) && pwd)/..
source ${ROOT}/tools/common.sh ${ROOT}

SECRET_ROOT=${ROOT}/volume/secret/${K8S_PLATFORM}

# ----------------------------------
# create genesis account setting
# ----------------------------------
GENESIS_ACCOUNTS_ADDRESSES=()
for u in $(ls ${SECRET_ROOT}/user-*.addr) ; do
	# this big number means 2 ^ 192 (ether) = 2 ^ 192 * 10 ^ 18 (wei) < 2 ^256
	GENESIS_ACCOUNTS_ADDRESSES+=("\"$(cat $u)\": { \"balance\": \"0x1000000000000000000000000000000000000000000000000\" }")
done
GENESIS_ACCOUNTS="$(IFS=,; echo "${GENESIS_ACCOUNTS_ADDRESSES[*]}")"

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

# ----------------------------------
# dump results
# ----------------------------------
echo "GENESIS_ACCOUNTS:${GENESIS_ACCOUNTS[@]}"
echo "VALIDATORS:${VALIDATORS[@]}"
cat ${NODE_PWDS}

# ----------------------------------
# create chain json
# ----------------------------------
cat ${ROOT}/volume/config/chain1.json.tmpl \
	| sed -e "s/__VALIDATORS__/${VALIDATORS}/" \
	| sed -e "s/__GENESIS_ACCOUNTS__/${GENESIS_ACCOUNTS}/" \
> ${ROOT}/volume/config/${K8S_PLATFORM}.json
