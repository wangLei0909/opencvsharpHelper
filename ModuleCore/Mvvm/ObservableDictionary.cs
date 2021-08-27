using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ModuleCore.Mvvm
{
    public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, INotifyCollectionChanged
    {
        public ObservableDictionary() : base()
        {
        }

        private int _index;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public new KeyCollection Keys
        {
            get { return base.Keys; }
        }

        public new ValueCollection Values
        {
            get { return base.Values; }
        }

        public new int Count
        {
            get { return base.Count; }
        }

        public new TValue this[TKey key]
        {
            get { return this.GetValue(key); }
            set { SetValue(key, value); }
        }

        public new void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                var pair = FindPair(key);
                base[key] = value;
                var newpair = FindPair(key);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newpair, pair, _index));
            }
            else
            {
                base.Add(key, value);
                var pair = FindPair(key);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, pair, _index));
            }
        }

        public new void Clear()
        {
            base.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public new bool Remove(TKey key)
        {
            var pair = FindPair(key);
            if (base.Remove(key))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, pair, _index));
                return true;
            }
            return false;
        }

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                CollectionChanged?.Invoke(this, e);
            }));
        }

        #region private方法

        private TValue GetValue(TKey key)
        {
            return ContainsKey(key) ? base[key] : default;
        }

        public void SetValue(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                var pair = FindPair(key);
                base[key] = value;
                var newpair = FindPair(key);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newpair, pair, _index));
            }
            else Add(key, value);
        }

        public KeyValuePair<TKey, TValue> FindPair(TKey key)
        {
            _index = 0;
            foreach (var item in this)
            {
                if (item.Key.Equals(key)) return item;
                _index++;
            }
            return default;
        }

        public int IndexOf(TKey key)
        {
            int index = 0;
            foreach (var item in this)
            {
                if (item.Key.Equals(key)) return index;
                index++;
            }
            return -1;
        }

        #endregion private方法
    }
}