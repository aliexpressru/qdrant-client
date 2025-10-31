using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Tests.Model;

internal class TestPayload
{
    public string Text { get; set; }

    public int? Integer { get; set; }

    public double? FloatingPointNumber { get; set; }

    public DateTimeOffset? DateTimeValue { get; set; }

    public bool AllPropertiesNotNull() =>
        !string.IsNullOrEmpty(Text)
        && Integer.HasValue
        && FloatingPointNumber.HasValue
        && DateTimeValue.HasValue;

    #region Operators

    public static implicit operator TestPayload(string value)
    {
        return new TestPayload()
        {
            Text = value
        };
    }

    public static implicit operator TestPayload(int value)
    {
        return new TestPayload()
        {
            Integer = value
        };
    }

    public static implicit operator TestPayload(double value)
    {
        return new TestPayload()
        {
            FloatingPointNumber = value
        };
    }

    public static implicit operator TestPayload(DateTime value)
    {
        return new TestPayload()
        {
            DateTimeValue = value
        };
    }

    #endregion
}
