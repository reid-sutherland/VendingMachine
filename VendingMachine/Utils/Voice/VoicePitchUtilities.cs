using VoiceChat;
using VoiceChat.Codec;
using VoiceChat.Networking;

namespace VendingMachine.Utils.Voice;

public class VoicePitchUtilities
{
    private readonly float[] ReceivedBuffer = new float[VoiceChatSettings.BufferLength];
    private readonly byte[] EncodedBuffer = new byte[VoiceChatSettings.MaxEncodedSize];

    private readonly OpusDecoder Decoder = OpusDecoderPool.Shared.Get();
    private readonly OpusEncoder Encoder = OpusEncoderPool.Shared.Get();

    private readonly PitchShifter PitchShifter = PitchShifterPool.Shared.Get();

    // TODO: Make this configurable or something
    private float PitchValue { get; set; } = 1.5f;

    internal VoiceMessage SetVoicePitch(VoiceMessage msg)
    {
        int decodedData = Decoder.Decode(msg.Data, msg.DataLength, ReceivedBuffer);

        PitchShifter.PitchShift(PitchValue, decodedData, VoiceChatSettings.SampleRate, ReceivedBuffer);

        int length = Encoder.Encode(ReceivedBuffer, EncodedBuffer);

        msg.Data = EncodedBuffer;
        msg.DataLength = length;

        return msg;
    }
}