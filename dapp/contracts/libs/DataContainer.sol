pragma solidity ^0.4.24;
pragma experimental ABIEncoderV2;

import "./StorageAccessor.sol";
import "./Restrictable.sol";

contract DataContainer is StorageAccessor, Restrictable {
    //ctor
    constructor(address storageAddress) StorageAccessor(storageAddress) Restrictable() public {
    } 

    //data name => (modification generation => modified ids)
    struct History {
        uint current_gen;
        mapping(uint => uint32[]) updated_by_gen;
        mapping(uint32 => bool) idmaps;
        uint current_total;
        uint32[] all_ids;
    }
    mapping(string => History) updateHistory_;

    //functions 
    function getRecords(string typ, uint[] ids) public view returns (bytes[]) {
        bytes[] memory ret = new bytes[](ids.length);
        for (uint i = 0; i < ids.length; i++) {
            uint hash = uint(keccak256(abi.encodePacked(typ, ids[i])));
            ret[i] = loadBytes(hash);
        }
        return ret;
    }
    function putRecords(string typ, uint[] ids, bytes[] data) public writer {
        require(ids.length == data.length);
        History storage h = updateHistory_[typ];
        h.updated_by_gen[h.current_gen] = new uint32[](ids.length);
        for (uint i = 0; i < ids.length; i++) {
            uint hash = uint(keccak256(abi.encodePacked(typ, ids[i])));
            uint32 id = uint32(ids[i]);
            saveBytes(hash, data[i]);
            h.updated_by_gen[h.current_gen][i] = id;
            if (!h.idmaps[id]) {
                require(h.all_ids.length >= h.current_total);
                if (h.all_ids.length == h.current_total) {
                    uint32[] memory all_id_tmp = h.all_ids;
                    h.all_ids = new uint32[](h.current_total * 2);
                    for (uint j = 0; j < h.current_total; j++) {
                        h.all_ids[j] = all_id_tmp[j];
                    }
                }
                h.all_ids[h.current_total++] = id;
                h.idmaps[id] = true;
            }
        }
    }
    function recordIdDiff(string typ, uint client_generation) public view returns (uint, uint32[][]) {
        History storage h = updateHistory_[typ];
        uint32[][] memory idlists;
        if (client_generation == 0) { //first time.
            //returns all ids
            idlists = new uint32[][](1);
            idlists[0] = h.all_ids;
        } else if (client_generation < h.current_gen) { //otherwise returns update histories
            idlists = new uint32[][](h.current_gen - client_generation);
            for (uint i = client_generation; i < h.current_gen; i++) {
                idlists[i - client_generation] = h.updated_by_gen[i];
            }
        }
        return (h.current_gen, idlists);
    }
}
