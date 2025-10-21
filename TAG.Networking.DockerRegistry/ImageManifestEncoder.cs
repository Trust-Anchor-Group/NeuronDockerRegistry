using System;
using System.Text;
using System.Threading.Tasks;
using TAG.Networking.DockerRegistry.Model;
using TAG.Networking.DockerRegistry.Model.Oci;
using Waher.Content;
using Waher.Runtime.Inventory;

namespace TAG.Networking.DockerRegistry
{
	public class ImageManifestEncoder : IContentEncoder
	{
		public ImageManifestEncoder()
		{

		}

		public string[] ContentTypes => contentTypes;
		private static readonly string[] contentTypes = new string[]
		{
			OCIImageManifest.MediaTypeValue
		};


		public string[] FileExtensions => throw new System.NotImplementedException();

		public Task<ContentResponse> EncodeAsync(object Object, Encoding Encoding, ICodecProgress Progress, params string[] AcceptedContentTypes)
		{
			if (!(Object is IImageManifest Manifest))
			{
				return Task.FromResult(new ContentResponse(new ArgumentException("Object not IImageManifest.", nameof(Object))));
			}

			return Task.FromResult(new ContentResponse(Manifest.MediaType, Object, Encoding.UTF8.GetBytes(Manifest.Raw)));
		}

		public bool Encodes(object Object, out Grade Grade, params string[] AcceptedContentTypes)
		{
			if (Object is IImageManifest)
			{
				Grade = Grade.Ok;
				return true;
			}
			else
			{
				Grade = Grade.NotAtAll;
				return false;
			}
		}

		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			ContentType = "";
			return false;
		}

		public bool TryGetFileExtension(string ContentType, out string FileExtension)
		{
			FileExtension = "";
			return false;
		}
	}
}
