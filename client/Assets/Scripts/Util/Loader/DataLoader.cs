using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class DataLoader {
    public interface IResultReceiver {
        Exception Error { get; set; }
    }
    public interface IDataSource {
        bool Read();
        R GetRecord<R>();
    }
    public interface IDataSourceFactory {
        IEnumerator Load<R>(IResultReceiver loader, string path) where R : Google.Protobuf.IMessage<R>;
        IDataSource Source { get; set; }
    }
    static public IEnumerator Load<ID,R,DSF>(IResultReceiver loader, 
        string path, Dictionary<ID, R> map, System.Func<R,ID> idgetter) 
        where DSF : IDataSourceFactory, new()
        where R : Google.Protobuf.IMessage<R> {
        DSF f = new DSF();
        yield return f.Load<R>(loader, path);
        if (loader.Error != null) {
            yield break;
        }
        var s = f.Source;
        f.Source = null; //source should gc'ed after data copied into map.
        R rec = default(R);
        while (s.Read()) {
            rec = s.GetRecord<R>();
            map[idgetter(rec)] = rec;
        }
	}
}