using System.Collections;

namespace Aer.QdrantClient.Http.Collections;

internal class CircularEnumerable<T> : IEnumerable<T>
{
    private readonly List<T> _items = new();
    private int _currentItemPointer;

    public int Count => _items.Count;

    public CircularEnumerable(IEnumerable<T> items)
    {
        _items.AddRange(items);

        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Can't initialize collection with zero items.");
        }

        _currentItemPointer = 0;
    }

    public T GetNext()
    {
        var itemToReturn = _items[_currentItemPointer];

        _currentItemPointer++;

        if (_currentItemPointer >= _items.Count)
        {
            _currentItemPointer = 0;
        }

        return itemToReturn;
    }

    public void StepBack()
    {
        _currentItemPointer--;

        if(_currentItemPointer < 0)
        {
            _currentItemPointer = _items.Count - 1;
        }
    }

    public bool ContainsElement(T elementToCheck, IEqualityComparer<T> comparer = null)
    {
        return _items.Contains(elementToCheck, comparer);
    }

    public void Reset()
    {
        _currentItemPointer = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable) _items).GetEnumerator();
    }
}
