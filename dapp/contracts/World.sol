pragma solidity ^0.4.17;

import "./Inventory.sol";
import "./Moritapo.sol";
import "./Constants.sol";
import "./libs/math/SafeMath.sol";
import "./libs/Restrictable.sol";
import "./libs/pb/Cat_pb.sol";
import "./NekoUtil.sol";


contract World is Restrictable, Constants {
  //defs
  uint constant CREATE_INITIAL_CAT_TX_FEE = 0.01 ether;
  uint constant TOKEN_DOUBLED_AMOUNT_THRESHOULD = 10 * 1 ether; //token price doubled for every 1000 ether sold
  uint constant BASE_TOKEN_PRICE_IN_WEI = 1; //base price: 1 wei == 1 moritapo
  using SafeMath for uint;


  //variables
  Moritapo token_;
  Inventory inventory_;
  uint tokenSold_;


  //ctor
  function World(address tokenAddress, address inventoryAddress) Restrictable() public {
    token_ = Moritapo(tokenAddress); 
    //msg.sender should be initial owner of all token
    require(token_.balanceOf(msg.sender) > 0);
    inventory_ = Inventory(inventoryAddress);
    tokenSold_ = 0;
  }


  //writer
  //get fixed parameter initial cat according to sel_idx, also give some token
  function createInitialCat(uint sel_idx, string name) public payable returns (bool) {
    //world should have write access to inventory
    require(inventory_.checkWritableFrom(this));
    require(inventory_.getSlotSize(msg.sender) <= 0);
    //check send value is enough
    if (msg.value >= CREATE_INITIAL_CAT_TX_FEE) {
      revert();
      return false;
    }
    //give cat according to sel_idx
    uint16[] memory skills;
    if (sel_idx == 0) {
      //hp type
      inventory_.addFixedCat(msg.sender, name, 75, 10, 10, skills);
    } else if (sel_idx == 1) {
      //attack type
      inventory_.addFixedCat(msg.sender, name, 50, 20, 10, skills);
    } else if (sel_idx == 2) {
      //defense type
      inventory_.addFixedCat(msg.sender, name, 50, 10, 20, skills);
    }
    //give initial token with current rate
    uint amount = msg.value / currentRateInWei();
    require(token_.transferFrom(administrator_, msg.sender, amount));
    tokenSold_ += amount;
    return true;
  }
  //just buy token with sent ether (where to have conversion rate?)
  function buyToken() public payable {
    //give initial token with current rate
    uint amount = msg.value / currentRateInWei();
    require(token_.transferFrom(administrator_, msg.sender, amount));
    tokenSold_ += amount;
  }
  //buy cat with set token price
  function buyCat(address from, uint cat_id) public returns (bool) {
    uint price = inventory_.getPrice(from, cat_id);
    require(price > 0); //ensure from has cat and for sale
    require(price < token_.balanceOf(msg.sender)); //ensure buyer have enough money
    require(token_.transferFrom(from, msg.sender, price));
    require(inventory_.transferCat(from, msg.sender, cat_id));
    return true;
  }
  //change your cat to token according to configured sell price
  function reclaimToken(uint index) public returns (bool) {
    var (reclaim_amount, cat_id) = getStandardPrice(msg.sender, index);
    require(reclaim_amount > 0); //ensure sender has the cat
    require(inventory_.transferCat(msg.sender, administrator_, cat_id));
    require(token_.transferFrom(administrator_, msg.sender, reclaim_amount));
    return true;
  }
  //breed msg.sender's cat_id and target's target_cat_id
  //cat's sex need to be different. and need some token to pay
  //if required_token != 0, token is not enough. 
  function breedCat(string name, uint cat_id, address target, uint target_cat_id) public returns (uint required_token) {
    var fee = estimateBreedFee(msg.sender, cat_id, target, target_cat_id);
    if (fee > token_.balanceOf(msg.sender)) {
      return fee;
    }
    require(inventory_.breed(name, msg.sender, cat_id, target, target_cat_id, -1));
    require(token_.transferFrom(msg.sender, administrator_, fee));
    return 0;
  }
  //estimate breed fee
  function estimateBreedFee(uint cat_id, address target, uint target_cat_id) public returns (uint) {
    return inventory_.estimateBreedFee(msg.sender, cat_id, target, target_cat_id, -1) * currentRateInWei();
  }  
  //set your cat for sale. if specify 0 to price, make corresponding cat not for sale.
  function setForSale(uint index, uint price) public returns (bool){
    return inventory_.setForSale(msg.sender, index, price);
  }


  //helper
  function currentRateInWei() public view returns (uint) {
    //sale. rate is doubled for each TOKEN_DOUBLED_AMOUNT_THRESHOULD token sold
    var power = tokenSold_ / TOKEN_DOUBLED_AMOUNT_THRESHOULD;
    return BASE_TOKEN_PRICE_IN_WEI ^ power;
  }
  function getStandardPrice(address reclaimer, uint index) public view returns (uint price, uint cat_id) {
    var (c, clen) = inventory_.getSlotBytes(reclaimer, index);
    var bs = StorageHelper.toBytes(c, clen);
    cat_id = inventory_.getSlotId(reclaimer, index);
    var cat = pb_neko_Cat.decode(bs);
    var base_price = NekoUtil.evaluateCat(cat);
    return (base_price * currentRateInWei() / 10, cat_id);
  }
  function estimateBreedFee(address sender, uint cat_id, address target, uint target_cat_id) internal returns (uint) {
    return inventory_.estimateBreedFee(sender, cat_id, target, target_cat_id, -1) * currentRateInWei();
  }
}
