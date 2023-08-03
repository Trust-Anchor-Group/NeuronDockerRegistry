using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
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
	public class RegistryServerV2 : HttpSynchronousResource, IHttpGetMethod, IHttpGetRangesMethod, IHttpPostMethod,
		IHttpDeleteMethod, IHttpPatchMethod, IHttpPatchRangesMethod, IHttpPutMethod, IHttpPutRangesMethod, IDisposable
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
		private readonly string dockerRegistryFolder;

		/// <summary>
		/// Docker Registry API v2.
		/// </summary>
		/// <param name="DockerRegistryFolder">Docker Registry folder.</param>
		/// <param name="AuthenticationSchemes">Authentication schemes.</param>
		public RegistryServerV2(string DockerRegistryFolder, params HttpAuthenticationScheme[] AuthenticationSchemes)
			: base("/v2")
		{
			this.dockerRegistryFolder = DockerRegistryFolder;
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
			return this.GET(Request, Response, null);
		}

		/// <summary>
		/// Executes a GET method.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="Response">Response object.</param>
		/// <param name="Interval">Range interval.</param>
		public async Task GET(HttpRequest Request, HttpResponse Response, ByteRangeInterval Interval)
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

							if (!Request.User.HasPrivilege("Docker.Upload"))
								throw new ForbiddenException(new DockerErrors(DockerErrorCode.DENIED, "Requested access to the resource is denied."));

							if (Pos == ResourceParts.Length ||
								string.IsNullOrEmpty(ResourceParts[Pos]) ||
								!Guid.TryParse(ResourceParts[Pos++], out Guid Uuid))
							{
								throw new BadRequestException(new DockerErrors(DockerErrorCode.BLOB_UPLOAD_INVALID, "BLOB upload invalid."));
							}

							if (!this.uploads.TryGetValue(Uuid, out BlobUpload UploadRecord))
								throw new NotFoundException(new DockerErrors(DockerErrorCode.BLOB_UPLOAD_UNKNOWN, "BLOB upload unknown to registry."));

							await UploadRecord.Lock();
							try
							{
								Response.SetHeader("Location", NameUrl(Names) + "/blobs/uploads/" + Uuid.ToString());
								Response.SetHeader("Docker-Upload-UUID", Uuid.ToString());

								if (UploadRecord.File is null || UploadRecord.File.Length == 0)
								{
									Response.StatusCode = 204;
									Response.StatusMessage = "No Content";
									Response.SetHeader("Content-Range", "0-0/0");
								}
								else
								{
									long Offset = Interval?.First ?? 0L;
									long Count;

									if (Interval is null)
										Count = UploadRecord.File.Length;
									else
									{
										long Last = Interval.Last ?? UploadRecord.File.Length - 1;
										Count = Last - Offset + 1;
									}

									Response.StatusCode = 200;
									Response.SetHeader("Content-Range", Offset.ToString() + "-" +
										(Offset + Count - 1).ToString() + "/" + UploadRecord.File.Length.ToString());

									await WriteToResponse(Response, UploadRecord.File, Offset, Count);
								}
							}
							finally
							{
								UploadRecord.Release();
							}

							await Response.SendResponse();
							return;
						}
						else
						{
							if (!TryGetDigest(ResourceParts, out HashFunction Function, out byte[] Digest, ref Pos))
								throw new BadRequestException(new DockerErrors(DockerErrorCode.DIGEST_INVALID, "Provided digest did not match uploaded content."));

							DockerBlob Blob = await Database.FindFirstIgnoreRest<DockerBlob>(new FilterAnd(
								new FilterFieldEqualTo("Digest", Digest),
								new FilterFieldEqualTo("Function", Function)))
								?? throw new NotFoundException(new DockerErrors(DockerErrorCode.BLOB_UNKNOWN, "BLOB unknown to registry."));

							if (!File.Exists(Blob.FileName))
								throw new NotFoundException(new DockerErrors(DockerErrorCode.BLOB_UNKNOWN, "BLOB unknown to registry."));

							if (!Request.User.HasPrivilege("Docker.Download"))
								throw new ForbiddenException(new DockerErrors(DockerErrorCode.DENIED, "Requested access to the resource is denied."));

							using (FileStream BlobFile = File.OpenRead(Blob.FileName))
							{
								long Offset = Interval?.First ?? 0L;
								long Count;

								if (Interval is null)
									Count = BlobFile.Length;
								else
								{
									long Last = Interval.Last ?? BlobFile.Length - 1;
									Count = Last - Offset + 1;
								}

								Response.StatusCode = 200;
								Response.SetHeader("Content-Range", Offset.ToString() + "-" +
									(Offset + Count - 1).ToString() + "/" + BlobFile.Length.ToString());

								await WriteToResponse(Response, BlobFile, Offset, Count);
							}

							await Response.SendResponse();
							return;
						}

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

		private static async Task WriteToResponse(HttpResponse Response, FileStream File, long Offset, long Count)
		{
			if (!Response.OnlyHeader)
			{
				File.Position = Offset;

				byte[] Buf = new byte[65536];
				int NrBytes;

				while (Count > 0)
				{
					NrBytes = (int)Math.Min(65536, Count);

					await File.ReadAsync(Buf, 0, NrBytes);
					await Response.Write(Buf, 0, NrBytes);

					Count -= NrBytes;
				}
			}
		}

		/// <summary>
		/// Executes a POST method.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="Response">Response object.</param>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			try
			{
				if (!Request.User.HasPrivilege("Docker.Upload"))
					throw new ForbiddenException(new DockerErrors(DockerErrorCode.DENIED, "Requested access to the resource is denied."));

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

		/// <summary>
		/// Executes a DELETE method.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="Response">Response object.</param>
		public async Task DELETE(HttpRequest Request, HttpResponse Response)
		{
			try
			{
				if (!Request.User.HasPrivilege("Docker.Upload"))
					throw new ForbiddenException(new DockerErrors(DockerErrorCode.DENIED, "Requested access to the resource is denied."));

				Prepare(Request.SubPath, out string[] ResourceParts, out int Pos, out int Len, out string KeyResourceName, out string[] Names);

				switch (KeyResourceName)
				{
					case "blobs":
						if (Pos >= ResourceParts.Length)
							throw new BadRequestException(new DockerErrors(DockerErrorCode.BLOB_UPLOAD_INVALID, "BLOB upload invalid."));

						if (ResourceParts[Pos] == "uploads")
						{
							Pos++;

							if (Pos == ResourceParts.Length ||
								string.IsNullOrEmpty(ResourceParts[Pos]) ||
								!Guid.TryParse(ResourceParts[Pos++], out Guid Uuid))
							{
								throw new BadRequestException(new DockerErrors(DockerErrorCode.BLOB_UPLOAD_INVALID, "BLOB upload invalid."));
							}

							if (!this.uploads.TryGetValue(Uuid, out BlobUpload UploadRecord))
								throw new NotFoundException(new DockerErrors(DockerErrorCode.BLOB_UPLOAD_UNKNOWN, "BLOB upload unknown to registry."));

							this.uploads.Remove(Uuid);

							Response.StatusCode = 200;
							await Response.SendResponse();
							return;
						}
						else
						{
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

		/// <summary>
		/// Executes a PATCH method.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="Response">Response object.</param>
		public Task PATCH(HttpRequest Request, HttpResponse Response)
		{
			return this.PATCH(Request, Response, null);
		}

		/// <summary>
		/// Executes a PATCH method.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="Response">Response object.</param>
		/// <param name="Interval">Range interval.</param>
		public async Task PATCH(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			try
			{
				if (!Request.User.HasPrivilege("Docker.Upload"))
					throw new ForbiddenException(new DockerErrors(DockerErrorCode.DENIED, "Requested access to the resource is denied."));

				Prepare(Request.SubPath, out string[] ResourceParts, out int Pos, out int Len, out string KeyResourceName, out string[] Names);

				switch (KeyResourceName)
				{
					case "blobs":
						if (Pos >= ResourceParts.Length)
							throw new BadRequestException(new DockerErrors(DockerErrorCode.BLOB_UPLOAD_INVALID, "BLOB upload invalid."));

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
								throw new NotFoundException(new DockerErrors(DockerErrorCode.BLOB_UPLOAD_UNKNOWN, "BLOB upload unknown to registry."));

							await UploadRecord.Lock();
							try
							{
								await this.CopyToBlobLocked(Request, Interval, UploadRecord, Uuid, Names);

								Response.StatusCode = 202;
								Response.StatusMessage = "Accepted";
								Response.SetHeader("Location", NameUrl(Names) + "/blobs/uploads/" + Uuid.ToString());
								Response.SetHeader("Range", "0-" + UploadRecord.File.Length.ToString());
								Response.SetHeader("Docker-Upload-UUID", Uuid.ToString());
								await Response.SendResponse();
								return;
							}
							finally
							{
								UploadRecord.Release();
							}
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

		/// <summary>
		/// Executes a PUT method.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="Response">Response object.</param>
		public Task PUT(HttpRequest Request, HttpResponse Response)
		{
			return this.PUT(Request, Response, null);
		}

		/// <summary>
		/// Executes a PUT method.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="Response">Response object.</param>
		/// <param name="Interval">Range interval.</param>
		public async Task PUT(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			try
			{
				if (!Request.User.HasPrivilege("Docker.Upload"))
					throw new ForbiddenException(new DockerErrors(DockerErrorCode.DENIED, "Requested access to the resource is denied."));

				Prepare(Request.SubPath, out string[] ResourceParts, out int Pos, out int Len, out string KeyResourceName, out string[] Names);

				switch (KeyResourceName)
				{
					case "blobs":
						if (Pos >= ResourceParts.Length)
							throw new BadRequestException(new DockerErrors(DockerErrorCode.BLOB_UPLOAD_INVALID, "BLOB upload invalid."));

						string DigestStr;
						byte[] Digest;
						HashFunction Function;

						if (ResourceParts[Pos] == "uploads")
						{
							Pos++;

							if (Pos == ResourceParts.Length ||
								string.IsNullOrEmpty(ResourceParts[Pos]) ||
								!Guid.TryParse(ResourceParts[Pos++], out Guid Uuid))
							{
								throw new BadRequestException(new DockerErrors(DockerErrorCode.BLOB_UPLOAD_INVALID, "BLOB upload invalid."));
							}

							if (!this.uploads.TryGetValue(Uuid, out BlobUpload UploadRecord))
								throw new NotFoundException(new DockerErrors(DockerErrorCode.BLOB_UPLOAD_UNKNOWN, "BLOB upload unknown to registry."));

							await UploadRecord.Lock();
							try
							{
								if (Request.HasData)
									await this.CopyToBlobLocked(Request, Interval, UploadRecord, Uuid, Names);

								if (!Request.Header.TryGetQueryParameter("digest", out DigestStr) ||
									!TryParseDigest(DigestStr = HttpUtility.UrlDecode(DigestStr), out Function, out Digest) ||
									Convert.ToBase64String(Digest) != Convert.ToBase64String(UploadRecord.ComputeDigestLocked(Function)))
								{
									throw new BadRequestException(new DockerErrors(DockerErrorCode.DIGEST_INVALID, "Provided digest did not match uploaded content."));
								}


								string ContentFileName = Path.Combine(this.BlobFolder, Hashes.BinaryToString(Digest) + ".bin");

								DockerBlob Prev = await Database.FindFirstIgnoreRest<DockerBlob>(new FilterAnd(
									new FilterFieldEqualTo("Digest", Digest),
									new FilterFieldEqualTo("Function", Function)));

								if (File.Exists(ContentFileName) || !(Prev is null))
									throw new ForbiddenException(new DockerErrors(DockerErrorCode.DENIED, "BLOB already exists."));

								using (FileStream Content = File.Create(ContentFileName))
								{
									UploadRecord.File.Position = 0;
									await UploadRecord.File.CopyToAsync(Content);
								}

								UploadRecord.Blob.Function = Function;
								UploadRecord.Blob.Digest = Digest;
								UploadRecord.Blob.FileName = ContentFileName;

								await Database.Insert(UploadRecord.Blob);
							}
							finally
							{
								UploadRecord.Release();
							}

							this.uploads.Remove(Uuid);

							Response.StatusCode = 201;
							Response.StatusMessage = "Created";
							Response.SetHeader("Location", NameUrl(Names) + "/blobs/" + DigestStr);
							Response.SetHeader("Docker-Content-Digest", DigestStr);
							await Response.SendResponse();
							return;
						}
						break;

					case "manifests":
						if (Pos >= ResourceParts.Length)
							throw new BadRequestException(new DockerErrors(DockerErrorCode.MANIFEST_INVALID, "Manifest invalid. URL incomplete."));

						string Reference = ResourceParts[Pos++];
						object Manifest = await Request.DecodeDataAsync();
						string ImageName = JoinNames(Names);
						string Tag = null;

						if (!(Manifest is Dictionary<string, object> ManifestObj))
							throw new BadRequestException(new DockerErrors(DockerErrorCode.MANIFEST_INVALID, "Manifest invalid. Unrecognized content."));

						if (ManifestObj.TryGetValue("name", out object Obj))
						{
							if (!(Obj is string ManifestName) || ImageName != ManifestName)
								throw new BadRequestException(new DockerErrors(DockerErrorCode.MANIFEST_INVALID, "Manifest invalid. URL name mismatch."));
						}

						if (ManifestObj.TryGetValue("layers", out Obj) && Obj is Array Layers)
						{
							int i, c = Layers.Length;

							for (i = 0; i < c; i++)
							{
								if (Layers.GetValue(i) is Dictionary<string, object> LayerObj &&
									LayerObj.TryGetValue("digest", out Obj) &&
									Obj is string LayerDigestStr &&
									TryParseDigest(LayerDigestStr, out HashFunction LayerFunction, out byte[] LayerDigest))
								{
									DockerBlob Layer = await Database.FindFirstIgnoreRest<DockerBlob>(new FilterAnd(
										new FilterFieldEqualTo("Digest", LayerDigest),
										new FilterFieldEqualTo("Function", LayerFunction)))
										?? throw new BadRequestException(new DockerErrors(DockerErrorCode.BLOB_UNKNOWN,
										"BLOB unknown to registry.", new Dictionary<string, object>()
										{
											{ "digest", LayerDigestStr }
										}));
								}
								else
									throw new BadRequestException(new DockerErrors(DockerErrorCode.MANIFEST_INVALID, "Manifest invalid. Invalid layer."));
							}
						}
						else
							throw new BadRequestException(new DockerErrors(DockerErrorCode.MANIFEST_INVALID, "Manifest invalid. Missing layers."));

						if (TryParseDigest(Reference, out Function, out Digest))
						{
							if (ManifestObj.TryGetValue("config", out Obj) &&
								Obj is Dictionary<string, object> Config &&
								Config.TryGetValue("digest", out Obj))
							{
								if ((DigestStr = Obj as string) is null ||
									!TryParseDigest(DigestStr, out HashFunction Function2, out byte[] Digest2))
								{
									throw new BadRequestException(new DockerErrors(DockerErrorCode.MANIFEST_INVALID, "Manifest invalid. Missing digest in config."));
								}

								if (Function != Function2 || Convert.ToBase64String(Digest) != Convert.ToBase64String(Digest2))
									throw new BadRequestException(new DockerErrors(DockerErrorCode.MANIFEST_INVALID, "Manifest invalid. Digest mismatch."));
							}
						}
						else
						{
							Tag = Reference;

							if (ManifestObj.TryGetValue("config", out Obj) && Obj is Dictionary<string, object> Config)
							{
								if (!Config.TryGetValue("digest", out Obj) ||
									(DigestStr = Obj as string) is null ||
									!TryParseDigest(DigestStr, out Function, out Digest))
								{
									throw new BadRequestException(new DockerErrors(DockerErrorCode.MANIFEST_INVALID, "Manifest invalid. Missing digest in config."));
								}
							}
							else
								throw new BadRequestException(new DockerErrors(DockerErrorCode.MANIFEST_INVALID, "Manifest invalid. Missing config."));
						}

						if (Pos == ResourceParts.Length)
						{
							DockerImage Image;

							if (string.IsNullOrEmpty(Tag))
							{
								Image = await Database.FindFirstIgnoreRest<DockerImage>(new FilterAnd(
									new FilterFieldEqualTo("Image", ImageName),
									new FilterFieldEqualTo("Digest", Digest),
									new FilterFieldEqualTo("Function", Function)));

								if (!(Image is null))
									Tag = Image.Tag;
								else
									throw new BadRequestException(new DockerErrors(DockerErrorCode.MANIFEST_INVALID, "Manifest invalid. Missing tag."));
							}
							else
							{
								Image = await Database.FindFirstIgnoreRest<DockerImage>(new FilterAnd(
									new FilterFieldEqualTo("Image", ImageName),
									new FilterFieldEqualTo("Tag", Tag)));
							}

							if (Image is null)
							{
								Image = new DockerImage()
								{
									AccountName = Request.User.UserName,
									Image = ImageName,
									Tag = Tag,
									Manifest = Manifest,
									Digest = Digest,
									Function = Function
								};

								await Database.Insert(Image);
							}
							else if (Image.AccountName != Request.User.UserName)
								throw new ForbiddenException(new DockerErrors(DockerErrorCode.DENIED, "Requested access to the resource is denied."));
							else
							{
								if (string.IsNullOrEmpty(Tag))
									Tag = Image.Tag;
								else
									Image.Tag = Tag;

								Image.Manifest = Manifest;
								Image.Digest = Digest;
								Image.Function = Function;

								await Database.Update(Image);
							}

							Response.StatusCode = 201;
							Response.StatusMessage = "Created";
							await Response.SendResponse();
							return;
						}
						break;

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

		/// <summary>
		/// Folder where BLOBs are uploaded to.
		/// </summary>
		public string UploadFolder
		{
			get
			{
				string UploadFolder = Path.Combine(this.dockerRegistryFolder, "Uploads");

				if (!Directory.Exists(UploadFolder))
					Directory.CreateDirectory(UploadFolder);

				return UploadFolder;
			}
		}

		/// <summary>
		/// Folder where validated uploaded BLOBs are stored.
		/// </summary>
		public string BlobFolder
		{
			get
			{
				string BlobFolder = Path.Combine(this.dockerRegistryFolder, "BLOBs");

				if (!Directory.Exists(BlobFolder))
					Directory.CreateDirectory(BlobFolder);

				return BlobFolder;
			}
		}

		private async Task CopyToBlobLocked(HttpRequest Request, ContentByteRangeInterval Interval, BlobUpload UploadRecord, Guid Uuid, string[] Names)
		{
			Request.DataStream.Position = 0;

			long Offset = Interval?.First ?? 0L;
			long Count = Interval is null ? Request.DataStream.Length : Interval.Last - Interval.First + 1;

			if (UploadRecord.Blob is null)
			{
				UploadRecord.Blob = new DockerBlob()
				{
					AccountName = Request.User.UserName,
					Image = JoinNames(Names)
				};

				UploadRecord.FileName = Path.Combine(this.UploadFolder, Uuid.ToString() + ".bin");

				if (File.Exists(UploadRecord.FileName))
					UploadRecord.File = File.OpenWrite(UploadRecord.FileName);
				else
					UploadRecord.File = File.Create(UploadRecord.FileName);
			}

			if (UploadRecord.File.Length < Offset)
			{
				byte[] Buf = new byte[65536];

				UploadRecord.File.Position = UploadRecord.File.Length;

				while (UploadRecord.File.Length < Offset)
				{
					int NrBytes = (int)Math.Min(65536, Offset - UploadRecord.File.Length);
					await UploadRecord.File.WriteAsync(Buf, 0, NrBytes);
				}
			}
			else
				UploadRecord.File.Position = Offset;

			while (Count > 0)
			{
				int NrBytes = (int)Math.Min(65536, Count);
				await Request.DataStream.CopyToAsync(UploadRecord.File, NrBytes);
				Count -= NrBytes;
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

		private static string JoinNames(string[] Names)
		{
			StringBuilder sb = new StringBuilder();
			bool First = true;

			foreach (string Name in Names)
			{
				if (First)
					First = false;
				else
					sb.Append('/');

				sb.Append(Name);
			}

			return sb.ToString();
		}

		private static bool TryGetDigest(string[] Parts, out HashFunction Function, out byte[] Digest, ref int Pos)
		{
			if (Pos != Parts.Length - 1)
			{
				Function = default;
				Digest = null;
				return false;
			}
			else
				return TryParseDigest(HttpUtility.UrlDecode(Parts[Pos++]), out Function, out Digest);
		}

		private static bool TryParseDigest(string s, out HashFunction Function, out byte[] Digest)
		{
			Function = default;
			Digest = null;

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

		/// <summary>
		/// Disposes of the resource.
		/// </summary>
		public void Dispose()
		{
			this.uploads.Clear();
			this.uploads.Dispose();
		}

		/// <summary>
		/// Returns Docker Registry error content for common HTTP error codes (if not provided by resource).
		/// </summary>
		/// <param name="StatusCode">HTTP Status code to return.</param>
		/// <returns>Custom content, or null if none.</returns>
		public override Task<object> DefaultErrorContent(int StatusCode)
		{
			switch (StatusCode)
			{
				case 429: return Task.FromResult<object>(new DockerErrors(DockerErrorCode.DENIED, "Requested access to the resource is denied due to rate limitations."));
				case 401: return Task.FromResult<object>(new DockerErrors(DockerErrorCode.UNAUTHORIZED, "Authentication required."));
				default: return base.DefaultErrorContent(StatusCode);
			}
		}
	}
}
