namespace TAG.Networking.DockerRegistry.Model.DockerImages
{
	internal interface IDockerImageV2ContentDescriptor
	{
		public string MediaType { get; set; }
		public int Size { get; set; }
		public HashDigest Digest { get; set; }
	}
}
