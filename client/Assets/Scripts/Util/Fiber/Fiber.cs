using System;
using System.Collections;

namespace Engine {
public interface IFiber {
    IEnumerator RunAsFiber();
}
}
