using System;
using System.Collections.Generic;

namespace TAG.Networking.DockerRegistry.Model.Docker
{
	[Serializable]
	public class DockerImageManifestV2 : IImageManifest
	{
		public const int SchemaVersionValue = 2;
		public const string MediaTypeValue = "application/vnd.docker.distribution.manifest.v2+json";
		public int SchemaVersion => SchemaVersionValue;
		public string MediaType => MediaTypeValue;
		public DockerImageV2Config Config { get; set; }
		public DockerImageV2Layer[] Layers { get; set; }
		public string Raw { get; set; }

		public DockerImageManifestV2()
		{

		}

		public DockerImageManifestV2(Dictionary<string, object> Dict)
		{
			if (!Dict.TryGetValue("mediaType", out object MediaTypeObj) ||
				!(MediaTypeObj is string MediaType) || MediaType != MediaTypeValue)
				throw new Exception("Unsupported media type. Only 'application/vnd.docker.distribution.manifest.v2+json' is supported.");

			if (!(Dict.TryGetValue("schemaVersion", out object SchemaVersionObj) &&
				SchemaVersionObj is int SchemaVersion &&
				SchemaVersion == SchemaVersionValue))
				throw new Exception("Unsupported schema version. Only version 2 is supported.");

			// config
			if (!(Dict.TryGetValue("config", out object ConfigObj) &&
				ConfigObj is Dictionary<string, object> DictConfig))
				throw new Exception("Invalid config.");

			DockerImageV2Config ParsedConfig = new DockerImageV2Config(DictConfig);
			Config = ParsedConfig;

			if (!(Dict.TryGetValue("layers", out object LayersObj) && LayersObj is object[] DictLayers))
				throw new Exception("Invalid layers.");

			List<DockerImageV2Layer> ParsedLayers = new List<DockerImageV2Layer>();

			foreach (object LayerObj in DictLayers)
			{
				if (!(LayerObj is Dictionary<string, object> LayerDict))
					throw new Exception("Invalid layer.");

				ParsedLayers.Add(new DockerImageV2Layer(LayerDict));
			}

			Layers = ParsedLayers.ToArray();
		}

		public IImageLayer[] GetLayers()
		{
			return Layers;
		}
		public IImageConfig GetConfig()
		{
			return Config;
		}
	}
}
