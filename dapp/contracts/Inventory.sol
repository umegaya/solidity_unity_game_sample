pragma solidity ^0.4.24;

import "./libs/StorageAccessor.sol";
import "./libs/Restrictable.sol";
import "./libs/pb/Card_pb.sol";
import "./libs/pb/Payment_pb.sol";
import "./libs/PRNG.sol";
import "./libs/math/Math.sol";
import "./CalcUtil.sol";
import "./Constants.sol";
import "./Cards.sol";

//TODO: make it ERC721 compatible
contract Inventory is StorageAccessor, Restrictable {
  //defs
  using PRNG for PRNG.Data;
  using pb_ch_Card for pb_ch_Card.Data;
  using pb_ch_Payment for pb_ch_Payment.Data;


  //variables
  uint idSeed_;
  mapping(uint => uint) prices_;
  Cards cards_;


  //events
  event MintCard(address indexed user, uint id, bytes created);
  event UpdateCard(address indexed user, uint id, bytes created);
  event Merge(address indexed user, uint remain_card_id, uint merged_card_id, bytes created);
  event ConsumeTx(address indexed user, string tx_id);


  //ctor
  constructor(address storageAddress, address cardsAddress)  
    StorageAccessor(storageAddress) 
    Restrictable() public {
    cards_ = Cards(cardsAddress);
    idSeed_ = 1;
  } 


  //reader
  function getSlotSize(address user) public view returns (uint) {
    return cards_.balanceOf(user);
  }
  //until solidity 0.4.21, return bytes directly and parse on client side
  function getSlotBytes(address user, uint slot_idx) public view returns (bytes) {
    uint id = cards_.tokenOfOwnerByIndex(user, slot_idx);
    return loadBytes(id);
  }
  function getSlotBytesAndId(address user, uint slot_idx) public view returns (uint, bytes) {
    uint id = cards_.tokenOfOwnerByIndex(user, slot_idx);
    return (id, loadBytes(id));
  }
  function getSlotId(address user, uint slot_idx) public view returns (uint) {
    return cards_.tokenOfOwnerByIndex(user, slot_idx);
  }
  function getSlotBytesById(uint card_id) public view returns (bytes) {
    return loadBytes(card_id);
  }
  function getPrice(uint id) public view returns (uint) {
    return prices_[id];
  }
  function getCard(uint id) internal view returns (pb_ch_Card.Data cat, bool found) {
      bytes memory c = loadBytes(id);
      if (c.length > 0) {
        cat = pb_ch_Card.decode(c);
        found = true;
      } else {
        found = false;
      }
  }
  function canReleaseCard(address user) public view returns (bool) {
    //cannot be the 'no card' status
    return cards_.balanceOf(user) > 1;
  }
  function estimateResultValue(uint source_card_id,
    uint target_card_id, int debug_rate) public view returns (uint) {
    pb_ch_Card.Data memory new_card = createMergedCard(source_card_id, target_card_id, debug_rate);
    return CalcUtil.evaluate(new_card);
  }
  function createMergedCard(
    uint a_card_id, uint b_card_id, int rate) internal view returns (pb_ch_Card.Data card) {
    bool tmp;
    pb_ch_Card.Data memory ca;
    (ca, tmp) = getCard(a_card_id);
    require(tmp);

    pb_ch_Card.Data memory cb;
    (cb, tmp) = getCard(b_card_id);
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
    //return all tokens to admin
    for (uint i = 0; i < cards_.balanceOf(user); i++) {
      uint id = cards_.tokenOfOwnerByIndex(user, i);
      prices_[id] = 0;
      cards_.privilegedTransfer(user, administrator_, id);
    }
  }
  function setForSale(address user, uint id, uint price) public writer {
    require(cards_.ownerOf(id) == user);
    prices_[id] = price;
  }
  //record payment, if fails, related transation rolled back.
  function recordPayment(address user, string tx_id) public returns (bool) {
    uint tx_id_hash = uint(keccak256(abi.encodePacked("payment", tx_id)));
    bytes memory bs = loadBytes(tx_id_hash);
    if (bs.length <= 0) {
      pb_ch_Payment.Data memory p;
      p.payer = user;
      saveBytes(tx_id_hash, p.encode());
      emit ConsumeTx(user, tx_id);
      return true;
    } else {
      return false;
    }
  }
  function transferCard(address from, address to, uint card_id) public writer {
    require(prices_[card_id] > 0);
    require(canReleaseCard(from));
    cards_.privilegedTransfer(from, to, card_id);
  }
  function returnCard(address from, uint card_id) public writer {
    require(canReleaseCard(from));
    cards_.privilegedTransfer(from, administrator_, card_id);
  }
  function merge(address user, 
    uint source_card_id, //this card remains
    uint target_card_id, //merged card, burned after merge success
    int debug_rate) public writer returns (bytes) {
    //verify
    require(cards_.ownerOf(source_card_id) == user);
    require(cards_.ownerOf(target_card_id) == user);
    //create merged card data
    pb_ch_Card.Data memory card = createMergedCard(source_card_id, target_card_id, debug_rate);
    //update card dat
    bytes memory bs = card.encode();
    saveBytes(source_card_id, bs);
    //issue events
    emit Merge(user, source_card_id, target_card_id, bs);
    //burn merged card
    cards_.merged(user, target_card_id);
    return bs;
  }
  function mintCard(address user) public writer returns (uint) {
    PRNG.Data memory rnd;
    uint n_skills = rnd.gen2(1, 3);
    uint16[] memory skills = new uint16[](n_skills);
    for (uint i = 0; i < n_skills; i++) {
      skills[i] = uint16(rnd.gen2(1, 16));
    }
    return mintFixedCard(user, 
                uint16(rnd.gen2(50, 100)), 
                uint16(rnd.gen2(10, 30)), uint16(rnd.gen2(10, 30)),
                skills);
  }
  function mintFixedCard(address user, 
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
    return mintFixedCard(user, c); //*/
  }
  function mintFixedCard(address user, pb_ch_Card.Data card) internal writer returns (uint) {
    uint id = idSeed_++;
    bytes memory bs = card.encode();
    saveBytes(id, bs);

    emit MintCard(user, id, bs);
    cards_.mint(user, id);
    return id;  
  }
}
