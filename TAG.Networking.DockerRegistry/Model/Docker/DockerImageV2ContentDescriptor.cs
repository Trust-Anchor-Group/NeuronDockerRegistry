using System;
using System.Collections.Generic;

namespace TAG.Networking.DockerRegistry.Model.Docker
{
	internal class DockerImageV2ContentDescriptor : IDockerImageV2ContentDescriptor
	{
		public static DockerImageV2ContentDescriptor Parse(Dictionary<string, object> Json)
		{
			DockerImageV2ContentDescriptor Descriptor = new DockerImageV2ContentDescriptor();

			// TODO: check which media types OCI Content Descriptor supports.
			if (!(Json.TryGetValue("mediaType", out object MediaTypeObj) && MediaTypeObj is string JsonMediaType))
				throw new Exception("Invalid media type.");
			Descriptor.MediaType = JsonMediaType;

			if (!(Json.TryGetValue("size", out object SizeObj) && SizeObj is int JsonSize))
				throw new Exception("Invalid size.");
			Descriptor.Size = JsonSize;

			if (!(Json.TryGetValue("digest", out object DigestObj) && DigestObj is string JsonDigestString))
				throw new Exception("Invalid digest.");
			if (!HashDigest.TryParseDigest(JsonDigestString, out HashDigest JsonDigest))
				throw new Exception("Invalid digest.");
			Descriptor.Digest = JsonDigest;

			return Descriptor;
		}

		public string MediaType { get; set; }
		public int Size { get; set; }
		public HashDigest Digest { get; set; }
	}
}
