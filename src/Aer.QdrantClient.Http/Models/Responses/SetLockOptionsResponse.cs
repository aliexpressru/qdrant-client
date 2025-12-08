using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the response of the Qdrant set lock options operation.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class SetLockOptionsResponse : QdrantResponseBase<SetLockOptionsResponse.PreviousSetLockOptionsState>
{
    /// <summary>
    /// Represents a previous set lock options state.
    /// </summary>
    public sealed class PreviousSetLockOptionsState
    {
        /// <summary>
        /// The write operations lock reason.
        /// </summary>
        public string ErrorMessage { init; get; }

        /// <summary>
        /// If set to <c>true</c> write operations are locked, otherwise - write operations are enabled.
        /// </summary>
        public bool Write { init; get; }
    }
}
