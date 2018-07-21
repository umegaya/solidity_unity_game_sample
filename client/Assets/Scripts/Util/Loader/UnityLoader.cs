using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UniRx;

public abstract class UnityLoader : MonoBehaviour, DataLoader.IResultReceiver {
    public enum EventType {
        Inititalized,
        Error,
    };
    public struct Event {
        public EventType Type;
        public System.Exception Error;
    }

    public Subject<Event> subject_ = new Subject<Event>();
    public string BasePath;
    public System.Exception Error { get; set; }
    [System.Serializable] public struct SourceLocation {
        public string name_;
        public string url_;
    }
    public SourceLocation[] locations_;
    public Dictionary<string, string> Locations {
        get {
            var r = new Dictionary<string, string>();
            foreach (var l in locations_) {
                if (l.url_.StartsWith("http") || l.url_.IndexOf("/") == -1) {
                    r[l.name_] = l.url_;
                } else if (l.url_.StartsWith("file://")) {
                    r[l.name_] = BasePath + "/" + l.url_.Substring(7);
                } else {
                    r[l.name_] = BasePath + "/" + l.url_;
                }
            }
            return r;
        }
    }

    public abstract IEnumerator Load(Dictionary<string, string> locations);

    void Awake() {
        if (string.IsNullOrEmpty(BasePath)) {
            BasePath = Application.streamingAssetsPath;
        }
        Game.Main.FiberMgr.Start(StartLoad);
    }
    public IEnumerator StartLoad() {
        //TODO: update streaming asset
        yield return Load(Locations);
        if (Error != null) {
            yield return Error;
        }
        subject_.OnNext(new Event{ Type = EventType.Inititalized });
    }
}    
