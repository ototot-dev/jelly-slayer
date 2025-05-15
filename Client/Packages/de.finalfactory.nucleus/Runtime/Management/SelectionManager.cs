using System;
using System.Collections.Generic;
using System.Linq;
using FinalFactory.Tooling;

namespace FinalFactory.Management
{
    public class SelectionManager<T>
    {
        public delegate void SelectionChanged(IEnumerable<T> selectedItems);
        
        private readonly HashSet<T> _selectedItems = new();

        private readonly Action<T> _onItemSelected;
        private readonly Action<T> _onItemDeselected;
        
        public event SelectionChanged EventSelectionChanged;
        
        public SelectionManager()
        {
            
        }

        public SelectionManager(Action<T> onItemSelected, Action<T> onItemDeselected)
        {
            _onItemSelected = onItemSelected;
            _onItemDeselected = onItemDeselected;
        }

        public int Count => _selectedItems.Count;
        
        public List<T> SelectedItems
        {
            get => _selectedItems.ToList();
            set => SetSelection(value);
        }
        
        public T this[int index] => _selectedItems.ElementAt(index);
        
        public void AddSelection(T item)
        {
            if (item != null)
            {
                _selectedItems.Add(item);
                _onItemSelected?.Invoke(item);
                OnEventSelectionChanged(_selectedItems);
            }

        }
        
        public void RemoveSelection(T item)
        {
            _selectedItems.Remove(item);
            _onItemDeselected?.Invoke(item);
            OnEventSelectionChanged(_selectedItems);
        }
        
        public void SetSelection(T item)
        {
            T[] tmp = null;
            if (_onItemDeselected != null) tmp = _selectedItems.ToArray();

            _selectedItems.Clear();
            if (_onItemDeselected != null) tmp.For(_onItemDeselected);
            if (item != null)
            {
                _selectedItems.Add(item);
                _onItemSelected?.Invoke(item);
            }
            OnEventSelectionChanged(_selectedItems);
        }
        
        public void SetSelection(IEnumerable<T> items)
        {
            T[] tmp = null;
            if (_onItemDeselected != null) tmp = _selectedItems.ToArray();
            _selectedItems.Replace(items);
            if (_onItemDeselected != null) tmp.For(_onItemDeselected);
            if (_onItemSelected != null) _selectedItems.ForEach(_onItemSelected);
            OnEventSelectionChanged(_selectedItems);
        }
        
        public void AddSelection(IEnumerable<T> items)
        {
            foreach (var i in items)
            {
                _selectedItems.Add(i);
                _onItemSelected?.Invoke(i);
            }
            
            OnEventSelectionChanged(_selectedItems);
        }
        
        public void RemoveSelection(IEnumerable<T> items)
        {
            foreach (var i in items)
            {
                _selectedItems.Remove(i);
                _onItemDeselected?.Invoke(i);
            }
            OnEventSelectionChanged(_selectedItems);
        }
        
        public void ClearSelection()
        {
            T[] tmp = null;
            if (_onItemDeselected != null) tmp = _selectedItems.ToArray();
            _selectedItems.Clear();
            if (_onItemDeselected != null) tmp.For(_onItemDeselected);
        }

        protected virtual void OnEventSelectionChanged(IEnumerable<T> items)
        {
            EventSelectionChanged?.Invoke(items);
        }

        public bool Contains(T item) => _selectedItems.Contains(item);
    }
}