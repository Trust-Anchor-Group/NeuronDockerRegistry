using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Inventory;

namespace TAG.Networking.DockerRegistry.Errors
{
	/// <summary>
	/// Encodes and decodes Docker errors.
	/// </summary>
	public class DockerErrorsEncoder : IContentEncoder
	{
		/// <summary>
		/// Encodes and decodes Docker errors.
		/// </summary>
		public DockerErrorsEncoder()
		{
		}

		/// <summary>
		/// Content-Types supported
		/// </summary>
		public string[] ContentTypes => new string[0];

		/// <summary>
		/// File extensions supported
		/// </summary>
		public string[] FileExtensions => new string[0];

		/// <summary>
		/// Checks if the encoder encodes objects of a given type.
		/// </summary>
		/// <param name="Object">Object instance.</param>
		/// <param name="Grade">Support grade</param>
		/// <param name="AcceptedContentTypes">Content-Types accepted in response.</param>
		/// <returns>If the encoder encodes objects of the given type.</returns>
		public bool Encodes(object Object, out Grade Grade, params string[] AcceptedContentTypes)
		{
			if (Object is DockerErrors Errors || Object is DockerError || Object is DockerErrorCode)
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

		/// <summary>
		/// Encodes an object.
		/// </summary>
		/// <param name="Object">Object instance.</param>
		/// <param name="Encoding">Default text encoding.</param>
		/// <param name="AcceptedContentTypes">Accepted content-types.</param>
		/// <returns>Encoded object.</returns>
		public Task<KeyValuePair<byte[], string>> EncodeAsync(object Object, Encoding Encoding, params string[] AcceptedContentTypes)
		{
			if (!(Object is DockerErrors Errors))
			{
				if (Object is DockerError Error)
					Errors = new DockerErrors(Error);
				else if (Object is DockerErrorCode ErrorCode)
					Errors = new DockerErrors(ErrorCode);
				else
					throw new ArgumentException("Object not a Docker error.", nameof(Object));
			}

			List<Dictionary<string, object>> EncodedErrors = new List<Dictionary<string, object>>();

			foreach (DockerError Error in Errors.Errors)
			{
				EncodedErrors.Add(new Dictionary<string, object>()
				{
					{ "code", Error.Code },
					{ "message", Error.Message },
					{ "detail", Error.Detail }
				});
			}

			string s = JSON.Encode(new Dictionary<string, object>()
			{
				{"errors", EncodedErrors.ToArray() }
			}, false);

			if (Encoding is null)
				Encoding = Encoding.UTF8;

			byte[] Bin = Encoding.GetBytes(s);
			string ContentType = Waher.Content.Json.JsonCodec.DefaultContentType + "; charset=" + Encoding.WebName;

			return Task.FromResult(new KeyValuePair<byte[], string>(Bin, ContentType));
		}

		/// <summary>
		/// Tries to get the content-
		/// </summary>
		/// <param name="FileExtension"></param>
		/// <param name="ContentType"></param>
		/// <returns></returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			ContentType = null;
			return false;
		}

		public bool TryGetFileExtension(string ContentType, out string FileExtension)
		{
			FileExtension = null;
			return false;
		}
	}
}
