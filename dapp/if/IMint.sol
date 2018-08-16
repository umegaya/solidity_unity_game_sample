pragma solidity ^0.4.24;

import "./libs/Restrictable.sol";
import "./libs/if/IAudit.sol";
import "./libs/if/IRegistry.sol";

contract IMint is Restrictable, StorageAccessor {
    IAudit audit_;
    IRegistry registry_;
    uint seed_;

    function createAsset(address user, bytes asset_payload) public writer returns (uint asset_id) {
        asset_id = ++seed_;
        saveBytes(asset_id, asset_payload);
        registry_.mint(user, asset_id);
        audit_.onCreateAsset(asset_id, asset_payload);
    }

    function destroyAsset(address user, uint asset_id) public writer {
        registery_.burn(user, asset_id)
        audit_.onDestroyAsset(id, asset_payload);
    }
}