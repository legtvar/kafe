using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;


namespace Kafe.Media.Services;

public interface IPigeonsTestQueue
{
    ValueTask EnqueueAsync(PigeonsTestRequest request);
    ValueTask<PigeonsTestRequest> DequeueAsync(CancellationToken ct);
}

public sealed class PigeonsTestQueue : IPigeonsTestQueue
{
    private readonly Channel<PigeonsTestRequest> _queue;
    private const int queueMaxCapacity = 300;

    public PigeonsTestQueue()
    {
        BoundedChannelOptions options = new(queueMaxCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        };
        _queue = Channel.CreateBounded<PigeonsTestRequest>(options);
    }

    public async ValueTask EnqueueAsync(PigeonsTestRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _queue.Writer.WriteAsync(request);
    }

    public async ValueTask<PigeonsTestRequest> DequeueAsync(CancellationToken ct)
    {
        PigeonsTestRequest? request = await _queue.Reader.ReadAsync(ct);
        return request;
    }
}