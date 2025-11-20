using System;
using System.Text;
using TAG.Networking.DockerRegistry.Model;
using Waher.Content.Json;
using Waher.Runtime.Inventory;
using Waher.Security;

namespace TAG.Networking.DockerRegistry
{
    public class HashDigestJsonEncoder : IJsonEncoder
    {
        public void Encode(object Object, int? Indent, StringBuilder Json)
        {
            HashDigest Digest = (HashDigest)Object;

            Json.Append("{\"HashFunction\":\"");
            Json.Append(Digest.HashFunction.ToString());
            Json.Append("\", \"Hash\":\"");
            Json.Append(Hashes.BinaryToString(Digest.Hash));
            Json.Append("\"}");
        }

        public Grade Supports(Type ObjectType)
        {
            return ObjectType == typeof(HashDigest) ? Grade.Excellent : Grade.NotAtAll;
        }
    }
}
