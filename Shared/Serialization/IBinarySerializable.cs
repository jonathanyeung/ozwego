using System.IO;

namespace Shared.Serialization
{
    public interface IBinarySerializable
    {
        void Write(BinaryWriter writer);
        void Read(BinaryReader reader);
    }
}
