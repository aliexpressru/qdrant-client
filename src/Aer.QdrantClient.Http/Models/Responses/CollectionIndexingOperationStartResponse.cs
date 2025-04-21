using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// The response for compound collection indexes creation operation.
/// </summary>
public class CollectionIndexingOperationStartResponse : QdrantResponseBase<List<CollectionIndexingOperationStartResponse.IndexingOperationStartStatus>>
{
	/// <summary>
	/// The status of one index (HNSW or payload) creation operation.
	/// </summary>
	public class IndexingOperationStartStatus
	{ 
		/// <summary>
		/// If <c>true</c> this index operation is for payload index.
		/// If <c>false</c> this is an HNSW index operation.
		/// </summary>
		public bool IsPayloadIndexOperation { get; set; }
		
		/// <summary>
		/// If set to <c>true</c> this index was successfully started.
		/// If set to <c>false</c> operation start failed. See <see cref="ErrorMessage"/> for details.
		/// </summary>
		public bool IsSuccess { get; set; }
		
		/// <summary>
		/// An error that happened during indexing operation start.
		/// </summary>
		public string ErrorMessage { get; set; }
	}
}
