namespace Aer.QdrantClient.Http.Helpers;

internal static class QueueExtensions
{
    public static void EnqueueMany<T>(this Queue<T> target, IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            target.Enqueue(item);
        }
    }

    /// <summary>
    /// Dequeues at most <paramref name="count"/> items from the queue.
    /// If queue does not have specified number of items dequeues one item.
    /// </summary>
    /// <param name="target">The source queue.</param>
    /// <param name="count">The number of elements to try to dequeue.</param>
    /// <typeparam name="T">The type of the element in the queue.</typeparam>
    public static List<T> DequeueAtMost<T>(this Queue<T> target, int count)
    {
        if (target.Count == 0)
        {
            throw new InvalidOperationException($"Can't dequeue at most {count} items from queue. The queue is empty.");
        }

        var result = new List<T>(count);

        if (target.Count < count)
        {
            result.Add(target.Dequeue());

            return result;
        }

        for (var i = 0; i < count; i++)
        {
            result.Add(target.Dequeue());
        }

        return result;
    }
}
