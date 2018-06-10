pragma solidity ^0.4.24;

import "./libs/StorageAccessor.sol";
import "./libs/Restrictable.sol";
import "./libs/pb/Card_pb.sol";
import "./libs/pb/Payment_pb.sol";
import "./libs/PRNG.sol";
import "./libs/math/Math.sol";
import "./CalcUtil.sol";
import "./Constants.sol";

contract Inventory is StorageAccessor, Restrictable {
  //defs
  struct Slot {
    uint id;
    uint price; //0 means not for sale
  }
  using PRNG for PRNG.Data;
  using pb_ch_Card for pb_ch_Card.Data;
  using pb_ch_Payment for pb_ch_Payment.Data;


  //variables
  uint idSeed_;
  mapping(address => Slot[]) inventories_; 


  //events
  event AddCard(address indexed user, uint id, bytes created);
  event Merge(address indexed user_a, address indexed user_b, uint id_a, uint id_b, uint new_id);


  //ctor
  constructor(address storageAddress) StorageAccessor(storageAddress) Restrictable() public {
    idSeed_ = 1;
  } 


  //reader
  function getSlotSize(address user) public view returns (uint) {
    return inventories_[user].length;
  }
  //until solidity 0.4.21, return bytes directly and parse on client side
  function getSlotBytes(address user, uint slot_idx) public view returns (bytes) {
    Slot[] storage iv = inventories_[user];
    require(iv.length > slot_idx);
    return loadBytes(iv[slot_idx].id);
  }
  function getSlotBytesAndId(address user, uint slot_idx) public view returns (uint, bytes) {
    Slot[] storage iv = inventories_[user];
    require(iv.length > slot_idx);
    bytes memory c = loadBytes(iv[slot_idx].id);    
    return (iv[slot_idx].id, c);
  }
  function getSlotId(address user, uint slot_idx) public view returns (uint) {
    Slot[] storage iv = inventories_[user];
    require(iv.length > slot_idx);
    return iv[slot_idx].id;
  }
  function getPrice(address user, uint id) public view returns (uint) {
    Slot[] storage iv = inventories_[user];
    for (uint i = 0; i < iv.length; i++) {
      if (id == iv[i].id) {
        return iv[i].price;
      }
    }
    return 0;
  }
  function getCard(address user, uint id) internal view returns (pb_ch_Card.Data cat, bool found) {
    Slot[] memory iv = inventories_[user];
    for (uint i = 0; i < iv.length; i++) {
      if (iv[i].id == id) {
        bytes memory c = loadBytes(id);
        cat = pb_ch_Card.decode(c);
        found = true;
        return;
      }
    }
    found = false;
  }
  function canReleaseCard(address user) public view returns (bool) {
    //cannot be the 'no cat' status
    return inventories_[user].length > 1;
  }
  function estimateResultValue(address source, uint source_card_id,
    address target, uint target_card_id, int debug_rate) public view returns (uint) {
    pb_ch_Card.Data memory new_card = createCard(source, source_card_id, target, target_card_id, debug_rate);
    return CalcUtil.evaluate(new_card);
  }
  function createCard(
    address a, uint a_card_id, 
    address b, uint b_card_id, int rate) internal view returns (pb_ch_Card.Data card) {
    bool tmp;
    pb_ch_Card.Data memory ca;
    (ca, tmp) = getCard(a, a_card_id);
    require(tmp);

    pb_ch_Card.Data memory cb;
    (cb, tmp) = getCard(b, b_card_id);
    require(tmp); //*/

    if (rate < 0) {
      tmp = false;
      rate = int(Math.max256(a_card_id % 16, b_card_id % 16) - Math.min256(a_card_id % 16, b_card_id % 16));
    } else {
      tmp = true;
    }//*/

    PRNG.Data memory rnd;
    card.hp = uint16(CalcUtil.mixParam(rnd, ca.hp, cb.hp, rate, tmp ? 0 : 10));
    card.attack = uint16(CalcUtil.mixParam(rnd, ca.attack, cb.attack, rate, tmp ? 0 : 3));
    card.defense = uint16(CalcUtil.mixParam(rnd, ca.defense, cb.defense, rate, tmp ? 0 : 3));
    uint a_skill_inherit = tmp ? 1 : rnd.gen2(0, ca.skills.length);
    uint total_skill = a_skill_inherit + (tmp ? 1 : rnd.gen2(0, cb.skills.length));
    card.skills = new pb_ch_Card_Skill.Data[](total_skill);
    for (uint i = 0; i < a_skill_inherit; i++) {
      card.skills[i] = ca.skills[i];
    }
    for (; i < total_skill; i++) {
      card.skills[i] = ca.skills[i - a_skill_inherit];
    }
  }


  //writer
  function clearSlots(address user) public admin {
    Slot[] storage iv = inventories_[user];
    iv.length = 0;
  }
  function setForSale(address user, uint index, uint price) public writer returns (bool) {
    Slot[] storage iv = inventories_[user];
    require(iv.length > index);
    iv[index].price = price;
    return true;
  }
  //record payment, if fails, related transation rolled back.
  function recordPayment(address user, string tx_id) public returns (bool) {
    uint tx_id_hash = uint(keccak256(abi.encodePacked("payment", tx_id)));
    bytes memory bs = loadBytes(tx_id_hash);
    if (bs.length <= 0) {
      pb_ch_Payment.Data memory p;
      p.payer = user;
      saveBytes(tx_id_hash, p.encode());
      return true;
    } else {
      return false;
    }
  }
  function transferCard(address from, address to, uint card_id) public writer returns (bool) {
    Slot[] storage iv = inventories_[from];
    bool found = false;
    for (uint i = 0; i < iv.length; i++) {
      if (!found) {
        if (card_id == iv[i].id) {
          found = true;
          Slot memory s;
          s.id = card_id;
          inventories_[to].push(s);              
        }
      } else {
        iv[i - 1] = iv[i];
      }
    }
    if (found) {
      iv.length--;
    }
    return found;    
  }
  function merge(
    address source, uint source_card_id,
    address target, uint target_card_id,
    int debug_rate) public writer returns (bool) {
    pb_ch_Card.Data memory card = createCard(source, source_card_id, target, target_card_id, debug_rate);
    uint new_id = addFixedCard(source, card);
    emit Merge(source, target, source_card_id, target_card_id, new_id);
    return true;
  }
  function addCard(address user) public writer returns (uint) {
    PRNG.Data memory rnd;
    uint n_skills = rnd.gen2(1, 3);
    uint16[] memory skills = new uint16[](n_skills);
    for (uint i = 0; i < n_skills; i++) {
      skills[i] = uint16(rnd.gen2(1, 16));
    }
    return addFixedCard(user, 
                uint16(rnd.gen2(50, 100)), 
                uint16(rnd.gen2(10, 30)), uint16(rnd.gen2(10, 30)),
                skills);
  }
  function addFixedCard(address user, 
                      uint16 hp, uint16 atk, uint16 def, 
                      uint16[] skills) public writer returns (uint) {
    pb_ch_Card.Data memory c;
    c.hp = hp;
    c.attack = atk;
    c.defense = def;
    c.exp = 0;
    c.skills = new pb_ch_Card_Skill.Data[](skills.length);
    for (uint i = 0; i < skills.length; i++) {
      c.skills[i].id = skills[i];
      c.skills[i].exp = 0;
    }
    return addFixedCard(user, c); //*/
  }
  function addFixedCard(address user, pb_ch_Card.Data card) internal writer returns (uint) {
    uint id = idSeed_++;
    bytes memory bs = card.encode();
    saveBytes(id, bs);

    Slot memory s;
    s.id = id;
    inventories_[user].push(s);  

    emit AddCard(user, id, bs);
    return id;  
  }
}
