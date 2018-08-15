pragma solidity ^0.4.24;

import "./libs/Restrictable.sol";
import "./libs/PRNG.sol";
import "./libs/if/ICurrency.sol";
import "./libs/if/IAsset.sol";
import "./libs/if/IMinter.sol";
import "./libs/if/IUser.sol";


contract IWorld is Restrictable {
  //defs
  using PRNG for PRNG.Data;


  //variables
  ICurrency currency_;
  IMinter minter_;
  IUser users_;
  IAsset assets_;
  uint clock_; //current world date time. updated with minutes frequency from authorized host


  //event
  event Exchange(uint value, uint rate, uint tokenSold, uint result);
  event MintCard(address indexed user, uint id, bytes created);
  event Approval(address indexed owner, address indexed spender, uint256 value); //from ERC20.sol
  event Transfer(address indexed from, address indexed to, uint256 value); //from ERC20.sol
  event Merge(address indexed user_a, address indexed user_b, uint id_a, uint id_b, uint new_id);

  event Error(address sender, uint require, uint allowance); //debug event

  //ctor
  constructor(
    address currencyAddress, 
    address minterAddress, 
    address userAddress
    address assetAddress) Restrictable() public {
    token_ = Moritapo(tokenAddress); 
    //msg.sender should be initial owner of all token
    require(currency_.balanceOf(msg.sender) > 0);
    currency_ = ICurrency(currencyAddress);
    minter_ = IMinter(minterAddress);
    users_ = IUser(userAddress);
    assets_ = IAsset(assetAddress);
    clock_ = 0;
  }

  //administraction
  function setCurrency(address a) public admin {
    currency_ = ICurrency(a);
  }
  function setMinter(address a) public admin {
    minter_ = IMinter(a);
  }
  function setUsers(address a) public admin {
    users_ = IUser(a);
  }
  function setAssets(address a) public admin {
    assets_ = IAsset(a);
  }
  function setClock(uint c) public admin {
    clock_ = c;
  }


  //writer
  function createUserAssets(address target, bytes payload) public writer returns (bool);
  function createUser(address target, string tx_id, uint payment_amount, bytes payload) public writer returns (bool) {
    //world should have write access to inventory
    require(minter_.checkWritableFrom(this));
    require(currency_.checkWritableFrom(this));
    require(users_.checkWritableFrom(this));
    require(assets_.balanceOf(target) <= 0); //only once
    //exchange payment amount with currency
    require(currency_.recordPayment(target, tx_id));
    uint exchanged = currency_.privilegedTransfer(target, payment_amount);
    require(exchanged > 0);
    require(createUserAssets(target, payload));
    return true;
  }
  //just buy token with sent ether (where to have conversion rate?)
  function buyToken(address target, string tx_id, uint payment_amount) public writer {
    require(currency_.recordPayment(target, tx_id));
    uint exchanged = currency_.privilegedTransfer(target, payment_amount);
    require(exchanged > 0);
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
    uint base_price = CalcUtil.evaluate(card, inventory_);
    return mergeFeeFromCardValue(base_price);
  }

  function starterPack(address target, uint32[] spec_ids, uint32[] prns) public writer {
    if (inventory_.getSlotSize(target) != 0) {
      return;
    }
    require(spec_ids.length == STARTER_PACK_CARD_COUNT);
    require(prns.length == STARTER_PACK_CARD_COUNT);
    for (uint i = 0; i < STARTER_PACK_CARD_COUNT; i++) {
      inventory_.mintFixedCard(target, spec_ids[i], prns[i] & 7, 0);
    }
  } 

  /*function updateShopList(address target) public writer {

  }

  function spawnTreasureBox(address target) public writer {

  }

  function hasBoxSlots(address target) public view returns (bool) {
    return true;
  }*/
}
