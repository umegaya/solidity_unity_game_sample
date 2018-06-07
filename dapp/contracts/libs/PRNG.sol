pragma solidity ^0.4.17;

library PRNG {
    struct Data {
        uint seed;
    }
    function gen(Data d) internal view returns (uint) {
        d.seed = d.seed + 1;
        return uint(keccak256(abi.encodePacked(d.seed, block.timestamp, blockhash(block.number - 1))));
    }
    function gen2(Data d, uint min, uint max) internal view returns (uint) {
        if (min >= max) { return min; }
        return (gen(d) % (max - min)) + min;
    }
}
