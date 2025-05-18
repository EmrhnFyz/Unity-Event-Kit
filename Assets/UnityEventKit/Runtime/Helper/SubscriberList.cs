using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityEventKit
{
    internal sealed class SubscriberList<T> : ISubscriberList where T : struct, IEvent
    {
        private Action<T>[] _items = new Action<T>[4];
        private int _count;
        public int Count => _count;

        public void Add(Delegate dlg)
        {
            if (_count == _items.Length)
            {
                Array.Resize(ref _items, _items.Length * 2);
            }
            
            _items[_count++]  = (Action<T>)dlg;
        }

        public void Remove(Delegate dlg)
        {
            var action = (Action<T>)dlg;

            for (int i = 0; i < _count; i++)
            {
                if (_items[i] != action)
                {
                    continue;
                }
                
                _items[i] = _items[--_count];
                _items[_count] = null;
                break;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(in T evnt)
        {
            for (int i = 0; i < _count; i++)
            {
                _items[i]?.Invoke(evnt);
            }
        }

        void ISubscriberList.InvokeBoxed(object evnt) => Invoke((T)evnt);
    }
}
