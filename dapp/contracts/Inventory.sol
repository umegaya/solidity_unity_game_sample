pragma solidity ^0.4.24;

import "./libs/StorageAccessor.sol";
import "./libs/Restrictable.sol";
import "./libs/pb/Card_pb.sol";
import "./libs/pb/Payment_pb.sol";
import "./libs/pb/CardSpec_pb.sol";
import "./libs/PRNG.sol";
import "./libs/math/Math.sol";
import "./CalcUtil.sol";
import "./Constants.sol";
import "./Cards.sol";
import "./Issuance.sol";

//TODO: make it ERC721 compatible
contract Inventory is StorageAccessor, Restrictable {
  //defs
  using PRNG for PRNG.Data;
  using pb_ch_Card for pb_ch_Card.Data;
  using pb_ch_Payment for pb_ch_Payment.Data;
  using pb_ch_CardSpec for pb_ch_CardSpec.Data;


  //variables
  uint idSeed_;
  mapping(uint => uint) prices_;
  Cards cards_;
  Issuance issuance_;


  //events
  event MintCard(address indexed user, uint id, bytes created);
  event UpdateCard(address indexed user, uint id, bytes created);
  event Merge(address indexed user, uint remain_card_id, uint merged_card_id, bytes created);
  event ConsumeTx(address indexed user, string tx_id);


  //ctor
  constructor(address storageAddress, address cardsAddress, address issuanceAddress)  
    StorageAccessor(storageAddress) 
    Restrictable() public {
    issuance_ = Issuance(issuanceAddress);
    cards_ = Cards(cardsAddress);
    idSeed_ = 1;
  } 

  function setCard(address cardsAddress) public writer {
    cards_ = Cards(cardsAddress);
  }
  function setIssuances(address issuanceAddress) public writer {
    issuance_ = Issuance(issuanceAddress);
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
  function canReleaseCard(address user) public view returns (bool) {
    //cannot be the 'no card' status
    return cards_.balanceOf(user) > 1;
  }
  function estimateResultValue(uint source_card_id,
    uint target_card_id) public view returns (uint) {
    pb_ch_Card.Data memory new_card = createMergedCard(source_card_id, target_card_id);
    return CalcUtil.evaluate(new_card, this);
  }
  function createMergedCard(
    uint target_card_id, uint merged_card_id) internal view returns (pb_ch_Card.Data memory card) {
    require(target_card_id != merged_card_id);
    bool tmp;
    pb_ch_Card.Data memory ca;
    (ca, tmp) = CalcUtil.getCard(this, target_card_id);
    require(tmp);

    pb_ch_Card.Data memory cb;
    (cb, tmp) = CalcUtil.getCard(this, merged_card_id);
    require(tmp); //*/

    require(ca.spec_id == cb.spec_id);

    card.spec_id = ca.spec_id;
    card.stack = ca.stack + 1;
    card.insert_flags = ca.insert_flags;
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
    int) public writer returns (bytes) {
    //verify
    require(cards_.ownerOf(source_card_id) == user);
    require(cards_.ownerOf(target_card_id) == user);
    //create merged card data
    pb_ch_Card.Data memory card = createMergedCard(source_card_id, target_card_id);
    //update card dat
    bytes memory bs = card.encode();
    saveBytes(source_card_id, bs);
    //issue events
    emit Merge(user, source_card_id, target_card_id, bs);
    //burn merged card
    cards_.burn(user, target_card_id);
    return bs;
  }
  function mintCard(address user) public writer returns (uint) {
    PRNG.Data memory rnd;
    return mintFixedCard(user, 
      uint32(rnd.gen2(1, 10000)), CalcUtil.RandomInsertFlag(), 1);
  }
  function mintFixedCard(address user, 
                      uint32 spec_id, uint32 insert_flags, uint32 stack) 
                      public writer returns (uint) {
    pb_ch_Card.Data memory c;
    c.spec_id = spec_id;
    c.insert_flags = insert_flags;
    c.stack = stack;
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
