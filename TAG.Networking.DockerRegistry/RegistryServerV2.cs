using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TAG.Networking.DockerRegistry.Errors;
using TAG.Networking.DockerRegistry.Model;
using Waher.Networking.HTTP;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Cache;
using Waher.Security;

namespace TAG.Networking.DockerRegistry
{
	/// <summary>
	/// Docker Registry API v2.
	/// 
	/// Reference:
	/// https://docs.docker.com/registry/spec/api/
	/// </summary>
	public class RegistryServerV2 : HttpAsynchronousResource, IHttpGetMethod, IHttpPostMethod, IHttpDeleteMethod,
		IHttpPatchMethod, IHttpPatchRangesMethod, IHttpPutMethod, IHttpPutRangesMethod, IDisposable
	{
		private static readonly Regex regexName = new Regex("[a-z0-9]+(?:[._-][a-z0-9]+)*", RegexOptions.Compiled | RegexOptions.Singleline);
		private static readonly string[] keyResourceNames = new string[]
		{
			"manifests",
			"blobs",
			"_catalog",
			"tags"
		};

		private readonly HttpAuthenticationScheme[] authenticationSchemes;
		private readonly Cache<Guid, BlobUpload> uploads = new Cache<Guid, BlobUpload>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromHours(1));

		/// <summary>
		/// Docker Registry API v2.
		/// </summary>
		/// <param name="AuthenticationSchemes">Authentication schemes.</param>
		public RegistryServerV2(params HttpAuthenticationScheme[] AuthenticationSchemes)
			: base("/v2")
		{
			this.authenticationSchemes = AuthenticationSchemes;
			this.uploads.Removed += this.Uploads_Removed;
		}

		/// <summary>
		/// If resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => true;

		/// <summary>
		/// If resource uses sessions (i.e. uses a session cookie).
		/// </summary>
		public override bool UserSessions => false;

		/// <summary>
		/// If GET method is supported.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// If POST method is supported.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// If DELETE method is supported.
		/// </summary>
		public bool AllowsDELETE => true;

		/// <summary>
		/// If PUT method is supported.
		/// </summary>
		public bool AllowsPUT => true;

		/// <summary>
		/// If PATCH method is supported.
		/// </summary>
		public bool AllowsPATCH => true;

		/// <summary>
		/// Gets available authentication schemes
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <returns>Array of authentication schemes.</returns>
		public override HttpAuthenticationScheme[] GetAuthenticationSchemes(HttpRequest Request)
		{
			return this.authenticationSchemes;
		}

		/// <summary>
		/// Checks if a Name is a valid Docker name.
		/// </summary>
		/// <param name="Name">Name</param>
		/// <returns>If <paramref name="Name"/> is a valid Docker name.</returns>
		public static bool IsName(string Name)
		{
			Match M = regexName.Match(Name);
			return M.Success && M.Index == 0 && M.Length == Name.Length;
		}

		private void Uploads_Removed(object Sender, CacheItemEventArgs<Guid, BlobUpload> e)
		{
			e.Value.Dispose();
		}

