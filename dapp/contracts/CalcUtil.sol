pragma solidity ^0.4.24;

import "./libs/PRNG.sol";
import "./libs/math/Math.sol";
import "./libs/pb/Card_pb.sol";

library CalcUtil {
  using PRNG for PRNG.Data;

  function evaluate(pb_ch_Card.Data c) internal pure returns (uint price) {
    uint rarity = Rarity(c);
    uint one = 1;
    price = (one << c.level) * NumberOfSetBits(c.visual_flags) * (1 << rarity) * 100;
  }

  function Rarity(pb_ch_Card.Data c) internal pure returns (uint) {
    return uint(c.bs[0]);
  }

  function NumberOfSetBits(uint i) internal pure returns (uint) {
    i = i - ((i >> 1) & 0x55555555);
    i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
    return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
  }

  function RandomVisualFlag() public view returns (uint32) {
    PRNG.Data memory rnd;
    return uint32(rnd.gen2(0, 7));
  }
}
