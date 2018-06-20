pragma solidity ^0.4.24;

import "./libs/StorageAccessor.sol";
import "./libs/Restrictable.sol";
import "./libs/pb/Match_pb.sol";

contract History is StorageAccessor, Restrictable {
    using pb_ch_Match for pb_ch_Match.Data;
    function recordMatch(address user, uint deck_id) public view returns (uint match_id) {
        return 0;
    }
}
