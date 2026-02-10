using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Kafe.Mate;

public class PigeonsTestQueue
{
    public const int QueueMaxCapacity = 300;

    private readonly Channel<PigeonsTestRequest> queue;

    public PigeonsTestQueue()
    {
        var options = new BoundedChannelOptions(QueueMaxCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        };
        queue = Channel.CreateBounded<PigeonsTestRequest>(options);
    }

    public async ValueTask EnqueueAsync(Uri shardUri, string homeworkType)
    {
        ArgumentNullException.ThrowIfNull(shardUri);
        await queue.Writer.WriteAsync(new PigeonsTestRequest(shardUri, homeworkType));
    }

    public PigeonsTestRequest? Dequeue()
    {
        if (queue.Reader.TryRead(out var request))
        {
            return request;
        }

        return null;
    }

    public ValueTask<bool> Wait(CancellationToken ct = default)
    {
        return queue.Reader.WaitToReadAsync(ct);
    }
}
