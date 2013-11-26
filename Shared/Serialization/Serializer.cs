using System;
using System.Collections.Generic;
using System.IO;

namespace Shared.Serialization
{

    public static class BinarySerializerExtensions
    {
        public static void WriteList<T>(this BinaryWriter writer, List<T> list) where T : IBinarySerializable
        {
            if (list != null)
            {
                writer.Write(1);
                writer.Write(list.Count);
                foreach (T item in list)
                {
                    item.Write(writer);
                }
            }
            else
            {
                writer.Write(0);
            }
        }

        public static void WriteList(this BinaryWriter writer, List<string> list)
        {
            if (list != null)
            {
                writer.Write(1);
                writer.Write(list.Count);
                foreach (string item in list)
                {
                    writer.Write(item);
                }
            }
            else
            {
                writer.Write(0);
            }
        }

        public static void WriteList(this BinaryWriter writer, List<int> list)
        {
            if (list != null)
            {
                writer.Write(1);
                writer.Write(list.Count);
                foreach (int item in list)
                {
                    writer.Write(item);
                }
            }
            else
            {
                writer.Write(0);
            }
        }

        public static void Write<T>(this BinaryWriter writer, T value) where T : IBinarySerializable
        {
            if (value != null)
            {
                writer.Write(true);
                value.Write(writer);
            }
            else
            {
                writer.Write(false);
            }
        }

        public static void Write(this BinaryWriter writer, DateTime value)
        {
            writer.Write(value.Ticks);
        }

        public static void WriteString(this BinaryWriter writer, string value)
        {
            writer.Write(value ?? string.Empty);
        }

        public static T ReadGeneric<T>(this BinaryReader reader) where T : IBinarySerializable, new()
        {
            if (reader.ReadBoolean())
            {
                T result = new T();
                result.Read(reader);
                return result;
            }
            return default(T);
        }

        public static List<string> ReadList(this BinaryReader reader)
        {
            int hasInstance = reader.ReadInt32();
            if (hasInstance == 1)
            {
                var list = new List<string>();
                int count = reader.ReadInt32(); //9
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        list.Add(reader.ReadString());
                    }
                }
                return list;
            }

            return null;
        }

        public static List<int> ReadIntList(this BinaryReader reader)
        {
            var hasInstance = reader.ReadInt32();
            if (hasInstance == 1)
            {
                var list = new List<int>();
                int count = reader.ReadInt32(); //9
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        list.Add(reader.ReadInt32());
                    }
                }
                return list;
            }
            return null;
        }

        public static List<T> ReadList<T>(this BinaryReader reader) where T : IBinarySerializable, new()
        {
            int hasInstance = reader.ReadInt32();
            if (hasInstance == 1)
            {
                var list = new List<T>();
                int count = reader.ReadInt32();
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        T item = new T();
                        item.Read(reader);
                        list.Add(item);
                    }
                }
                return list;
            }

            return null;
        }

        public static DateTime ReadDateTime(this BinaryReader reader)
        {
            var int64 = reader.ReadInt64();
            return new DateTime(int64);
        }

        public static void WriteDictionary<T, U>(this BinaryWriter writer, Dictionary<T, U> dictionary)
            where T : IBinarySerializable
            where U : IBinarySerializable
        {
            if (dictionary != null)
            {
                writer.Write(1);
                writer.Write(dictionary.Count);
                foreach (KeyValuePair<T, U> item in dictionary)
                {
                    item.Key.Write(writer);
                    item.Value.Write(writer);
                }
            }
            else
            {
                writer.Write(0);
            }
        }

        public static Dictionary<T, U> ReadDictionary<T, U>(this BinaryReader reader)
            where T : IBinarySerializable, new()
            where U : IBinarySerializable, new()
        {
            int hasInstance = reader.ReadInt32();
            if (hasInstance == 1)
            {
                var dictionary = new Dictionary<T, U>();
                int count = reader.ReadInt32();
                if (count > 0)
                {
                    for (var i = 0; i < count; i++)
                    {
                        var key = new T();
                        key.Read(reader);
                        var value = new U();
                        value.Read(reader);
                        dictionary.Add(key, value);
                    }
                }
                return dictionary;
            }

            return null;
        }

        public static void WriteDictionary(this BinaryWriter writer, Dictionary<int, int> dictionary)
        {
            if (dictionary != null)
            {
                writer.Write(1);
                writer.Write(dictionary.Count);
                foreach (var pair in dictionary)
                {
                    //ToDo: For all of these dictionaries, do a null check on both Key and Value; Find a way to bail gracefully if they are null.
                    writer.Write(pair.Key);
                    writer.Write(pair.Value);
                }
            }
            else
            {
                writer.Write(0);
            }
        }

        public static Dictionary<int, int> ReadIntDictionary(this BinaryReader reader)
        {
            int hasInstance = reader.ReadInt32();
            if (hasInstance == 1)
            {
                var dictionary = new Dictionary<int, int>();
                var count = reader.ReadInt32();
                if (count > 0)
                {
                    for (var i = 0; i < count; i++)
                    {
                        dictionary.Add(reader.ReadInt32(), reader.ReadInt32());
                    }
                }
                return dictionary;
            }

            return null;
        }

        public static void WriteDictionary<U>(this BinaryWriter writer, Dictionary<string, U> dictionary)
            where U : IBinarySerializable
        {
            if (dictionary != null)
            {
                writer.Write(1);
                writer.Write(dictionary.Count);
                foreach (var pair in dictionary)
                {
                    writer.Write(pair.Key);
                    pair.Value.Write(writer);
                }
            }
            else
            {
                writer.Write(0);
            }
        }

        public static Dictionary<string, T> ReadPlayerDictionary<T>(this BinaryReader reader)
            where T : IBinarySerializable, new()
        {
            int hasInstance = reader.ReadInt32();
            if (hasInstance == 1)
            {
                var dictionary = new Dictionary<string, T>();
                var count = reader.ReadInt32();
                if (count > 0)
                {
                    for (var i = 0; i < count; i++)
                    {
                        var key = reader.ReadString();

                        var value = new T();
                        value.Read(reader);
                        dictionary.Add(key, value);
                    }
                }
                return dictionary;
            }

            return null;
        }
    }
}

