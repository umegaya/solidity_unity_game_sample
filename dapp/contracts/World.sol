pragma solidity ^0.4.17;

import "./Inventory.sol";
import "./Moritapo.sol";
import "./Constants.sol";
import "./libs/Restrictable.sol";
import "./libs/pb/Cat_pb.sol";
import "./libs/PRNG.sol";
import "./NekoUtil.sol";


contract World is Restrictable, Constants {
  //defs
  using PRNG for PRNG.Data;


  //variables
  Moritapo token_;
  Inventory inventory_;
  uint tokenSold_;


  //event
  event Exchange(uint value, uint rate, uint tokenSold, uint result);
  event AddCat(address indexed user, uint id, bytes created); //from Inventory.sol
  event Approval(address indexed owner, address indexed spender, uint256 value); //from ERC20.sol
  event Transfer(address indexed from, address indexed to, uint256 value); //from ERC20.sol
  event Breed(address indexed user_a, address indexed user_b, uint id_a, uint id_b, uint new_id);

  event Error(address sender, uint require, uint allowance);

  //ctor
  function World(address tokenAddress, address inventoryAddress) Restrictable() public {
    token_ = Moritapo(tokenAddress); 
    //msg.sender should be initial owner of all token
    require(token_.balanceOf(msg.sender) > 0);
    inventory_ = Inventory(inventoryAddress);
    tokenSold_ = 0;
  }


  //reader
  function getTokenBalance() public view returns (uint256) {
    return token_.balanceOf(msg.sender);
  }
  //estimate breed fee
  function estimateBreedFee(uint cat_id, address target, uint target_cat_id) public view returns (uint) {
    return estimateBreedFee(msg.sender, cat_id, target, target_cat_id);
  }
  function estimateReclaimToken(uint index) returns (uint256) {
    var (c, clen) = inventory_.getSlotBytes(msg.sender, index);
    pb_neko_Cat.Data memory cat = pb_neko_Cat.decode(StorageHelper.toBytes(c, clen));
    return breedTokenFromCatValue(NekoUtil.evaluateCat(cat));
  }


  //writer
  //get fixed parameter initial cat according to sel_idx, also give some token
  function createInitialCat(address target, uint payment_unit, uint sel_idx, string name, bool debug_is_male) public writer returns (bool) {
    //world should have write access to inventory
    require(inventory_.checkWritableFrom(this));
    require(inventory_.getSlotSize(target) <= 0); //only once
    //give cat according to sel_idx
    PRNG.Data memory rnd;
    bool is_male = checkWritableFrom(target) ? debug_is_male : (rnd.gen2(0, 1) == 0);
    uint16[] memory skills = new uint16[](1);
    skills[0] = uint16(rnd.gen2(1, 64));
    if (sel_idx == 0) {
      //hp type
      inventory_.addFixedCat(target, name, 75, 10, 10, skills, is_male);
    } else if (sel_idx == 1) {
      //attack type
      inventory_.addFixedCat(target, name, 50, 20, 10, skills, is_male);
    } else if (sel_idx == 2) {
      //defense type
      inventory_.addFixedCat(target, name, 50, 10, 20, skills, is_male);
    }//*/
    //give initial token with current rate
    var amount = payment_unit / currentRateForPU();
    require(token_.privilegedTransfer(target, amount));
    Exchange(payment_unit, currentRateForPU(), tokenSold_, amount);
    tokenSold_ += amount;
    return true;
  }
  //just buy token with sent ether (where to have conversion rate?)
  function buyToken(address target, uint payment_unit) public writer {
    //give initial token with current rate
    uint amount = payment_unit / currentRateForPU();
    require(token_.privilegedTransfer(target, amount));
    Exchange(payment_unit, currentRateForPU(), tokenSold_, amount);
    tokenSold_ += amount;
  }
  //buy cat with set token price
  //sender have to approve from to spend 'price' token.
  function buyCat(address from, uint cat_id) public returns (bool) {
    require(inventory_.canReleaseCat(from));
    uint price = inventory_.getPrice(from, cat_id);
    require(price > 0); //ensure address 'from' has cat and for sale
    require(price < token_.balanceOf(msg.sender)); //ensure buyer have enough token
    require(token_.transferFrom(msg.sender, from, price));
    require(inventory_.transferCat(from, msg.sender, cat_id));
    return true;
  }
  //change your cat to token according to configured sell price
  function reclaimToken(uint index) public returns (bool) {
    require(inventory_.canReleaseCat(msg.sender));
    var (reclaim_amount, cat_id) = getStandardPrice(msg.sender, index);
    require(reclaim_amount > 0); //ensure sender has the cat
    require(token_.privilegedTransfer(msg.sender, reclaim_amount));
    require(inventory_.transferCat(msg.sender, administrator_, cat_id));
    return true;
  }
  //breed msg.sender's cat_id and target's target_cat_id
  //cat's sex need to be different. and need some token to pay
  //if required_token != 0, token is not enough. 
  //sender have to approve administrator_ to spend 'estimateBreedFee' 
  function breedCat(string name, uint cat_id, address target, uint target_cat_id) public returns (uint required_token) {
    var fee = estimateBreedFee(msg.sender, cat_id, target, target_cat_id);
    if (fee > token_.balanceOf(msg.sender)) {
      return fee;
    }
    require(inventory_.breed(name, msg.sender, cat_id, target, target_cat_id, -1));
    require(token_.transferFrom(msg.sender, administrator_, fee));
    return 0;
  }
  //set your cat for sale. if specify 0 to price, make corresponding cat not for sale.
  function setForSale(uint index, uint price) public returns (bool){
    return inventory_.setForSale(msg.sender, index, price);
  }


  //helper
  function currentRateForPU() internal view returns (uint) {
    //sale. rate is doubled for each TOKEN_DOUBLED_AMOUNT_THRESHOULD token sold
    var power = tokenSold_ / TOKEN_DOUBLED_AMOUNT_THRESHOULD;
    return 2**power;
  }
  function breedTokenFromCatValue(uint cv) internal view returns (uint) {
    return cv / CAT_VALUE_DIVIDE_BY_TOKEN;
  }
  function getStandardPrice(address reclaimer, uint index) public view returns (uint price, uint cat_id) {
    var (c, clen) = inventory_.getSlotBytes(reclaimer, index);
    var bs = StorageHelper.toBytes(c, clen);
    cat_id = inventory_.getSlotId(reclaimer, index);
    var cat = pb_neko_Cat.decode(bs);
    var base_price = NekoUtil.evaluateCat(cat);
    return (breedTokenFromCatValue(base_price), cat_id);
  }
  function estimateBreedFee(address sender, uint cat_id, address target, uint target_cat_id) internal view returns (uint) {
    return breedTokenFromCatValue(inventory_.estimateBreedValue(sender, cat_id, target, target_cat_id, -1));
  }
}
