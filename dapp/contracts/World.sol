pragma solidity ^0.4.24;

import "./Inventory.sol";
import "./Moritapo.sol";
import "./Constants.sol";
import "./libs/Restrictable.sol";
import "./libs/pb/Card_pb.sol";
import "./libs/PRNG.sol";
import "./CalcUtil.sol";

//TODO: make this ERC721 token
contract World is Restrictable, Constants {
  //defs
  using PRNG for PRNG.Data;


  //variables
  Moritapo token_;
  Inventory inventory_;
  uint tokenSold_;
  uint clock_; //current world date time. updated with minutes frequency from authorized host


  //event
  event Exchange(uint value, uint rate, uint tokenSold, uint result);
  event MintCard(address indexed user, uint id, bytes created);
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
  function estimateMergeFee(uint card_id, uint target_card_id) public view returns (uint) {
    return mergeFeeFromCardValue(inventory_.estimateResultValue(card_id, target_card_id));
  }
  function estimateReclaimToken(uint card_id) public view returns (uint256) {
    bytes memory c = inventory_.getSlotBytesById(card_id);
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
    if (sel_idx == 0) {
      //hp type
      inventory_.mintFixedCard(target, 1001, 0, 1, 4);
    } else if (sel_idx == 1) {
      //attack type
      inventory_.mintFixedCard(target, 1002, 0, 1, 4);
    } else if (sel_idx == 2) {
      //defense type
      inventory_.mintFixedCard(target, 1003, 0, 1, 4);
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
  function updateClock(uint current_clock) public admin {
    clock_ = current_clock;
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
  function buyCard(address from, uint card_id) public returns (bool) {
    uint price = inventory_.getPrice(card_id);
    require(price > 0); //ensure address 'from' has cat and for sale
    require(price < token_.balanceOf(msg.sender)); //ensure buyer have enough token
    inventory_.transferCard(from, msg.sender, card_id);
    require(token_.transferFrom(msg.sender, from, price));
    return true;
  }
  //change your cat to token according to configured sell price
  function reclaimToken(uint card_id) public returns (bool) {
    require(inventory_.canReleaseCard(msg.sender));
    uint reclaim_amount = getStandardPrice(card_id);
    inventory_.returnCard(msg.sender, card_id);
    require(token_.privilegedTransfer(msg.sender, reclaim_amount));
    return true;
  }
  //breed msg.sender's cat_id and target's target_cat_id
  //cat's sex need to be different. and need some token to pay
  //if required_token != 0, token is not enough. 
  //sender have to approve administrator_ to spend 'estimateBreedFee' 
  function mergeCard(uint card_id, uint target_card_id) public returns (uint) {
    uint fee = estimateMergeFee(card_id, target_card_id);
    if (fee > token_.balanceOf(msg.sender)) {
      return fee;
    }
    inventory_.merge(msg.sender, card_id, target_card_id, -1);
    require(token_.transferFrom(msg.sender, administrator_, fee));
    return 0;
  }
  //set your cat for sale. if specify 0 to price, make corresponding cat not for sale.
  function setForSale(uint id, uint price) public {
    inventory_.setForSale(msg.sender, id, price);
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
  function getStandardPrice(uint card_id) public view returns (uint) {
    bytes memory bs = inventory_.getSlotBytesById(card_id);
    pb_ch_Card.Data memory card = pb_ch_Card.decode(bs);
    uint base_price = CalcUtil.evaluate(card);
    return mergeFeeFromCardValue(base_price);
  }
}
