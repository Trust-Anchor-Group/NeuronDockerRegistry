using System;
using System.Collections.Generic;

namespace TAG.Networking.DockerRegistry.Model.Docker
{
	[Serializable]
	public class DockerImageV2Config : IImageConfig
	{
		public string MediaType { get; set; }

		public int Size { get; set; }

		public HashDigest Digest { get; set; }

		public DockerImageV2Config()
		{
		}

		public DockerImageV2Config(Dictionary<string, object> dict)
		{
			DockerImageV2ContentDescriptor Descriptor = DockerImageV2ContentDescriptor.Parse(dict);

			this.MediaType = Descriptor.MediaType;
			this.Size = Descriptor.Size;
			this.Digest = Descriptor.Digest;
		}
	}
}
