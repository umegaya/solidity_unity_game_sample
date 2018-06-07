pragma solidity ^0.4.24;

import "./libs/StorageAccessor.sol";
import "./libs/Restrictable.sol";
import "./libs/pb/Payment_pb.sol";

contract Payment is StorageAccessor, Restrictable {
    using pb_neko_Payment for pb_neko_Payment.Data;
    function findOrUpdatePayment(address user, string tx_id) public view returns (bool, bytes) {
        uint tx_id_hash = uint(keccak256(abi.encodePacked("payment", tx_id)));
        bytes memory bs = loadBytes(tx_id_hash);
        if (bs.length <= 0) {
            pb_neko_Payment.Data memory p;
            p.payer = user;
            saveBytes(tx_id_hash, p.encode());
            return (false, bs);
        } else {
            return (true, bs);
        }
    }
}
