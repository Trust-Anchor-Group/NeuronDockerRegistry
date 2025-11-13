namespace TAG.Networking.DockerRegistry.Model
{
	public interface IImageManifest
	{
		public int SchemaVersion { get; }
		public string MediaType { get; }
		public byte[] Raw { get; set; }
		public IImageLayer[] GetLayers();
		public IImageConfig GetConfig();
	}
}
