test:
	@echo "tests!!"

infra:
	@echo "make sure that you set correct K8S_PLATFORM(${K8S_PLATFORM}) to create infra."
	make -C server/infra init plan exec
	sleep 5
	make -C server/infra taint plan exec
	make -C server/infra chain_spec plan exec
	sleep 5
	make -C server/infra taint plan exec
