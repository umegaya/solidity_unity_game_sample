#!/bin/bash

CWD=$(cd dirname $0 && pwd)

HOST=${1:-localhost}
DB=${2:-data}
SCHEMA=${3:-${CWD}/database/mysql.sql}
MYSQL_ROOT_PASSWORD=${4:-namaham149}

docker kill nkdb
docker rm nkdb

set -e
kcd create -f ${CWD}/k8s/db.yaml
set +e

echo "wait for mysql server running"
while :
do
	mysql -u root -p${MYSQL_ROOT_PASSWORD} -h ${HOST} -e "show databases" > /dev/null 2>/dev/null
	if [ $? -eq 0 ]; then
		echo "mysql server starts"
		break
	fi
	printf "."
	sleep 1
done

set -e
mysql -u root -p${MYSQL_ROOT_PASSWORD} -h ${HOST} -e "create database if not exists ${DB}"
mysql -u root -p${MYSQL_ROOT_PASSWORD} -h ${HOST} -e "set global max_connections = 65535"
mysql -u root -p${MYSQL_ROOT_PASSWORD} -h ${HOST} ${DB} < ${PWD}/${SCHEMA}

