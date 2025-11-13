using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TAG.Networking.DockerRegistry.Errors;
using TAG.Networking.DockerRegistry.Model;
using Waher.Networking.HTTP;

namespace TAG.Networking.DockerRegistry.Endpoints
{
	internal abstract class DockerEndpoints : IDockerEndpoints
	{
		protected static readonly KeyValuePair<string, string> apiHeader = new KeyValuePair<string, string>("Docker-Distribution-API-Version", "registry/2.0");

		protected readonly string dockerRegistryFolder;
		public DockerEndpoints(string DockerRegistryFolder)
		{
			this.dockerRegistryFolder = DockerRegistryFolder;
		}

		protected static void AssertRepositoryPrivilages(IDockerActor Actor, DockerRepository Repository, DockerRepository.RepositoryAction Action, HttpRequest Request)
		{
			bool HasPermission = Repository.HasPermission(Actor, Action);
			if (!HasPermission)
				throw new ForbiddenException(new DockerErrors(DockerErrorCode.DENIED, "Requested access to the resource is denied."), apiHeader);
		}

		protected static async Task WriteToResponse(HttpResponse Response, FileStream File, long Offset, long Count)
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
					await Response.Write(false, Buf, 0, NrBytes);

					Count -= NrBytes;
				}
			}
		}
		public virtual void Dispose()
		{

		}
	}
}
