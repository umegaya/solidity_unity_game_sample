pragma solidity ^0.4.17;

import "./libs/StorageAccessor.sol";
import "./libs/Restrictable.sol";
import "./libs/pb/Cat_pb.sol";
import "./libs/PRNG.sol";
import "./libs/math/Math.sol";
import "./NekoUtil.sol";
import "./Constants.sol";

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
  event AddCat(address indexed user, uint id, bytes created);
  event Breed(address indexed user_a, address indexed user_b, uint id_a, uint id_b, uint new_id);


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
    require(iv.length > slot_idx);
    return loadBytes(iv[slot_idx].id);
  }
  function getSlotBytesAndId(address user, uint slot_idx) public view returns (uint, byte[256], uint) {
    var iv = inventories_[user];
    require(iv.length > slot_idx);
    var (c, clen) = loadBytes(iv[slot_idx].id);    
    return (iv[slot_idx].id, c, clen);
  }
  function getSlotId(address user, uint slot_idx) public view returns (uint) {
    var iv = inventories_[user];
    require(iv.length > slot_idx);
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
        return;
      }
    }
    found = false;
  }
  function canReleaseCat(address user) public view returns (bool) {
    //cannot be the 'no cat' status
    return inventories_[user].length > 1;
  }
  function estimateBreedValue(address breeder, uint breeder_cat_id,
    address breedee, uint breedee_cat_id, int debug_rate) public view returns (uint) {
    var kitty = createKitty(breeder, breeder_cat_id, breedee, breedee_cat_id, debug_rate);
    return NekoUtil.evaluateCat(kitty);
  }
  function createKitty(
    address a, uint a_cat_id, 
    address b, uint b_cat_id, int rate) internal view returns (pb_neko_Cat.Data kitty) {
    bool tmp;
    pb_neko_Cat.Data memory ca;
    (ca, tmp) = getCat(a, a_cat_id);
    require(tmp);

    pb_neko_Cat.Data memory cb;
    (cb, tmp) = getCat(b, b_cat_id);
    require(tmp); //*/

    require(ca.is_male != cb.is_male);

    if (rate < 0) {
      tmp = false;
      rate = int(Math.max256(a_cat_id % 16, b_cat_id % 16) - Math.min256(a_cat_id % 16, b_cat_id % 16));
    } else {
      tmp = true;
    }//*/

    PRNG.Data memory rnd;
    kitty.hp = uint16(NekoUtil.mixParam(rnd, ca.hp, cb.hp, rate, tmp ? 0 : 10));
    kitty.attack = uint16(NekoUtil.mixParam(rnd, ca.attack, cb.attack, rate, tmp ? 0 : 3));
    kitty.defense = uint16(NekoUtil.mixParam(rnd, ca.defense, cb.defense, rate, tmp ? 0 : 3));
    var a_skill_inherit = tmp ? 1 : rnd.gen2(0, ca.skills.length);
    var total_skill = a_skill_inherit + (tmp ? 1 : rnd.gen2(0, cb.skills.length));
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
    address breedee, uint breedee_cat_id,
    int debug_rate) public writer returns (bool) {
    var kitty = createKitty(breeder, breeder_cat_id, breedee, breedee_cat_id, debug_rate);
    kitty.name = name;
    var new_id = addFixedCat(breeder, kitty);
    Breed(breeder, breedee, breeder_cat_id, breedee_cat_id, new_id);
    return true;
  }
  function addCat(address user, string name) public writer returns (uint) {
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
                skills, rnd.gen2(0, 1) == 0);
  }
  function addFixedCat(address user, string name, 
                      uint16 hp, uint16 atk, uint16 def, 
                      uint16[] skills, bool is_male) public writer returns (uint) {
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
    c.is_male = is_male;
    return addFixedCat(user, c); //*/
  }
  function addFixedCat(address user, pb_neko_Cat.Data cat) internal writer returns (uint) {
    var id = idSeed_++;
    var bs = cat.encode();
    saveBytes(id, bs);

    Slot memory s;
    s.id = id;
    inventories_[user].push(s);  

    AddCat(user, id, bs);
    return id;  
  }
}
