pragma solidity ^0.4.17;

import "./Storage.sol";

contract StorageAccessor {
    uint internal constant CHUNK_SIZE = 256; //should match with Storage.CHUNK_SIZE
    Storage public storageContract_; //Storage contract

    function StorageAccessor(address storageAddress) public {
      storageContract_ = Storage(storageAddress);
    }

    function saveBytes(uint id, bytes memory b) internal {
      //sender of setBytes will be this.
      require(storageContract_.checkWritableFrom(this));
      storageContract_.setBytes(id, b);
    }

    function loadBytes(uint id) public view returns (byte[256], uint) {
      return storageContract_.getBytesRange(id, 0);
    }
}
