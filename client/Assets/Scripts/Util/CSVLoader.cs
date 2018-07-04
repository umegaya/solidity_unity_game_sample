using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class CSVIO {
    static public IEnumerator Read(string path, Action<TextReader> cb) {
        #if UNITY_ANDROID && !UNITY_EDITOR
        WWW www = new WWW(path);
        yield return www;
        if (!string.IsNullOrEmpty(www.error)) {
            cb(null);
            yield break;
        }
        cb(new StringReader(www.text));
        #else
        try {
            cb(new StreamReader(TrimBOM(path)));
        } catch (FileNotFoundException) {
            cb(null);
        }
        #endif        
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
    static public IEnumerator Load<ID,R>(string path, Dictionary<ID, R> map, System.Func<R,ID> idgetter) {
		return CSVIO.Read(path, (System.IO.TextReader r) => {
			var csv = new CsvHelper.CsvReader(r);
			while (true) {
				var rec = csv.GetRecord<R>();
				if (rec == null) {
					break;
				}
				map[idgetter(rec)] = rec;
			}
		});
	}

    public abstract class Loader : MonoBehaviour {
        public delegate void OnLoadFinish(Loader loaded);
        public OnLoadFinish OnFinish;
        public string BasePath = Application.streamingAssetsPath + "/CSV/Data/";
        public abstract IEnumerator Load(string basepath);
        public abstract string[] CSVNames();

        void Awake() {
            StartCoroutine(StartLoad());
        }
        IEnumerator StartLoad() {
            //TODO: update streaming asset
            yield return StartCoroutine(Load(BasePath));
            OnFinish(this);
        }
    }    
}
