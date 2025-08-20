using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents peer information retrieval result.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class GetPeerResponse : QdrantResponseBase<GetPeerResponse.PeerInfo>
{
    /// <summary>
    /// The peer information.
    /// </summary>
    public class PeerInfo
    {
        /// <summary>
        /// The found peer identifier.
        /// </summary>
        public ulong PeerId { init; get; }

        /// <summary>
        /// The found peer uri.
        /// </summary>
        public string PeerUri { init; get; }

        /// <summary>
        /// The other cluster peer identifiers.
        /// </summary>
        public List<ulong> OtherPeerIds { init; get; }

        /// <summary>
        /// The all peer uris per peer ids.
        /// </summary>
        public Dictionary<ulong, string> PeerUriPerPeerIds { init; get; }

        /// <summary>
        /// Deconstruct <see cref="PeerInfo"/> to a value tuple.
        /// </summary>
        /// <param name="peerId">The peer id.</param>
        /// <param name="peerUri">The peer uri.</param>
        /// <param name="otherPeerIds">Other peer ids.</param>
        /// <param name="peerUriPerPeerIds">The all peer uris per peer ids.</param>
        public void Deconstruct(
            out ulong peerId,
            out string peerUri,
            out List<ulong> otherPeerIds,
            out Dictionary<ulong, string> peerUriPerPeerIds)
        {
            peerId = PeerId;
            peerUri = PeerUri;
            otherPeerIds = OtherPeerIds;
            peerUriPerPeerIds = PeerUriPerPeerIds;
        }
    }
}
