#if ENABLE_UNET
using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace UnityEngine.Networking
{
    // unrolled for your pleasure.
    [Serializable]
    public struct NetworkHash128
    {
        // struct cannot have embedded arrays..
        public byte i0;
        public byte i1;
        public byte i2;
        public byte i3;
        public byte i4;
        public byte i5;
        public byte i6;
        public byte i7;
        public byte i8;
        public byte i9;
        public byte i10;
        public byte i11;
        public byte i12;
        public byte i13;
        public byte i14;
        public byte i15;

        public void Reset()
        {
            i0 = 0;
            i1 = 0;
            i2 = 0;
            i3 = 0;
            i4 = 0;
            i5 = 0;
            i6 = 0;
            i7 = 0;
            i8 = 0;
            i9 = 0;
            i10 = 0;
            i11 = 0;
            i12 = 0;
            i13 = 0;
            i14 = 0;
            i15 = 0;
        }

        public bool IsValid()
        {
            return (i0 | i1 | i2 | i3 | i4 | i5 | i6 | i7 | i8 | i9 | i10 | i11 | i12 | i13 | i14 | i15) != 0;
        }

        public static byte[] GetStringToBytes(string text)
        {
            return SoapHexBinary.Parse(text).Value;
        }

        public static string GetBytesToString(byte[] value)
        {
            return new SoapHexBinary(value).ToString();
        }

        public static NetworkHash128 Parse(string text)
        {
            NetworkHash128 hash;

            // vis2k: shorter version for padding 0s
            text = text.PadLeft(32, '0');

            // vis2k: shorter version for hex string to bytes
            byte[] bytes = GetStringToBytes(text);
            hash.i0 = bytes[0];
            hash.i1 = bytes[1];
            hash.i2 = bytes[2];
            hash.i3 = bytes[3];
            hash.i4 = bytes[4];
            hash.i5 = bytes[5];
            hash.i6 = bytes[6];
            hash.i7 = bytes[7];
            hash.i8 = bytes[8];
            hash.i9 = bytes[9];
            hash.i10 = bytes[10];
            hash.i11 = bytes[11];
            hash.i12 = bytes[12];
            hash.i13 = bytes[13];
            hash.i14 = bytes[14];
            hash.i15 = bytes[15];

            return hash;
        }

        public override string ToString()
        {
            // vis2k: shorter version. and to lowercase for consistency with AssetDatabase.AssetPathToGUID
            return GetBytesToString(new byte[]{i0, i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14, i15}).ToLower();
        }
    }
}
#endif //ENABLE_UNET
