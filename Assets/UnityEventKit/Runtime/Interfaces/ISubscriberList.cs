using System;
using UnityEngine;

namespace UnityEventKit
{
	internal interface ISubscriberList
    {
	    void InvokeBoxed(object evnt);
	    void Add(Delegate dlg);
	    void Remove(Delegate dlg);
	    int Count { get; }
    }
}
