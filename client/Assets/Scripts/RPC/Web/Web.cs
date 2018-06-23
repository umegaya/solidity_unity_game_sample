using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace Game.RPC {
public class Web : MonoBehaviour {
    public string end_point_;
    internal ulong id_seed_ = 1;
    internal Dictionary<ulong, Request> requests_ = new Dictionary<ulong, Request>();

    public class Request {
        WWW www_;
        Web owner_;
        ulong msgid_;
        public Request(Web owner) { 
            owner_ = owner; 
            msgid_ = owner_.id_seed_++;
            owner_.requests_[msgid_] = this;
        }
        public IEnumerator Call<REQ>(string func, REQ args) { 
            var json = JsonConvert.SerializeObject(args);
            Debug.Log("encoded json:" + json);
            www_ = new WWW(owner_.end_point_ + "/" + func,  System.Text.Encoding.UTF8.GetBytes(json), 
                new Dictionary<string, string> {
                    {"Content-Type", "application/json"},
                }
            );
            yield return www_;
            Debug.Log(owner_.end_point_ + " => end request:" + www_.error + "|" + System.Text.Encoding.UTF8.GetString(www_.bytes));
            owner_.requests_.Remove(msgid_);
        }
        public RES As<RES>() {
            try {
                if (www_.error != null) {
                    return default(RES);
                }
                var json = System.Text.Encoding.UTF8.GetString(www_.bytes);
                return JsonConvert.DeserializeObject<RES>(json);
            } finally {
                owner_.requests_.Remove(msgid_);
            }
        }
        public string ResponseText {
            get {
                return System.Text.Encoding.UTF8.GetString(www_.bytes);
            }
        }
        public System.Exception Error {
            get {
                if (www_.error == null) {
                    return null;
                }
                return new System.Exception(www_.error);
            }
        }
    }

    public Request NewReq() {
        return new Request(this);
    }
}
}