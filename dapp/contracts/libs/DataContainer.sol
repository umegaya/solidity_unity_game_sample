pragma solidity ^0.4.24;
pragma experimental ABIEncoderV2;

import "./StorageAccessor.sol";
import "./Restrictable.sol";

contract DataContainer is StorageAccessor, Restrictable {
    //const
    uint public constant INITIAL_IDLIST_LENGTH = 256; 

    //event
    event Error(string func, int code, int arg1, int arg2);

    //ctor
    constructor(address storageAddress) StorageAccessor(storageAddress) Restrictable() public {
    } 

    //data name => (modification generation => modified ids)
    struct History {
        uint current_gen;
        mapping(uint => bytes[]) updated_by_gen;
        mapping(bytes => bool) idmaps;
        uint current_total;
        bytes[] all_ids;
    }
    mapping(string => History) updateHistory_;

    //functions 
    function getRecords(string typ, bytes[] ids) public view returns (bytes[]) {
        bytes[] memory ret = new bytes[](ids.length);
        for (uint i = 0; i < ids.length; i++) {
            uint hash = uint(keccak256(abi.encodePacked(typ, ids[i])));
            ret[i] = loadBytes(hash);
        }
        return ret;
    }
    function putRecords(string typ, bytes[] ids, bytes[] data) public writer {
        if (ids.length != data.length) {
            emit Error(typ, 2, 0, 0);
            return;
        }
        emit Error(typ, 999, 0, 0);
        History storage h = updateHistory_[typ];
        h.updated_by_gen[h.current_gen] = new bytes[](ids.length);
        for (uint i = 0; i < ids.length; i++) {
            bytes memory id = ids[i];
            require(id.length > 0 && data[i].length > 0);
            uint hash = uint(keccak256(abi.encodePacked(typ, id)));
            saveBytes(hash, data[i]);
            h.updated_by_gen[h.current_gen][i] = id;
            if (!h.idmaps[id]) {
                if (h.current_total == 0) {
                    h.all_ids = new bytes[](INITIAL_IDLIST_LENGTH);
                }
                require(h.all_ids.length >= h.current_total);
                if (h.all_ids.length == h.current_total) {
                    bytes[] memory all_id_tmp = h.all_ids;
                    h.all_ids = new bytes[](h.current_total * 2);
                    for (uint j = 0; j < h.current_total; j++) {
                        h.all_ids[j] = all_id_tmp[j];
                    }
                }
                h.all_ids[h.current_total++] = id;
                h.idmaps[id] = true;
            }
        }//*/
        h.current_gen++;
    }
    function recordIdDiff(string typ, uint client_generation) public view returns (uint, bytes[][]) {
        History storage h = updateHistory_[typ];
        bytes[][] memory idlists;
        if (client_generation == 0) { //first time.
            //returns all ids
            idlists = new bytes[][](1);
            idlists[0] = h.all_ids;
        } else if (client_generation < h.current_gen) { //otherwise returns update histories
            idlists = new bytes[][](h.current_gen - client_generation);
            for (uint i = client_generation; i < h.current_gen; i++) {
                idlists[i - client_generation] = h.updated_by_gen[i];
            }
        }
        return (h.current_gen, idlists);
    }

    struct Hoge {
        uint a;
        uint b;
    }
    function getHoges() public pure returns (Hoge[]) {
        Hoge[] memory hs = new Hoge[](3);
        return hs;
    }

    function countFuga(bytes[] hugas) public pure returns (uint) {
        return 1;
    }
}
