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

        internal void Rewind() { pos_ = 0; }
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
        if (eth[contract] == null) {
            loader.Error = new System.Exception("contract not exists: name = " + contract);
            yield break;
        }
        var call = eth[contract].Call();
        yield return call.Exec("recordIdDiff", name, current_gen);
        if (call.Error != null) {
            loader.Error = call.Error;
            yield break;
        }
        int next_gen;
        List<List<byte[]>> ids;
        try {
            if (int.TryParse(call.As<BigInteger>(0).ToString(), out next_gen) && next_gen > 0) {
                ids = call.As<List<List<byte[]>>>(1);
            } else {
                throw new System.Exception(name + ":gen number is too big or out of range:" + 
                    call.As<BigInteger>(0).ToString());
            }
        } catch (System.Exception e) {
            Debug.Log("recordIdDiff error:" + e.Message);
            loader.Error = e;
            yield break;
        }

        //if updated records found, retrieve it from contract
        if (ids.Count > 0) {
            HashSet<byte[]> hs = new HashSet<byte[]>();
            //de-dupe duplicate record (update multiple time since last updated time)
            for (int i = 0; i < ids.Count; i++) {
                for (int j = 0; j < ids[i].Count; j++) {
                    hs.Add(ids[i][j]);
                }
            }
            byte[][] updated_ids_distinct = hs.ToArray();
            R[] updated_records = new R[updated_ids_distinct.Length];
            
            //then query updated records
            for (int n_query = 0; n_query < updated_ids_distinct.Length; n_query += BATCH_QUERY_SIZE) {
                var batch_ids = updated_ids_distinct.Skip(n_query).Take(BATCH_QUERY_SIZE).ToArray();
                Debug.Log(name + ": n_query = " + n_query + " and " + batch_ids.Length);
                call = eth[contract].Call();
                yield return call.Exec("getRecords", name, batch_ids);
                if (call.Error != null) {
                    loader.Error = call.Error;
                    yield break;
                }
                try {
                    var rs = call.AsArray<R>(parser, 0);
                    for (int i = n_query; i < (n_query + batch_ids.Length); i++) {
                        local_records[batch_ids[i - n_query]] = rs[i];
                        updated_records[i] = rs[i];
                    }
                } catch (System.Exception e) {
                    loader.Error = e;
                    yield break;
                }
            }

            //save updated records into local storage
            var err = storage.SaveRecords(next_gen, name, updated_ids_distinct, updated_records);
            if (err != null) {
                loader.Error = err;
                yield break;
            }
        } else {
            Debug.Log(name + ": no update since last access:" + local_records.Count);
        }

        //set source
        var s = new ConstractSource<R>(local_records.Count);   
        Source = s;          
        foreach (var kv in local_records) {
            s.Add(kv.Value);
        }
        s.Rewind();
        yield break;
    }
}
