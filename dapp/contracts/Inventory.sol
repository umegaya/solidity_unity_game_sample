pragma solidity ^0.4.17;

import "./libs/StorageAccessor.sol";
import "./libs/Restrictable.sol";
import "./libs/pb/Cat_pb.sol";
import "./libs/PRNG.sol";
import "./libs/math/Math.sol";
import "./NekoUtil.sol";

contract Inventory is StorageAccessor, Restrictable {
  //defs
  struct Slot {
    uint id;
    uint price; //0 means not for sale
  }
  using PRNG for PRNG.Data;
  using pb_neko_Cat for pb_neko_Cat.Data;


  //variables
  uint idSeed_;
  mapping(address => Slot[]) inventories_; 


  //events
  event AddCat(address user, uint id, bytes created);


  //ctor
  function Inventory(address storageAddress) StorageAccessor(storageAddress) Restrictable() public {
    idSeed_ = 1;
  } 


  //reader
  function getSlotSize(address user) public view returns (uint) {
    return inventories_[user].length;
  }
  //until solidity 0.4.21, return bytes directly and parse on client side
  function getSlotBytes(address user, uint slot_idx) public view returns (byte[256], uint) {
    var iv = inventories_[user];
    assert(iv.length > slot_idx);
    return loadBytes(iv[slot_idx].id);
  }
  function getSlotId(address user, uint slot_idx) public view returns (uint) {
    var iv = inventories_[user];
    assert(iv.length > slot_idx);
    return iv[slot_idx].id;
  }
  function getPrice(address user, uint id) public view returns (uint) {
    var iv = inventories_[user];
    for (uint i = 0; i < iv.length; i++) {
      if (id == iv[i].id) {
        return iv[i].price;
      }
    }
    return 0;
  }
  function getCat(address user, uint id) internal view returns (pb_neko_Cat.Data cat, bool found) {
    var iv = inventories_[user];
    for (uint i = 0; i < iv.length; i++) {
      if (iv[i].id == id) {
        var (c, clen) = loadBytes(id);
        cat = pb_neko_Cat.decode(StorageHelper.toBytes(c, clen));
        found = true;
      }
    }
    found = false;
  }
  function estimateBreedFee(address breeder, uint breeder_cat_id,
    address breedee, uint breedee_cat_id) public view returns (uint) {
    var kitty = createKitty(breeder, breeder_cat_id, breedee, breedee_cat_id);
    return NekoUtil.evaluateCat(kitty);               
  }
  function createKitty(
    address a, uint a_cat_id, 
    address b, uint b_cat_id) internal returns (pb_neko_Cat.Data kitty) {
    var (ca, found_a) = getCat(a, a_cat_id);
    require(found_a);

    var (cb, found_b) = getCat(b, b_cat_id);
    require(found_b);

    uint rate = Math.max256(a_cat_id, b_cat_id) % 16 - Math.min256(a_cat_id, b_cat_id) % 16;

    PRNG.Data memory rnd;
    kitty.hp = uint16(NekoUtil.mixParam(rnd, ca.hp, cb.hp, rate, 10));
    kitty.attack = uint16(NekoUtil.mixParam(rnd, ca.attack, cb.attack, rate, 3));
    kitty.defense = uint16(NekoUtil.mixParam(rnd, ca.defense, cb.defense, rate, 3));
    var a_skill_inherit = rnd.gen2(0, ca.skills.length);
    var total_skill = a_skill_inherit + rnd.gen2(0, cb.skills.length);
    kitty.skills = new pb_neko_Cat_Skill.Data[](total_skill);
    for (uint i = 0; i < a_skill_inherit; i++) {
      kitty.skills[i] = ca.skills[i];
    }
    for (; i < total_skill; i++) {
      kitty.skills[i] = ca.skills[i - a_skill_inherit];
    }
  }


  //writer
  function setForSale(address user, uint index, uint price) public writer returns (bool) {
    var iv = inventories_[user];
    require(iv.length > index);
    iv[index].price = price;
    return true;
  }
  function transferCat(address from, address to, uint cat_id) public writer returns (bool) {
    var iv = inventories_[from];
    bool found = false;
    for (uint i = 0; i < iv.length; i++) {
      if (!found) {
        if (cat_id == iv[i].id) {
          found = true;
          Slot memory s;
          s.id = cat_id;
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
  function breed(string name, 
    address breeder, uint breeder_cat_id,
    address breedee, uint breedee_cat_id) public writer returns (bool) {
    var kitty = createKitty(breeder, breeder_cat_id, breedee, breedee_cat_id);
    kitty.name = name;
    addFixedCat(breeder, kitty);
    return true;
  }
  function addCat(address user, string name) public writer returns (bool) {
    require(bytes(name).length <= 32);
    PRNG.Data memory rnd;
    var n_skills = rnd.gen2(1, 3);
    var skills = new uint16[](n_skills);
    for (uint i = 0; i < n_skills; i++) {
      skills[i] = uint16(rnd.gen2(1, 16));
    }
    return addFixedCat(user, name, 
                uint16(rnd.gen2(50, 100)), 
                uint16(rnd.gen2(10, 30)), uint16(rnd.gen2(10, 30)),
                skills);
  }
  function addFixedCat(address user, string name, 
                      uint16 hp, uint16 atk, uint16 def, 
                      uint16[] skills) public writer returns (bool) {
    require(bytes(name).length <= 32);
    pb_neko_Cat.Data memory c;
    c.hp = hp;
    c.attack = atk;
    c.defense = def;
    c.exp = 0;
    c.skills = new pb_neko_Cat_Skill.Data[](skills.length);
    for (uint i = 0; i < skills.length; i++) {
      c.skills[i].id = skills[i];
      c.skills[i].exp = 0;
    }
    c.name = name;
    return addFixedCat(user, c); 
  }
  function addFixedCat(address user, pb_neko_Cat.Data cat) internal writer returns (bool) {
    var id = idSeed_++;
    var bs = cat.encode();
    saveBytes(id, bs);

    Slot memory s;
    s.id = id;
    inventories_[user].push(s);  

    AddCat(user, id, bs);
    return true;  
  }
}
