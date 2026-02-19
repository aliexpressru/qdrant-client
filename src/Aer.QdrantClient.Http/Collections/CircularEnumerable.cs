using System.Collections;

namespace Aer.QdrantClient.Http.Collections;

/// <summary>
/// Provides an enumerable collection that cycles through its elements in a circular manner, allowing repeated access to
/// items in sequence. Essentially it is used to ease round robin distribution.
/// </summary>
/// <remarks>CircularEnumerable enables iteration over a fixed set of items, returning elements in order and
/// wrapping around to the beginning when the end is reached. The collection must be initialized with at least one item.
/// Circle detection can be enabled to prevent infinite loops by marking a starting point and throwing an exception if
/// the sequence cycles back to it. This type is not thread-safe.</remarks>
/// <typeparam name="T">The type of elements contained in the collection.</typeparam>
internal sealed class CircularEnumerable<T> : IEnumerable<T>
{
    private sealed class CircleDetector(CircularEnumerable<T> enumerable,
        int circleStartElementPointer,
        bool isSkipInitialCircleStart) : IDisposable
    {
        private bool _isDisposed;

        public bool DetectCircle()
        {
            if (_isDisposed)
            {
                return false;
            }

            if (enumerable._currentItemPointer != circleStartElementPointer)
            {
                return false;
            }

            // means we encountered the initial of a new circle start

            if (!isSkipInitialCircleStart)
            {
                return true;
            }

            isSkipInitialCircleStart = false;
            return false;
        }

        public void Dispose() => _isDisposed = true;
    }

    private readonly IReadOnlyList<T> _items;
    private int _currentItemPointer;

    private CircleDetector _circleDetector;

    public int Count => _items.Count;

    public CircularEnumerable(IEnumerable<T> items)
    {
        // If items is already a read-only collection - just store a reference
        // If it is not - copy the content

        if (items is IReadOnlyList<T> readOnlyItems)
        {
            _items = readOnlyItems;
        }
        else
        {
            _items = [.. items];
        }

        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Can't initialize collection with zero items.");
        }

        _currentItemPointer = -1;
    }

    public T GetNext()
    {
        // store previous pointer to restore it if the circle is detected
        var previousCurrentItemPointer = _currentItemPointer;

        _currentItemPointer++;

        if (_currentItemPointer >= _items.Count)
        {
            _currentItemPointer = 0;
        }

        bool isCircleDetected = _circleDetector?.DetectCircle() ?? false;

        if (isCircleDetected)
        {
            _currentItemPointer = previousCurrentItemPointer;
            throw new InvalidOperationException("Circle detected");
        }

        return _items[_currentItemPointer];
    }

    /// <summary>
    /// Marks the previously returned element as the start of the circle
    /// for detecting whether we had moved full circle around the collection.
    /// If we encounter this element again - <see cref="InvalidOperationException"/> gets thrown.
    /// </summary>
    public IDisposable StartCircleDetection()
    {
        _circleDetector = new CircleDetector(
            this,
            _currentItemPointer == -1
                ? 0
                : _currentItemPointer,
            // when starting detecting circle right at the start of enumeration - first occurrence of
            // the circle start index should be skipped
            isSkipInitialCircleStart: _currentItemPointer == -1);

        return _circleDetector;
    }

    public void Reset() => _currentItemPointer = 0;

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();
}
