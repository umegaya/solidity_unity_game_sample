pragma solidity ^0.4.24;
pragma experimental ABIEncoderV2;

contract Test {
  function foo(bytes[] bs) public pure returns (uint) {
      return bs.length;
  }
}
