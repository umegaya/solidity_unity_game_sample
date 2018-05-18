FN=

db:
	docker inspect ffdb 2>&1 > /dev/null || bash ./tools/server/rundb.sh 192.168.99.100

run: db
	node index.js

dev:
	webpack --config ./tools/webpack/config.js --watch &
	node_modules/.bin/nodemon --ext js --watch dist -x 'node index.js'

compile:
	webpack --config ./tools/webpack/config.js

stop:
	-docker kill ffdb
	-docker rm ffdb
	-killall node

test:
	wrk -t4 -c10 -d10s -s bench/entry.lua http://localhost:5000
