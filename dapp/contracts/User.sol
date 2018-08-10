pragma solidity ^0.4.24;

import './libs/Restrictable.sol';
import './libs/StorageAccessor.sol';
import "./libs/pb/User_pb.sol";

//this contract cannot be upgradable easily, so please do not add complex features in this contract.
contract User is Restrictable, StorageAccessor {
  using pb_ch_User for pb_ch_User.Data;

  constructor(address storageAddress) Restrictable() StorageAccessor(storageAddress) public {

  }
}
