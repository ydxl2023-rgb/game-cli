using System;
using System.Collections.Generic;
using System.IO;
using System.Buffers.Binary;
using System.Text;

// Research adapter, not original Luban runtime. Uses the recovered constructors unchanged.
namespace Luban
{
    public sealed class SerializationException : Exception { }
    public abstract class BeanBase { public abstract int GetTypeId(); }
    public static class StringUtil
    {
        public static string CollectionToString<T>(IEnumerable<T> values) => string.Join(",", values);
        public static string CollectionToString<K, V>(IEnumerable<KeyValuePair<K, V>> values) => string.Join(",", values);
    }
    public sealed class ByteBuf
    {
        private readonly byte[] data;
        public int Position { get; private set; }
        public int Size => data.Length - Position;
        public ByteBuf(byte[] data) { this.data = data; }
        private byte Byte() => Position < data.Length ? data[Position++] : throw new EndOfStreamException();
        public int ReadInt()
        {
            int first = Byte();
            int count = first < 128 ? 0 : first < 192 ? 1 : first < 224 ? 2 : first < 240 ? 3 : 4;
            uint result = (uint)(first & (count == 4 ? 0 : (127 >> count)));
            for (int i = 0; i < count; i++) result = (result << 8) | Byte();
            return unchecked((int)result);
        }
        public long ReadLong()
        {
            int first = Byte();
            int count = 0;
            for (int mask = 128; mask > 0 && (first & mask) != 0; mask >>= 1) count++;
            ulong result = (ulong)(first & (count == 8 ? 0 : (127 >> count)));
            for (int i = 0; i < count; i++) result = (result << 8) | Byte();
            return unchecked((long)result);
        }
        public int ReadSize()
        {
            int size = ReadInt();
            if (size < 0 || size > Size) throw new InvalidDataException($"Invalid size {size} at {Position}");
            return size;
        }
        public bool ReadBool()
        {
            byte value = Byte();
            if (value > 1) throw new InvalidDataException("Invalid boolean");
            return value != 0;
        }
        public float ReadFloat()
        {
            float value = BinaryPrimitives.ReadSingleLittleEndian(data.AsSpan(Position, 4));
            Position += 4;
            return value;
        }
        public string ReadString()
        {
            int size = ReadSize();
            string value = new UTF8Encoding(false, true).GetString(data, Position, size);
            Position += size;
            return value;
        }
    }
}
// These constructors only support unused generated conversion operators; no Unity execution.
namespace UnityEngine
{
    public record struct Vector2(float x, float y) { public static Vector2 zero => default; }
    public record struct Vector3(float x, float y, float z) { public static Vector3 zero => default; }
}
