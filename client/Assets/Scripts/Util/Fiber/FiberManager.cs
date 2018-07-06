using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;   

//retryable coroutine execution
namespace Engine {
public partial class FiberManager {
    class Context {
        public List<IEnumerator> stack_; 
        public IYieldable yield_op_;
        public System.Exception error_;
    }
    Dictionary<IFiber, Context> fibers_ = new Dictionary<IFiber, Context>();
    List<IFiber> finishes_ = new List<IFiber>();
    Dictionary<IFiber, Context> pendings_ = new Dictionary<IFiber, Context>();

    public delegate void LoggerDelegate(string txt);
    public LoggerDelegate Logger;

    public FiberManager() {}

    public void Start(IFiber f) {
        Context ctx;
        if (fibers_.TryGetValue(f, out ctx)) {
            return;
        }
        if (pendings_.TryGetValue(f, out ctx)) {
            fibers_[f] = ctx;
            return;
        }
        var it = f.RunAsFiber();
        fibers_[f] = new Context {
            stack_ = new List<IEnumerator> { it },
        };
    }

    public void Stop(IFiber f) {
        Context ctx;
        if (fibers_.TryGetValue(f, out ctx)) {
            fibers_.Remove(f);
        }
        if (pendings_.TryGetValue(f, out ctx)) {
            pendings_.Remove(f);
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
                        c.stack_.Last().Reset();
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
