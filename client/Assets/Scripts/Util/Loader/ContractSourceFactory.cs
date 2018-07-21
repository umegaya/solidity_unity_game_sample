using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;

using BigInteger = System.Numerics.BigInteger;

public class ContractSourceFactory : DataLoader.IDataSourceFactory {
    public const int BATCH_QUERY_SIZE = 32;
    public class ConstractSource<RECORD> : DataLoader.IDataSource {
        int size_, pos_;
        RECORD[] records_;
        public ConstractSource(int size) {
            size_ = size;
            pos_ = 0;
            records_ = new RECORD[size];
        }
        public bool Read() { 
            if (size_ > pos_) {
                pos_++;
                return true;
            }
            return false;
        }
        public R GetRecord<R>() {
            //should be always ok (because RECORDS == R always)
            Debug.Assert(typeof(R) == typeof(RECORD));
            return (R)Convert.ChangeType(records_[pos_ - 1], typeof(R));
        }

        internal RECORD[] GetRecords() {
            return records_;
        }

        internal void Add(RECORD r) {
            if (size_ > pos_) {
                records_[pos_++] = r;
            } else {
                Debug.Assert(false);
            }
        }
    }

    public DataLoader.IDataSource Source { get; set; }

    public IEnumerator Load<R>(DataLoader.IResultReceiver loader, string path)
        where R : Google.Protobuf.IMessage<R> {
        //get message parser by reflection
        PropertyInfo propertyInfo;
        propertyInfo = typeof(R).GetProperty("Parser", BindingFlags.Public | BindingFlags.Static); 
        // Use the PropertyInfo to retrieve the value from the type by not passing in an instance
        Google.Protobuf.MessageParser<R> parser = 
            (Google.Protobuf.MessageParser<R>)propertyInfo.GetValue(null, null);

        //get record name by reflection
        var eth = Game.Main.RPCMgr.Eth;
        string contract = path;
        string name = typeof(R).Name;

        //load local objects
        int current_gen = 0;
        var storage = Game.Main.StorageMgr;
        var local_records = storage.LoadAllRecords(name, parser, out current_gen);
        if (current_gen < 0) {
            loader.Error = new System.Exception("fail to load record");
            yield break;
        }

        //get size of updated records
        Debug.Log("request to " + contract);
        yield return eth[contract].Call("recordIdDiff", name, current_gen);
        Debug.Log("end request " + eth.CallResponse.Error.Message);
        if (eth.CallResponse.Error != null) {
            loader.Error = eth.CallResponse.Error;
            yield break;
        }
        int next_gen = eth.CallResponse.As<int>(1);
        byte[][][] ids = eth.CallResponse.As<byte[][][]>(2);

        //collection which stored update records
        ConstractSource<R> s = new ConstractSource<R>(ids.Length + local_records.Count);

        //if updated records found, retrieve it from contract
        if (ids.Length > 0) {
            HashSet<byte[]> hs = new HashSet<byte[]>();
            //de-dupe duplicate record (update multiple time since last updated time)
            for (int i = 0; i < ids.Length; i++) {
                for (int j = 0; j < ids[i].Length; i++) {
                    hs.Add(ids[i][j]);
                }
            }
            byte[][] updated_ids_distinct = hs.ToArray();
            R[] updated_records = new R[updated_ids_distinct.Length];
            
            //load sizeof current R[] and plus ids.Length, then allocate source object array
            s = new ConstractSource<R>(ids.Length + local_records.Count);

            //then query updated records
            for (int n_query = 0; n_query < updated_ids_distinct.Length; n_query += BATCH_QUERY_SIZE) {
                var batch_ids = updated_ids_distinct.Skip(n_query).Take(BATCH_QUERY_SIZE).ToArray();
                yield return eth[contract].Call("getRecords", name, batch_ids);
                if (eth.CallResponse.Error != null) {
                    loader.Error = eth.CallResponse.Error;
                    yield break;
                }
                var rs = eth.CallResponse.AsArray<R>(parser);
                for (int i = n_query; i < (n_query + batch_ids.Length); i++) {
                    local_records[batch_ids[i - n_query]] = rs[i];
                    updated_records[i] = rs[i];
                }
            }

            //save updated records into local storage
            storage.SaveRecords(next_gen, name, updated_ids_distinct, updated_records);
        }

        //set source
        Source = s;
        foreach (var kv in local_records) {
            s.Add(kv.Value);
        }
        yield break;
    }
}
