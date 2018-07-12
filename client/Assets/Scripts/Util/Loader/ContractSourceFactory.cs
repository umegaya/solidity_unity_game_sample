using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using UnityEngine;

public class ContractSourceFactory : DataLoader.IDataSourceFactory {
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

        internal void Add(RECORD r) {
            if (size_ > pos_) {
                records_[pos_++] = r;
            } else {
                Debug.Assert(false);
            }
        }
    }

    public DataLoader.IDataSource Source { get; private set; }

    public IEnumerator Load<R>(DataLoader.IResultReceiver loader, string path)
        where R : Google.Protobuf.IMessage<R> {
        //load newest record timestamp of R[]

        //get message parser by reflection
        PropertyInfo propertyInfo;
        propertyInfo = typeof(R).GetProperty("Parser", BindingFlags.Public | BindingFlags.Static); 
        // Use the PropertyInfo to retrieve the value from the type by not passing in an instance
        Google.Protobuf.MessageParser<R> parser = 
            (Google.Protobuf.MessageParser<R>)propertyInfo.GetValue(null, null);

        //parse path = https://{url}/{ContractName}
        Regex rgx = new Regex(@"https://([^/]+)/([^/]+)");
        Match m = rgx.Match(path);
        var eth = Game.Main.RPCMgr.Eth;
        string url = m.Groups[1].ToString(), contract = m.Groups[2].ToString();
        string name = typeof(R).Name;

        //first, get size of updated records
        yield return eth[contract].Call("recordIdDiff", name);
        if (eth.CallResponse.Error != null) {
            loader.Error = eth.CallResponse.Error;
            yield break;
        }
        uint[] ids = eth.CallResponse.As<uint[]>();

        //load sizeof current R[] and plus ids.Length, then allocate source object array
        ConstractSource<R> s = new ConstractSource<R>(ids.Length);

        //then load local objects into s first

        //then query updated records
        yield return eth[contract].Call("getRecords", name, ids);
        if (eth.CallResponse.Error != null) {
            loader.Error = eth.CallResponse.Error;
            yield break;
        }
        var rs = eth.CallResponse.AsArray<R>(parser);
        foreach (var r in rs) {
            s.Add(r);
        }
        //save updated records into local storage

        //set source
        Source = s;
        yield break;
    }
}
