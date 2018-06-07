pragma solidity ^0.4.24;

import "./Restrictable.sol";

contract Storage is Restrictable {
    mapping (uint => bytes) chunks_;

    constructor() Restrictable() public {
    }

    function getBytes(uint id) public reader view returns (bytes) {
        return chunks_[id];
    }

    function setBytes(uint id, bytes data) public writer returns (bool) {
        bytes memory prev = chunks_[id];
        chunks_[id] = data;
        assert(chunks_[id].length > 0);
        return prev.length > 0;
    }
}
