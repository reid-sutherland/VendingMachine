using System.Collections.Concurrent;
using Exiled.API.Features.Pools;
using VoiceChat.Codec;

namespace VendingMachine.Utils.Voice;

public class OpusDecoderPool : IPool<OpusDecoder>
{
    public static readonly OpusDecoderPool Shared = new();

    private readonly ConcurrentQueue<OpusDecoder> _pool = new();

    public OpusDecoder Get() => _pool.TryDequeue(out var decoder) ? decoder : new OpusDecoder();

    public void Return(OpusDecoder decoder) => _pool.Enqueue(decoder);
}