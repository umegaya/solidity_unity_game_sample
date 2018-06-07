pragma solidity ^0.4.17;

contract Restrictable {
    enum Privilege { None, Read, Write }
    address public administrator_;
    mapping (address => Privilege) public members_;

    constructor() public {
        administrator_ = msg.sender;
    }

    modifier admin {
        require(msg.sender == administrator_);
        _;
    }

    modifier writer {
        require(checkWritableFrom(msg.sender));
        _;
    }

    modifier reader {
        require(checkReadableFrom(msg.sender));
        _;        
    }

    function changeOwner(address newAdmin) public admin {
        if (newAdmin != address(0)) {
            administrator_ = newAdmin;
        }
    }

    function setPrivilege(address member, Privilege p) public admin {
        if (uint(p) <= uint(Privilege.Write)) {
            members_[member] = p;
        }
    }

    function checkWritableFrom(address sender) view public returns (bool) {
      return sender == administrator_ || uint(members_[sender]) >= uint(Privilege.Write);
    }

    function checkReadableFrom(address sender) view public returns (bool) {
      return sender == administrator_ || uint(members_[sender]) >= uint(Privilege.Read);
    }
}
