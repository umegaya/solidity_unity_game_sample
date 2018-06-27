pragma solidity ^0.4.24;

import "./libs/StorageAccessor.sol";
import "./libs/Restrictable.sol";
import "./libs/pb/Match_pb.sol";

contract History is StorageAccessor, Restrictable {
    using pb_ch_Match for pb_ch_Match.Data;
    function recordMatch(address user, uint deck_id) public pure returns (uint match_id) {
/*
uint tx_id_hash = uint(keccak256(abi.encodePacked("payment", tx_id)));
bytes memory bs = loadBytes(tx_id_hash);
*/
        return 0;
    }
}
