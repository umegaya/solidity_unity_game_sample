abi:
	@make -C ../dapp compile_if_not
	@docker run --rm \
		-v `pwd`/../dapp/build/contracts:/in -v `pwd`/Assets/Resources/Contracts:/out \
		realguess/jq sh -c "cat /in/World.json | jq '.abi' > /out/World.json"
