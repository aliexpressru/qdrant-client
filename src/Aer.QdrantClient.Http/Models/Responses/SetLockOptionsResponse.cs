using Aer.QdrantClient.Http.Models.Responses.Base;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the response of the Qdrant set lock options operation.
/// </summary>
public sealed class SetLockOptionsResponse : QdrantResponseBase<SetLockOptionsResponse.PreviousSetLockOptionsState>
{
    /// <summary>
    /// Represents a previous set lock options state.
    /// </summary>
    public class PreviousSetLockOptionsState
    {
        /// <summary>
        /// The write oprations lock reason.
        /// </summary>
        public string ErrorMessage { set; get; }

        /// <summary>
        /// If set to <c>true</c> write operations are locked, otherwise - write oprations are enabled.
        /// </summary>
        public bool Write { set; get; }
    }
}
