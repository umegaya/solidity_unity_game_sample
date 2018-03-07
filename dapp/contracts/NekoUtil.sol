pragma solidity ^0.4.17;

import "./libs/PRNG.sol";
import "./libs/math/Math.sol";
import "./libs/pb/Cat_pb.sol";

library NekoUtil {
  using PRNG for PRNG.Data;

  function mixParam(PRNG.Data memory rnd, 
    uint p1, uint p2, int blend_rate, uint bonus) public view returns (uint) {
    var max_param = uint(Math.max256(p1, p2));
    var min_param = uint(Math.min256(p1, p2));
    return ((max_param * uint(16 - blend_rate) + min_param * uint(blend_rate)) / 16) + rnd.gen2(0, bonus);
  }

  function evaluateCat(pb_neko_Cat.Data c) internal view returns (uint price) {
    //estimate max price is 30000. we want this worth 0.3 eth (3 * (10 ^ 17) wei for initial rate)
    price = ((c.hp + c.attack + c.defense) * 100 + c.exp);
    for (uint i = 0; i < c.skills.length; i++) {
      price += c.exp;
    }
  }
}
