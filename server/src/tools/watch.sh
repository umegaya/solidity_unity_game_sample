# watch script for parcel
if [ -z "$1" ]; then
	# watch all
	(
		trap "kill 0" EXIT
		for dir in `ls ./functions/` ; do
			if [ -d "./functions/$dir" ]; then
				parcel ./functions/$dir/index.ts -d dist/$dir &
			fi
		done
		wait
	)
else
	parcel ./functions/$1/index.ts -d dist/$1 --log-level 3
fi
