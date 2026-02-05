using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Kafe.Mate;

public class PigeonsTestQueue
{
    public const int QueueMaxCapacity = 300;

    private readonly Channel<Hrib> queue;

    public PigeonsTestQueue()
    {
        var options = new BoundedChannelOptions(QueueMaxCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        };
        queue = Channel.CreateBounded<Hrib>(options);
    }

    public async ValueTask EnqueueAsync(Hrib shardId)
    {
        ArgumentNullException.ThrowIfNull(shardId);
        await queue.Writer.WriteAsync(shardId);
    }

    public Hrib? Dequeue()
    {
        if (queue.Reader.TryRead(out var hrib))
        {
            return hrib;
        }

        return null;
    }

    public ValueTask<bool> Wait(CancellationToken ct = default)
    {
        return queue.Reader.WaitToReadAsync(ct);
    }
}
