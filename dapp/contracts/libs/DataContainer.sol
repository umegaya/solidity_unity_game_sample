pragma solidity ^0.4.24;
pragma experimental ABIEncoderV2;

import "./StorageAccessor.sol";
import "./Restrictable.sol";

contract DataContainer is StorageAccessor, Restrictable {
    //const
    uint public constant INITIAL_IDLIST_LENGTH = 256; 

    //event
    event Payload(uint index, uint idlen, uint datalen);

    //ctor
    constructor(address storageAddress) StorageAccessor(storageAddress) Restrictable() public {
    } 

    //data name => (modification generation => modified ids)
    //TODO: should be protobuf, bytes cannot be a key of map in protobuf,
    //so idmaps regenerate after other data recover from storage
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
        require (ids.length == data.length);
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
                h.idmaps[id] = true;//*/
            }//*/
        }//*/
        h.current_gen++;
    }
    function recordIdDiff(string typ, uint client_generation) public view returns (uint, bytes[][]) {
        History storage h = updateHistory_[typ];
        bytes[][] memory idlists;
        uint i;
        if (client_generation == 0) { //first time.
            //returns all ids
            idlists = new bytes[][](1);
            idlists[0] = new bytes[](h.current_total);
            for (i = 0; i < h.current_total; i++) {
                idlists[0][i] = h.all_ids[i];
            }
        } else if (client_generation < h.current_gen) { //otherwise returns update histories
            idlists = new bytes[][](h.current_gen - client_generation);
            for (i = client_generation; i < h.current_gen; i++) {
                idlists[i - client_generation] = h.updated_by_gen[i];
            }
        }
        return (h.current_gen, idlists);
    }
}
