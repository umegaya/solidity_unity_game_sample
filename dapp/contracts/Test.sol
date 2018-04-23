contract Test {
    uint lastIndex_;
    function testCall(uint slot_index) public pure returns (uint) {
        require(slot_index < 1);
        return slot_index + 100;
    }
    function testTx(uint slot_index) public returns (uint) {
        require(slot_index < 1);
        lastIndex_ = slot_index;
        return slot_index + 100;        
    }
}
