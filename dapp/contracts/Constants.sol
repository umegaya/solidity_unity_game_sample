pragma solidity ^0.4.24;

contract Constants {
  //moritapo initial supply
  uint256 public constant MRT_INITIAL_SUPPLY = 100000000 * 1 ether;

  //player at least send this value to start game
  uint256 public constant CREATE_INITIAL_CAT_TX_FEE = 0.01 ether;

  //how many token spent for a cat evaluation
  uint256 public constant CAT_VALUE_DIVIDE_BY_TOKEN = 100;

  //token price doubled for every 10 million token sold
  uint256 public constant TOKEN_DOUBLED_AMOUNT_THRESHOULD = 10**7; 

  //how many card packed in starter pack?
  uint256 public constant STARTER_PACK_CARD_COUNT = 12;

  //max shop entry
  uint256 public constant MAX_SHOP_ENTRY = 3;

  //shop sale duration
  uint256 public constant SHOP_SALE_DURATION_IN_SEC = 86400; //24h
}