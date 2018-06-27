using Google.Protobuf;

public static class ProtobufExtension {
    public static byte[] Encode<T>(this T msg) where T : IMessage {
        int blen = msg.CalculateSize();
        byte[] bs = new byte[blen];
        msg.WriteTo(new System.IO.MemoryStream(bs));
        return bs;
    }
}
