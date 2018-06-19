PROJECT_ID=
REGION=us-central1
IMAGE=umegaya/gcfdev
CREDVOLUME=gcloud-config-for-nch
BIN=./node_modules/.bin
WEBPACK=$(BIN)/webpack
NODEMON=$(BIN)/nodemon
HOSTPORT=5000
FN=

#---------------
# bootstrap(linux/osx)
#---------------
init:
	@echo "please provide your google cloud sdk credential..."
	@-docker rm $(CREDVOLUME) 
	docker run -ti --name $(CREDVOLUME) $(IMAGE) gcloud auth login
	docker run -ti --volumes-from $(CREDVOLUME) $(IMAGE) gcloud config set project $(PROJECT_ID)
	docker run -ti --volumes-from $(CREDVOLUME) $(IMAGE) gcloud config set functions/region $(REGION)

shell:
	docker run --rm -ti --volumes-from $(CREDVOLUME) --link $(DBNAME):dbhost -p 5000:$(HOSTPORT) -v `pwd`:/project -w /project $(IMAGE) bash

#---------------
# build code 
#---------------
dev:
	$(WEBPACK) --config ./tools/webpack/config.js --watch &
	sleep 1
	$(NODEMON) --ext js --watch dist -x 'node index.js'

compile:
	$(WEBPACK) --config ./tools/webpack/config.js

#---------------
# test 
#---------------
test:
	wrk -t4 -c10 -d10s -s bench/entry.lua http://localhost:5000

#---------------
# deployment
#---------------
deploy:
	bash ./tools/deploy/gcp.sh $(FN)
