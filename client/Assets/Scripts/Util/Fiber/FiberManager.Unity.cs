using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;   

using UnityEngine;
using UniRx;

//retryable coroutine execution
namespace Engine {
using Fiber = System.Func<IEnumerator>;
public partial class FiberManager {
    public Subject<System.Tuple<System.Exception, Fiber>> error_stream_ = 
        new Subject<System.Tuple<System.Exception, Fiber>>();

    public void Raise(System.Exception e, Fiber f) {
        error_stream_.OnNext(System.Tuple.Create(e, f));
    }

    //Sleep
    public class Sleep : IYieldable {
        float end_at_;
       public Sleep(float duration) {
           end_at_ = Time.time + duration;
       }
       public bool YieldDone() {
           return Time.time > end_at_;
       }
    }

    //make yieldable
    //WWW
    class YieldableWWW : IYieldable {
        WWW www_;
        public YieldableWWW(WWW www) {
            www_ = www;
        }
        public bool YieldDone() {
            return www_.isDone;
        }
    }
    public static IYieldable Yieldable(WWW www) {
        return new YieldableWWW(www);
    }
    class YieldAsyncOperation : IYieldable {
        AsyncOperation op_;
        public YieldAsyncOperation(AsyncOperation op) {
            op_ = op;
        }
        public bool YieldDone() {
            return op_.isDone;
        }
    }
    public static IYieldable Yieldable(AsyncOperation op) {
        return new YieldAsyncOperation(op);
    }

    //object
    public static IYieldable TryYieldable(object o) {
        if (o is WWW) {
            return Yieldable(o as WWW);
        } else if (o is AsyncOperation) {
            return Yieldable(o as AsyncOperation);
        }
        return null;
    }
}

}