		/// <summary>
		/// Executes a GET method.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="Response">Response object.</param>
		public Task GET(HttpRequest Request, HttpResponse Response)
		{
			this.ProcessGet(Request, Response);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Executes a POST method.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="Response">Response object.</param>
		public Task POST(HttpRequest Request, HttpResponse Response)
		{
			this.ProcessPost(Request, Response);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Executes a DELETE method.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="Response">Response object.</param>
		public Task DELETE(HttpRequest Request, HttpResponse Response)
		{
			this.ProcessDelete(Request, Response);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Executes a PATCH method.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="Response">Response object.</param>
		public Task PATCH(HttpRequest Request, HttpResponse Response)
		{
			this.ProcessPatch(Request, Response, null);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Executes a PATCH method.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="Response">Response object.</param>
		/// <param name="Interval">Range interval.</param>
		public Task PATCH(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			this.ProcessPatch(Request, Response, Interval);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Executes a PUT method.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="Response">Response object.</param>
		public Task PUT(HttpRequest Request, HttpResponse Response)
		{
			this.ProcessPut(Request, Response, null);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Executes a PUT method.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="Response">Response object.</param>
		/// <param name="Interval">Range interval.</param>
		public Task PUT(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			this.ProcessPut(Request, Response, Interval);
			return Task.CompletedTask;
		}

		private async void ProcessGet(HttpRequest Request, HttpResponse Response)
		{
			try
			{
				string Resource = Request.SubPath;

				if (Resource == "/" || string.IsNullOrEmpty(Resource))  // API Version Check
				{
					Response.StatusCode = 200;
					await Response.SendResponse();
					return;
				}

				string[] ResourceParts = Resource.Split('/');
				int Pos = 0;
				int Len = ResourceParts.Length;

				if (!string.IsNullOrEmpty(ResourceParts[Pos++]))
					throw new BadRequestException(new DockerErrors(DockerErrorCode.UNSUPPORTED, "The operation is unsupported."));

				if (!TryGetKeyResourceName(ResourceParts, out string KeyResourceName, out string[] Names, ref Pos))
					throw new BadRequestException(new DockerErrors(DockerErrorCode.UNSUPPORTED, "The operation is unsupported."));

				switch (KeyResourceName)
				{
					case "blobs":
						if (Pos >= ResourceParts.Length)
							throw new BadRequestException(new DockerErrors(DockerErrorCode.DIGEST_INVALID, "Provided digest did not match uploaded content."));

						if (ResourceParts[Pos] == "uploads")
						{
							Pos++;

							// TODO
						}
						else
						{
							if (!TryGetDigest(ResourceParts, out HashFunction Function, out byte[] Digest, ref Pos))
								throw new BadRequestException(new DockerErrors(DockerErrorCode.DIGEST_INVALID, "Provided digest did not match uploaded content."));

							DockerBlob Blob = await Database.FindFirstIgnoreRest<DockerBlob>(new FilterAnd(
								new FilterFieldEqualTo("Digest", Digest),
								new FilterFieldEqualTo("Function", Function)))
								?? throw new NotFoundException(new DockerErrors(DockerErrorCode.BLOB_UNKNOWN, "BLOB unknown to registry."));

							// TODO
						}
						break;

					case "manifests":
					// TODO
					case "_catalog":
					// TODO
					case "tags":
					// TODO
					default:
						throw new BadRequestException(new DockerErrors(DockerErrorCode.UNSUPPORTED, "The operation is unsupported."));
				}

				throw new BadRequestException(new DockerErrors(DockerErrorCode.UNSUPPORTED, "The operation is unsupported."));
			}
			catch (Exception ex)
			{
				await Response.SendResponse(ex);
			}
		}

		private async void ProcessPost(HttpRequest Request, HttpResponse Response)
		{
			try
			{
				Prepare(Request.SubPath, out string[] ResourceParts, out int Pos, out int Len, out string KeyResourceName, out string[] Names);

				switch (KeyResourceName)
				{
					case "blobs":
						if (Pos >= ResourceParts.Length)
							throw new BadRequestException(new DockerErrors(DockerErrorCode.DIGEST_INVALID, "Provided digest did not match uploaded content."));

						if (ResourceParts[Pos] == "uploads")
						{
							Pos++;

							if (Pos == ResourceParts.Length || string.IsNullOrEmpty(ResourceParts[Pos]))
							{
								Pos++;

								Guid Uuid = Guid.NewGuid();

								this.uploads[Uuid] = new BlobUpload(Uuid);

								Response.StatusCode = 202;
								Response.StatusMessage = "Accepted";
								Response.SetHeader("Location", NameUrl(Names) + "/blobs/uploads/" + Uuid.ToString());
								Response.SetHeader("Range", "0-0");
								Response.SetHeader("Docker-Upload-UUID", Uuid.ToString());
								await Response.SendResponse();
								return;
							}
							else
							{
								// TODO
							}
						}
						else
						{
							if (!TryGetDigest(ResourceParts, out HashFunction Function, out byte[] Digest, ref Pos))
								throw new BadRequestException(new DockerErrors(DockerErrorCode.DIGEST_INVALID, "Provided digest did not match uploaded content."));

							DockerBlob Blob = await Database.FindFirstIgnoreRest<DockerBlob>(new FilterAnd(
								new FilterFieldEqualTo("Digest", Digest),
								new FilterFieldEqualTo("Function", Function)))
								?? throw new NotFoundException(new DockerErrors(DockerErrorCode.BLOB_UNKNOWN, "BLOB unknown to registry."));

							// TODO
						}
						break;

					case "manifests":
					// TODO
					case "_catalog":
					// TODO
					case "tags":
					// TODO
					default:
						throw new BadRequestException(new DockerErrors(DockerErrorCode.UNSUPPORTED, "The operation is unsupported."));
				}

				throw new BadRequestException(new DockerErrors(DockerErrorCode.UNSUPPORTED, "The operation is unsupported."));
			}
			catch (Exception ex)
			{
				await Response.SendResponse(ex);
			}
		}

		private async void ProcessPatch(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			try
			{
				Prepare(Request.SubPath, out string[] ResourceParts, out int Pos, out int Len, out string KeyResourceName, out string[] Names);

				switch (KeyResourceName)
				{
					case "blobs":
						if (Pos >= ResourceParts.Length)
							throw new BadRequestException(new DockerErrors(DockerErrorCode.DIGEST_INVALID, "Provided digest did not match uploaded content."));

						if (ResourceParts[Pos] == "uploads")
						{
							Pos++;

							if (Pos == ResourceParts.Length ||
								string.IsNullOrEmpty(ResourceParts[Pos]) ||
								!Guid.TryParse(ResourceParts[Pos++], out Guid Uuid) ||
								!Request.HasData)
							{
								throw new BadRequestException(new DockerErrors(DockerErrorCode.BLOB_UPLOAD_INVALID, "BLOB upload invalid."));
							}

							if (!this.uploads.TryGetValue(Uuid, out BlobUpload UploadRecord))
								throw new BadRequestException(new DockerErrors(DockerErrorCode.BLOB_UPLOAD_UNKNOWN, "BLOB upload unknown to registry."));

							Request.DataStream.Position = 0;

							long Offset = Interval?.First ?? 0L;
							long Count = Interval is null ? Request.DataStream.Length : Interval.Last - Interval.First + 1;

							Response.StatusCode = 202;
							Response.StatusMessage = "Accepted";
							Response.SetHeader("Location", NameUrl(Names) + "/blobs/uploads/" + Uuid.ToString());
							Response.SetHeader("Range", "0-0");
							Response.SetHeader("Docker-Upload-UUID", Uuid.ToString());
							await Response.SendResponse();
							return;
						}
						else
						{
							// TODO
						}
						break;

					// TODO
					case "manifests":
					// TODO
					case "_catalog":
					// TODO
					case "tags":
					// TODO
					default:
						throw new BadRequestException(new DockerErrors(DockerErrorCode.UNSUPPORTED, "The operation is unsupported."));
				}

				throw new BadRequestException(new DockerErrors(DockerErrorCode.UNSUPPORTED, "The operation is unsupported."));
			}
			catch (Exception ex)
			{
				await Response.SendResponse(ex);
			}
		}

		private async void ProcessPut(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			try
			{
				Prepare(Request.SubPath, out string[] ResourceParts, out int Pos, out int Len, out string KeyResourceName, out string[] Names);

				switch (KeyResourceName)
				{
					case "blobs":
					// TODO
					case "manifests":
					// TODO
					case "_catalog":
					// TODO
					case "tags":
					// TODO
					default:
						throw new BadRequestException(new DockerErrors(DockerErrorCode.UNSUPPORTED, "The operation is unsupported."));
				}

				throw new BadRequestException(new DockerErrors(DockerErrorCode.UNSUPPORTED, "The operation is unsupported."));
			}
			catch (Exception ex)
			{
				await Response.SendResponse(ex);
			}
		}

		private async void ProcessDelete(HttpRequest Request, HttpResponse Response)
		{
			try
			{
				Prepare(Request.SubPath, out string[] ResourceParts, out int Pos, out int Len, out string KeyResourceName, out string[] Names);

				switch (KeyResourceName)
				{
					case "blobs":
					// TODO
					case "manifests":
					// TODO
					case "_catalog":
					// TODO
					case "tags":
					// TODO
					default:
						throw new BadRequestException(new DockerErrors(DockerErrorCode.UNSUPPORTED, "The operation is unsupported."));
				}

				throw new BadRequestException(new DockerErrors(DockerErrorCode.UNSUPPORTED, "The operation is unsupported."));
			}
			catch (Exception ex)
			{
				await Response.SendResponse(ex);
			}
		}

		private static void Prepare(string Resource, out string[] ResourceParts, out int Pos, out int Len,
			out string KeyResourceName, out string[] Names)
		{
			if (Resource == "/" || string.IsNullOrEmpty(Resource))  // API Version Check
				throw new BadRequestException(new DockerErrors(DockerErrorCode.UNSUPPORTED, "The operation is unsupported."));

			ResourceParts = Resource.Split('/');
			Len = ResourceParts.Length;
			Pos = 0;

			if (!string.IsNullOrEmpty(ResourceParts[Pos++]))
				throw new BadRequestException(new DockerErrors(DockerErrorCode.UNSUPPORTED, "The operation is unsupported."));

			if (!TryGetKeyResourceName(ResourceParts, out KeyResourceName, out Names, ref Pos))
				throw new BadRequestException(new DockerErrors(DockerErrorCode.UNSUPPORTED, "The operation is unsupported."));
		}

		private static string NameUrl(string[] Names)
		{
			StringBuilder sb = new StringBuilder("/v2");

			foreach (string Name in Names)
			{
				sb.Append('/');
				sb.Append(Name);
			}

			return sb.ToString();
		}

		private static bool TryGetDigest(string[] Parts, out HashFunction Function, out byte[] Digest, ref int Pos)
		{
			Function = default;
			Digest = null;

			if (Pos != Parts.Length - 1)
				return false;

			string s = Parts[Pos++];
			int i = s.IndexOf(':');
			if (i < 0)
				return false;

			if (!Enum.TryParse(s.Substring(0, i), true, out Function))
				return false;

			try
			{
				Digest = Hashes.StringToBinary(s.Substring(i + 1));
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private static bool TryGetKeyResourceName(string[] Parts, out string KeyResourceName, out string[] Name, ref int Pos)
		{
			KeyResourceName = null;
			Name = null;

			int Len = Parts.Length;
			if (Pos >= Len)
				return false;

			List<string> Names = new List<string>();

			while (Pos < Len)
			{
				string s = Parts[Pos++];
				int i = Array.IndexOf(keyResourceNames, s);
				if (i >= 0)
				{
					KeyResourceName = s;
					Name = Names.ToArray();
					return true;
				}

				if (!IsName(s))
					throw new BadRequestException(new DockerErrors(DockerErrorCode.NAME_INVALID, "Invalid repository name."));

				Names.Add(s);
			}

			return false;
		}

		private static string GetDigestString(HashFunction HashFunction, byte[] Digest)
		{
			return HashFunction.ToString().ToLower() + ":" + Hashes.BinaryToString(Digest);
		}

		private void AddDigestHeader(HttpResponse Response, HashFunction HashFunction, byte[] Digest)
		{
			Response.SetHeader("Docker-Content-Digest", GetDigestString(HashFunction, Digest));
		}

		/// <summary>
		/// Disposes of the resource.
		/// </summary>
		public void Dispose()
		{
			this.uploads.Clear();
			this.uploads.Dispose();
		}


		// TODO:
		// - 429 rate limit error in Networking.HTTP: Customize payload.
		// - 401 Unauthorized error in Networking.HTTP: Customize payload.
	}
}
