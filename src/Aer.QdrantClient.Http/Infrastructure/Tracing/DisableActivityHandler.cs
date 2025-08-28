using System.Diagnostics;

namespace Aer.QdrantClient.Http.Infrastructure.Tracing;

/// <summary>
/// The http client handler that disables activity propagation.
/// </summary>
internal sealed class DisableActivityHandler : DelegatingHandler
{
	public DisableActivityHandler(HttpMessageHandler innerHandler) : base(innerHandler)
	{ }

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		Activity.Current = null;
		
		ConditionalPropagator.IgnoreRequest.Value = true;
		return await base.SendAsync(request, cancellationToken);
	}
}

/// See hacky gist from issue https://github.com/dotnet/runtime/issues/85883 for details : https://gist.github.com/MihaZupan/835591bb22270b1aa7feeeece721520d
internal sealed class ConditionalPropagator : DistributedContextPropagator
{
	public static readonly AsyncLocal<bool> IgnoreRequest = new();

	private readonly DistributedContextPropagator _originalPropagator = Current;

	public override IReadOnlyCollection<string> Fields => _originalPropagator.Fields;

	public override void Inject(Activity activity, object carrier, PropagatorSetterCallback setter)
	{
		if (IgnoreRequest.Value)
		{
			return;
		}

		_originalPropagator.Inject(activity, carrier, setter);
	}

	public override void ExtractTraceIdAndState(
		object carrier,
		PropagatorGetterCallback getter,
		out string traceId,
		out string traceState)
		=>
			_originalPropagator.ExtractTraceIdAndState(carrier, getter, out traceId, out traceState);

	public override IEnumerable<KeyValuePair<string, string>> ExtractBaggage(
		object carrier,
		PropagatorGetterCallback getter)
		=>
			_originalPropagator.ExtractBaggage(carrier, getter);
}
