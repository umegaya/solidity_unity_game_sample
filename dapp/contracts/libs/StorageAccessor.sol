pragma solidity ^0.4.17;

import "./Storage.sol";

contract StorageAccessor {
    Storage public storageContract_; //Storage contract

    constructor(address storageAddress) public {
      storageContract_ = Storage(storageAddress);
    }

    function saveBytes(uint id, bytes memory b) internal {
      require(storageContract_.checkWritableFrom(this));
      storageContract_.setBytes(id, b);
    }

    function loadBytes(uint id) public view returns (bytes) {
      return storageContract_.getBytes(id);
    }
}
