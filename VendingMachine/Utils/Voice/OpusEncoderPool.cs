using System.Collections.Concurrent;
using Exiled.API.Features.Pools;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;

namespace VendingMachine.Utils.Voice;

public class OpusEncoderPool : IPool<OpusEncoder>
{
    public static readonly OpusEncoderPool Shared = new();

    private readonly ConcurrentQueue<OpusEncoder> _pool = new();

    public OpusEncoder Get() => _pool.TryDequeue(out var encoder) ? encoder : new OpusEncoder(OpusApplicationType.Voip);

    public void Return(OpusEncoder encoder) => _pool.Enqueue(encoder);
}