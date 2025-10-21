using System;
using System.Collections.Generic;

namespace TAG.Networking.DockerRegistry.Model.Oci
{
	[Serializable]
	public class OciPlatform
	{
		public OciPlatform()
		{

		}
		public OciPlatform(Dictionary<string, object> Json)
		{
			if (!(Json.TryGetValue("architecture", out object ArchitectureObj) && ArchitectureObj is string JsonArchitecture))
				throw new Exception("Invalid architecture.");
			Architecture = JsonArchitecture;

			if (!(Json.TryGetValue("os", out object OSObj) && OSObj is string JsonOS))
				throw new Exception("Invalid os.");
			OS = JsonOS;

			if (Json.TryGetValue("os.version", out object OSVersionObj) && OSVersionObj is string JsonOSVersion)
				OSVersion = JsonOSVersion;

			if (Json.TryGetValue("os.features", out object OSFeaturesObj) && OSFeaturesObj is object[] OSFeaturesArray)
			{
				List<string> OSFeaturesList = new List<string>();
				foreach (object Feature in OSFeaturesArray)
				{
					if (Feature is string s)
						OSFeaturesList.Add(s);
				}
				OSFeatures = OSFeaturesList.ToArray();
			}

			if (Json.TryGetValue("variant", out object VariantObj) && VariantObj is string JsonVariant)
				Variant = JsonVariant;

			if (Json.TryGetValue("features", out object FeaturesObj) && FeaturesObj is Dictionary<string, string> FeaturesDict)
				Features = FeaturesDict;

		}

		public string Architecture; // required
		public string OS; // required
		public string OSVersion; // optional
		public string[] OSFeatures; // optional
		public string Variant; // optional
		public Dictionary<string, string> Features; // optional
	}
}
