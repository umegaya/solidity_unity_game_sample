pragma solidity ^0.4.24;

import './libs/Restrictable.sol';
import './libs/StorageAccessor.sol';
import './libs/pb/Issuance_pb.sol'; 
import "./libs/pb/CardSpec_pb.sol";

//this contract cannot be upgradable easily, so please do not add complex features in this contract.
contract Issuance is Restrictable, StorageAccessor {
  using pb_ch_Issuance for pb_ch_Issuance.Data;
  using pb_ch_Wagon for pb_ch_Wagon.Data;
  using pb_ch_Wagon_ProhibitList for pb_ch_Wagon_ProhibitList.Data;

  //card_id => issuance data
  mapping (uint => pb_ch_Issuance.Data) entries_;
  //rarity => card bucket for sale
  mapping (uint => pb_ch_Wagon.Data) wagons_;

  constructor(address storageAddress) Restrictable() StorageAccessor(storageAddress) public {

  }

  //add limitation. if returns 0, limitation set successfully, otherwise returns required least limit
  function SetIssuanceEntry(uint64 card_id, uint32 spec_id, uint rarity, uint32 insert_flags, uint32 limit) public writer returns (uint) {
    pb_ch_Issuance.Data storage ent = entries_[card_id];
    if (ent.version <= 0) {
      ent.version = 1;
    }
    (bool found, pb_ch_Issuance_Stat.Data storage stent) = ent.search_stats(insert_flags);
    if (found) {
      bool reach_limit = (stent.limit == stent.issued && stent.limit > 0);
      if (stent.limit > limit) {
        return stent.limit;
      }
      if (stent.issued >= limit) {
        return stent.issued + 1;
      }
      if (reach_limit) {
        PermitMint(spec_id, rarity, insert_flags);
      }
      stent.limit = limit;
    } else {
      pb_ch_Issuance_Stat.Data memory stentnew;
      stentnew.limit = limit;
      ent.add_stats(insert_flags, stentnew);
    }
    //save ent
    return 0; //ok
  }

  function ProhibitMint(uint32 spec_id, uint rarity, uint32 insert_flags) internal {
    pb_ch_Wagon.Data storage w = wagons_[rarity];
    (bool found, pb_ch_Wagon_ProhibitList.Data storage ph) = w.search_prohibits(insert_flags);
    if (found) {
      ph.spec_ids.push(spec_id);
    } else {
      pb_ch_Wagon_ProhibitList.Data memory newph;
      w.add_prohibits(insert_flags, newph);
      w.get_prohibits(insert_flags).spec_ids.push(spec_id);
    }
  }

  function PermitMint(uint32 spec_id, uint rarity, uint32 insert_flags) internal {
    pb_ch_Wagon.Data storage w = wagons_[rarity];
    (bool found, pb_ch_Wagon_ProhibitList.Data storage ph) = w.search_prohibits(insert_flags);
    if (found) {
      for (uint i = 0; i < ph.spec_ids.length; i++) {
        if (ph.spec_ids[i] == spec_id) {
          ph.spec_ids[i] = ph.spec_ids[ph.spec_ids.length - 1];
          ph.spec_ids.length--;
          break;
        }
      }
    }
  }

  function GetProhibitCardSpecs(uint rarity, uint32 insert_flags) public view returns (uint32[]) {
    pb_ch_Wagon.Data storage w = wagons_[rarity];
    (bool found, pb_ch_Wagon_ProhibitList.Data storage ph) = w.search_prohibits(insert_flags);
    if (found) {
      return ph.spec_ids;
    } else {
      uint32[] memory dummy_spec_ids;
      return dummy_spec_ids;
    }
  }

  //return 0 if cannot issue anymore. otherwise, returns resulting issue count
  function Issue(uint64 card_id, uint32 spec_id, uint rarity, uint32 insert_flags) public writer returns (uint) {
    pb_ch_Issuance.Data storage ent = entries_[card_id];
    if (ent.version <= 0) {
      ent.version = 1;
    }
    (bool found, pb_ch_Issuance_Stat.Data storage stent) = ent.search_stats(insert_flags);
    if (found) {
      if (stent.limit > 0 && stent.issued >= stent.limit) {
        return 0;
      }
      stent.issued++;
      if (stent.issued == stent.limit) {
        ProhibitMint(spec_id, rarity, insert_flags);
      }
      return stent.issued;
    } else {
      pb_ch_Issuance_Stat.Data memory stentnew;
      stentnew.issued = 1;
      if (insert_flags != 0) {
        stentnew.limit = 100; //default limitation
      }
      ent.add_stats(insert_flags, stentnew);
      return stentnew.issued;
    }
  }

  function Owned(uint64 card_id, uint32 insert_flags) public writer {
    pb_ch_Issuance.Data storage ent = entries_[card_id];
    require(ent.version > 0);//if not issued, should not Owned
    (bool found, pb_ch_Issuance_Stat.Data storage stent) = ent.search_stats(insert_flags);
    require(found); //same as above
    //basic state check
    require(stent.limit >= stent.issued);
    require(stent.issued > stent.owned); 
    stent.owned++;
  }

  function PutToWagon(uint64 card_id, uint rarity) public writer {
    pb_ch_Wagon.Data storage w = wagons_[rarity];
    w.card_ids.push(card_id);
  }

  function PickFromWagon(uint rarity, uint rand) public writer returns (uint card_id) {
    pb_ch_Wagon.Data storage w = wagons_[rarity];
    if (w.card_ids.length <= 0) {
      card_id = 0; //cannot pick
      return;
    }
    uint index = rand % w.card_ids.length;
    card_id = w.card_ids[index];
    w.card_ids[index] = w.card_ids[w.card_ids.length - 1];
    w.card_ids.length--;
  }
}
