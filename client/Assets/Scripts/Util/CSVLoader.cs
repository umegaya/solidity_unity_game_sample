using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UniRx;

public class CSVIO {
    static public IEnumerator Read(Loader loader, string path, Action<TextReader> cb) {
        try {
        #if UNITY_ANDROID && !UNITY_EDITOR
            WWW www = new WWW(path);
            yield return www;
            if (!string.IsNullOrEmpty(www.error)) {
                throw new System.Exception(www.error);
            }
            cb(new StringReader(www.text));
        #else
            cb(new StreamReader(TrimBOM(path)));
        #endif        
        } catch (System.Exception e) {
            loader.Error = e;
        }
        yield break;
    }
    static public string TrimBOM(string text) {
        if (string.IsNullOrEmpty(text)) {
            return text;
        }
        //\xef\xbb\xbf
        if (((byte)text[0]) == 0xEF && 
            ((byte)text[1]) == 0xBB &&
            ((byte)text[2]) == 0xBF) {
            return text.Substring(3);
        }
        //\xff (why it happens?)
        if (((byte)text[0]) == 255) {
            return text.Substring(1);
        }
        return text;
    }
    public const int SAFETY_COUNT = 100;
    static public IEnumerator Load<ID,R>(Loader loader, 
        string path, Dictionary<ID, R> map, System.Func<R,ID> idgetter) {
        return CSVIO.Read(loader, path, (System.IO.TextReader r) => {
            R rec = default(R);
            var csv = new CsvHelper.CsvReader(r);
            csv.Read();
            csv.ReadHeader();
            while (csv.Read()) {
                rec = csv.GetRecord<R>();
                map[idgetter(rec)] = rec;
            }
		});
	}

    public abstract class Loader : MonoBehaviour {
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
        public System.Exception Error {
            get; set;
        }
        public abstract IEnumerator Load(Loader loader, string basepath);
        public abstract string[] CSVNames();

        void Awake() {
            if (string.IsNullOrEmpty(BasePath)) {
                BasePath = Application.streamingAssetsPath + "/CSV/Data/";
            }
            Game.Main.FiberMgr.Start(StartLoad);
        }
        public IEnumerator StartLoad() {
            //TODO: update streaming asset
            yield return Load(this, BasePath);
            if (Error != null) {
                yield return Error;
            }
            subject_.OnNext(new Event{ Type = EventType.Inititalized });
        }
    }    
}
