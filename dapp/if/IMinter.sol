pragma solidity ^0.4.24;

import "./libs/Restrictable.sol";
import "./libs/if/IAudit.sol";

contract IMinter is Restrictable {
    IAudit audit_;   
}