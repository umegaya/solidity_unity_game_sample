using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Globalization;

using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.UnityClient;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Game.Web3Util;

using UnityEngine;

namespace Game.Web3 {
public class GetTransactionReceiptRequest:UnityRpcClient<Dictionary<string, object>>
{
    private readonly Nethereum.RPC.Eth.Transactions.EthGetTransactionReceipt _ethGetTransactionReceipt;
    public GetTransactionReceiptRequest(string url, JsonSerializerSettings jsonSerializerSettings = null):base(url, jsonSerializerSettings) {
        _ethGetTransactionReceipt = new Nethereum.RPC.Eth.Transactions.EthGetTransactionReceipt(null);
    }
    public IEnumerator SendRequest(System.String transactionHash) {
        var request = _ethGetTransactionReceipt.BuildRequest(transactionHash);
        yield return SendRequest(request);
    }
}
public class Receipt : Dictionary<string, object> {
    public class Log {
        JToken src;
        public Log(JToken s) {
            src = s;
        }
        public HexBigInteger LogIndex { get { return ((string)src["logIndex"]).AsInt(); } }
        public HexBigInteger TransactionIndex { get { return ((string)src["transactionIndex"]).AsInt(); } }
        public string TransactionHash { get { return (string)src["transactionHash"]; } }
        public string BlockHash { get{ return (string)src["blockHash"]; } }
        public string Address { get{ return (string)src["address"]; } }
        public string Data { get{ return (string)src["data"]; } }
        public JArray Topics { get{ return (JArray)src["topics"]; } }
        public string Type { get{ return (string)src["type"]; } }
    };
    protected List<Log> logs_cache = null;
    public Receipt(IDictionary<string, object> src) : base(src) {}
    public static HexBigInteger AsInt(object o) {
        var src = (string)o;
        return src.AsInt();
    }
    public static string AsStr(object o) {
        return (string)o;
    }
    public string TransactionHash { get { return AsStr(this["transactionHash"]); } }
    public HexBigInteger TransactionIndex { get { return AsInt(this["transactionIndex"]); } }
    public string BlockHash { get { return AsStr(this["blockHash"]); } }
    public HexBigInteger BlockNumber { get { return AsInt(this["blockNumber"]); } }
    public HexBigInteger GasUsed { get { return AsInt(this["gasUsed"]); } }
    public HexBigInteger CumulativeGasUsed { get { return AsInt(this["cumulativeGasUsed"]); } }
    public string ContractAddress { get { return AsStr(this["contractAddress"]); } }
    public bool Status { get { return AsInt(this["status"]).Value > 0; }}
    public JArray Logs { get { return (JArray)this["logs"]; }}

    public void Dump() {
        Debug.Log("Receipt" + 
            " status:" + Status + 
            ",txhash:" + TransactionHash + 
            ",txidx:" + TransactionIndex.Value + 
            ",bhash:" + BlockHash + 
            ",bnum:" + BlockNumber.Value + 
            ",gasUsed:" + GasUsed.Value + 
            ",cumulativeGasUsed:" + CumulativeGasUsed.Value + 
            ",contractAddress:" + ContractAddress
        );

        for (int i = 0; i < Logs.Count; i++) {
            var l = new Log(Logs[i]);
            Debug.Log("Log[" + i + "]" + 
                " logidx:" + l.LogIndex.Value + 
                ",txidx:" + l.TransactionIndex.Value + 
                ",bhash:" + l.BlockHash + 
                ",addr:" + l.Address + 
                ",type:" + l.Type + 
                ",data:" + l.Data 
            );
            for (int j = 0; j < l.Topics.Count; j++) {
                Debug.Log("Log[" + i + "] topics[" + j + "]:" + l.Topics[j]);
            }
        }
    }//*/
}
}
