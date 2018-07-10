using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class CSVSourceFactory : DataLoader.IDataSourceFactory {
    public class CSVSource : CsvHelper.CsvReader, DataLoader.IDataSource {
        public CSVSource(TextReader r) : base(r) {}
    }

    public DataLoader.IDataSource Source { get; private set; }

    public IEnumerator Load<R>(DataLoader.IResultReceiver loader, string path) 
        where R : Google.Protobuf.IMessage<R> {
        //TODO: update csv if necessary (with assetbundle or something)
        CSVSource s;
        try {
        #if UNITY_ANDROID && !UNITY_EDITOR
            WWW www = new WWW(path);
            yield return www;
            if (!string.IsNullOrEmpty(www.error)) {
                throw new System.Exception(www.error);
            }
            s = new CSVSource(new StringReader(www.text));
        #else
            s = new CSVSource(new StreamReader(TrimBOM(path)));
        #endif        
        } catch (System.Exception e) {
            loader.Error = e;
            yield break;
        }
        s.Read();
        s.ReadHeader();
        Source = s;
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
}
