run:
	node index.js

dev:
	webpack --config ./tools/webpack/config.js --watch &
	node_modules/.bin/nodemon --ext js --watch dist -x 'node index.js'

compile:
	webpack --config ./tools/webpack/config.js

test:
	wrk -t4 -c10 -d10s -s bench/entry.lua http://localhost:5000
