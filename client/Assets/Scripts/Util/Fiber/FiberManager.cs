using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;   

//retryable coroutine execution
namespace Engine {
using FiberMap = Dictionary<System.Func<IEnumerator>, FiberManager.Context>;
using Fiber = System.Func<IEnumerator>;
public partial class FiberManager {
    //execution contexts
    internal class Context {
        public List<IEnumerator> stack_; 
        public IYieldable yield_op_;
        public System.Exception error_;
    }
    FiberMap fibers_ = new FiberMap();
    FiberMap pendings_ = new FiberMap();
    List<Fiber> finishes_ = new List<Fiber>();

    //logger
    public delegate void LoggerDelegate(string txt);
    public LoggerDelegate Logger;

    public FiberManager() {}

    public void Start(Fiber f) {
        Context ctx;
        if (fibers_.TryGetValue(f, out ctx)) {
            return;
        }
        var it = f();
        fibers_[f] = new Context {
            stack_ = new List<IEnumerator> { it },
        };
    }

    public void Stop(Fiber f) {
        Context ctx;
        if (fibers_.TryGetValue(f, out ctx)) {
            fibers_.Remove(f);
        }
    }

    public void Poll() {
        foreach (var kv in fibers_) {
            var c = kv.Value;
            if (c.yield_op_ != null) {
                if (!c.yield_op_.YieldDone()) {
                    continue; //yield operation not finished
                }
            }
            var it = c.stack_.Last();
            if (it.MoveNext()) {
                object cur = it.Current;
                if (cur is IYieldable) {
                    c.yield_op_ = (cur as IYieldable);
                } else if (cur is IEnumerator) {
                    c.stack_.Add(cur as IEnumerator);
                } else if (cur is System.Exception) {
                    c.error_ = (cur as System.Exception);
                    finishes_.Add(kv.Key);
                    Logger("fiber raise error:" + c.error_.Message);
                } else if (cur != null) {
                    c.yield_op_ = TryYieldable(cur);
                    if (c.yield_op_ == null) { 
                        System.Type type = cur.GetType();
                        Logger(cur + " is not supported.");
                    }
                }
            } else {
                c.stack_.RemoveAt(c.stack_.Count - 1);
                if (c.stack_.Count <= 0) {
                    finishes_.Add(kv.Key);
                }
            }
        }
        if (finishes_.Count > 0) {
            foreach (var f in finishes_) {
                Context c;
                if (fibers_.TryGetValue(f, out c)) {
                    fibers_.Remove(f);
                    if (c.error_ != null) {
                        //current coroutine restarts
                        pendings_[f] = c;
                        Raise(c.error_, f);
                    }
                }
            }
            finishes_.Clear();
        }
    }
}
}
