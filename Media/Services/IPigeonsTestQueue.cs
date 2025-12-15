using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;


namespace Kafe.Media.Services;

public interface IPigeonsTestQueue
{
    ValueTask EnqueueAsync(Hrib ShardId);
    ValueTask<Hrib> DequeueAsync(CancellationToken ct);
}

public sealed class PigeonsTestQueue : IPigeonsTestQueue
{
    private readonly Channel<Hrib> _queue;
    private const int queueMaxCapacity = 300;

    public PigeonsTestQueue()
    {
        BoundedChannelOptions options = new(queueMaxCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        };
        _queue = Channel.CreateBounded<Hrib>(options);
    }

    public async ValueTask EnqueueAsync(Hrib ShardId)
    {
        ArgumentNullException.ThrowIfNull(ShardId);
        await _queue.Writer.WriteAsync(ShardId);
    }

    public async ValueTask<Hrib> DequeueAsync(CancellationToken ct)
    {
        Hrib? ShardId = await _queue.Reader.ReadAsync(ct);
        return ShardId;
    }
}