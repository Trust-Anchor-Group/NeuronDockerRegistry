using System.Collections.Generic;

namespace TAG.Networking.DockerRegistry.Model.Oci
{
	public interface IOciContentDescriptor
	{
		public string MediaType { get; set; }
		public int Size { get; set; }
		public HashDigest Digest { get; set; }
		public string[] Urls { get; set; }
		public Dictionary<string, string> Annotations { get; set; }
		public string Data { get; set; }
		public string ArtifactType { get; set; }
		public OciPlatform Platform { get; set; }
	}
}
