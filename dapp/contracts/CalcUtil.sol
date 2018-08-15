pragma solidity ^0.4.24;

import "./libs/PRNG.sol";
import "./libs/StorageAccessor.sol";
import "./libs/math/Math.sol";
import "./libs/pb/Card_pb.sol";
import "./libs/pb/CardSpec_pb.sol";

library CalcUtil {
  using PRNG for PRNG.Data;
  using pb_ch_CardSpec for pb_ch_CardSpec.Data;

  function evaluate(pb_ch_Card.Data c, StorageAccessor sa) internal view returns (uint price) {
    int rarity = Rarity(c, sa);
    uint one = 1;
    price = (one << c.stack) * (1 + NumberOfSetBits(c.insert_flags)) * (1 << uint(rarity)) * 100;
  }

  function Rarity(pb_ch_Card.Data c, StorageAccessor sa) internal view returns (int) {
    (pb_ch_CardSpec.Data memory cs, bool found) = getCardSpec(sa, c.spec_id);
    return found ? cs.rarity : 0;
  }

  function NumberOfSetBits(uint i) internal pure returns (uint) {
    /*i = i - ((i >> 1) & 0x55555555);
    i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
    return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;*/
    require(i <= 7);
    if (i == 1 || i == 2 || i == 4) { return 1; }
    else if (i == 5 || i == 6 || i == 3) { return 2; }
    else if (i == 7) { return 3; }
    else { return 0; }
  }

  function RandomInsertFlag() public view returns (uint32) {
    PRNG.Data memory rnd;
    return uint32(rnd.gen2(0, 7));
  }

  //internal helper
  function getCardSpec(StorageAccessor sa, uint id) internal view returns (pb_ch_CardSpec.Data cs, bool found) {
    uint hash = uint(keccak256(abi.encodePacked("CardSpec", id)));
    bytes memory bs = sa.loadBytes(hash);
    if (bs.length > 0) {
      cs = pb_ch_CardSpec.decode(bs);
      found = true;
    } else {
      found = false;
    }
  }

  function getCard(StorageAccessor sa, uint id) internal view returns (pb_ch_Card.Data cat, bool found) {
    bytes memory c = sa.loadBytes(id);
    if (c.length > 0) {
      cat = pb_ch_Card.decode(c);
      found = true;
    } else {
      found = false;
    }
  }

}
