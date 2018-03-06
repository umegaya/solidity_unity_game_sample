pragma solidity ^0.4.17;

import "./Restrictable.sol";

library StorageHelper {
  function toBytes(byte[256] b, uint len) internal pure returns (bytes) {
    bytes memory bs = new bytes(len);
    for (uint i = 0; i < len; i++) {
      bs[i] = b[i];
    }
    return bs;
  }
}

contract Storage is Restrictable {
    uint public constant CHUNK_SIZE = 256;
    mapping (uint => bytes) chunks_;

    function Storage() Restrictable() public {
    }

    function getBytes(uint id) public view returns (bytes) {
        return chunks_[id];
    }
    function getBytesRange(uint id, uint offset) public view returns (byte[256], uint) {
        bytes memory bs = chunks_[id];
        byte[256] memory rbs;
        uint remain = bs.length - offset;
        if (remain < 0) {
            return (rbs, 0);
        } else if (remain > 256) {
            remain = offset + 256;
        } else {
            remain = bs.length;
        }
        for (uint i = offset; i < remain; i++) {
            rbs[i - offset] = bs[i];
        }
        return (rbs, remain);
    }
    function setBytes(uint id, bytes data) public writer returns (bool) {
        require(data.length <= CHUNK_SIZE);
        bytes memory prev = chunks_[id];
        chunks_[id] = data;
        assert(chunks_[id].length > 0);
        return prev.length > 0;
    }
}
