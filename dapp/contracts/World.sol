pragma solidity ^0.4.24;

import "./Inventory.sol";
import "./Moritapo.sol";
import "./Constants.sol";
import "./libs/Restrictable.sol";
import "./libs/pb/Card_pb.sol";
import "./libs/PRNG.sol";
import "./CalcUtil.sol";


contract World is Restrictable, Constants {
  //defs
  using PRNG for PRNG.Data;


  //variables
  Moritapo token_;
  Inventory inventory_;
  uint tokenSold_;


  //event
  event Exchange(uint value, uint rate, uint tokenSold, uint result);
  event AddCard(address indexed user, uint id, bytes created); //from Inventory.sol
  event Approval(address indexed owner, address indexed spender, uint256 value); //from ERC20.sol
  event Transfer(address indexed from, address indexed to, uint256 value); //from ERC20.sol
  event Merge(address indexed user_a, address indexed user_b, uint id_a, uint id_b, uint new_id);

  event Error(address sender, uint require, uint allowance);

  //ctor
  constructor(address tokenAddress, address inventoryAddress) Restrictable() public {
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
  function estimateMergeFee(uint cat_id, address target, uint target_cat_id) public view returns (uint) {
    return estimateMergeFee(msg.sender, cat_id, target, target_cat_id);
  }
  function estimateReclaimToken(uint index) internal view returns (uint256) {
    bytes memory c = inventory_.getSlotBytes(msg.sender, index);
    pb_ch_Card.Data memory card = pb_ch_Card.decode(c);
    return mergeFeeFromCardValue(CalcUtil.evaluate(card));
  }


  //writer
  //get fixed parameter initial cat according to sel_idx, also give some token
  function createInitialDeck(
    address target, string tx_id, uint payment_unit, uint sel_idx) 
    public writer returns (bool) {
    //world should have write access to inventory
    require(inventory_.checkWritableFrom(this));
    require(inventory_.getSlotSize(target) <= 0); //only once
    require(inventory_.recordPayment(target, tx_id));
    //give cat according to sel_idx
    PRNG.Data memory rnd;
    uint16[] memory skills = new uint16[](1);
    skills[0] = uint16(rnd.gen2(1, 64));
    if (sel_idx == 0) {
      //hp type
      inventory_.addFixedCard(target, 75, 10, 10, skills);
    } else if (sel_idx == 1) {
      //attack type
      inventory_.addFixedCard(target, 50, 20, 10, skills);
    } else if (sel_idx == 2) {
      //defense type
      inventory_.addFixedCard(target, 50, 10, 20, skills);
    }//*/
    //give initial token with current rate
    uint amount = payment_unit / currentRateForPU();
    require(token_.privilegedTransfer(target, amount));
    emit Exchange(payment_unit, currentRateForPU(), tokenSold_, amount);
    tokenSold_ += amount;
    return true;
  }
  function payForInitialCard(uint sel_idx) public payable {
    createInitialDeck(msg.sender, "hoge", 10000, sel_idx);
  }
  //just buy token with sent ether (where to have conversion rate?)
  function buyToken(address target, string tx_id, uint payment_unit) public writer {
    //give initial token with current rate
    uint amount = payment_unit / currentRateForPU();
    require(inventory_.recordPayment(target, tx_id));
    require(token_.privilegedTransfer(target, amount));
    emit Exchange(payment_unit, currentRateForPU(), tokenSold_, amount);
    tokenSold_ += amount;
  }
  //buy cat with set token price
  //sender have to approve from to spend 'price' token.
  function buyCat(address from, uint cat_id) public returns (bool) {
    require(inventory_.canReleaseCard(from));
    uint price = inventory_.getPrice(from, cat_id);
    require(price > 0); //ensure address 'from' has cat and for sale
    require(price < token_.balanceOf(msg.sender)); //ensure buyer have enough token
    require(token_.transferFrom(msg.sender, from, price));
    require(inventory_.transferCard(from, msg.sender, cat_id));
    return true;
  }
  //change your cat to token according to configured sell price
  function reclaimToken(uint index) public returns (bool) {
    require(inventory_.canReleaseCard(msg.sender));
    (uint reclaim_amount, uint cat_id) = getStandardPrice(msg.sender, index);
    require(reclaim_amount > 0); //ensure sender has the cat
    require(token_.privilegedTransfer(msg.sender, reclaim_amount));
    require(inventory_.transferCard(msg.sender, administrator_, cat_id));
    return true;
  }
  //breed msg.sender's cat_id and target's target_cat_id
  //cat's sex need to be different. and need some token to pay
  //if required_token != 0, token is not enough. 
  //sender have to approve administrator_ to spend 'estimateBreedFee' 
  function mergeCard(uint cat_id, address target, uint target_cat_id) public returns (uint required_token) {
    uint fee = estimateMergeFee(msg.sender, cat_id, target, target_cat_id);
    if (fee > token_.balanceOf(msg.sender)) {
      return fee;
    }
    require(inventory_.merge(msg.sender, cat_id, target, target_cat_id, -1));
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
    uint power = tokenSold_ / TOKEN_DOUBLED_AMOUNT_THRESHOULD;
    return 2**power;
  }
  function mergeFeeFromCardValue(uint cv) internal pure returns (uint) {
    return cv / CAT_VALUE_DIVIDE_BY_TOKEN;
  }
  function getStandardPrice(address reclaimer, uint index) public view returns (uint price, uint cat_id) {
    bytes memory bs = inventory_.getSlotBytes(reclaimer, index);
    cat_id = inventory_.getSlotId(reclaimer, index);
    pb_ch_Card.Data memory card = pb_ch_Card.decode(bs);
    uint base_price = CalcUtil.evaluate(card);
    return (mergeFeeFromCardValue(base_price), cat_id);
  }
  function estimateMergeFee(address sender, uint cat_id, address target, uint target_cat_id) internal view returns (uint) {
    return mergeFeeFromCardValue(inventory_.estimateResultValue(sender, cat_id, target, target_cat_id, -1));
  }
}
