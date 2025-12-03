using System.IO;
using System.Threading.Tasks;
using TAG.Networking.DockerRegistry.Errors;
using TAG.Networking.DockerRegistry.Model;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Persistence;
using Waher.Persistence.Filters;


namespace TAG.Networking.DockerRegistry.Endpoints
{
	internal class BlobEndpoints : DockerEndpoints
	{
		private BlobStorage blobStorage;

		public BlobEndpoints(string DockerRegistryFolder, ISniffer[] Sniffers, BlobStorage BlobStorage)
			: base(DockerRegistryFolder, Sniffers)
		{
			this.blobStorage = BlobStorage;
		}

		public async Task GET(HttpRequest Request, HttpResponse Response, ByteRangeInterval Interval, DockerActor Actor, DockerRepository Repository, string Reference)
		{
            await AssertRepositoryPrivilages(Actor, Repository, DockerRepository.RepositoryAction.Pull, Request);

            if (!HashDigest.TryParseDigest(Reference, out HashDigest Digest))
				throw new BadRequestException(new DockerErrors(DockerErrorCode.DIGEST_INVALID, "Provided digest did not match uploaded content."), apiHeader);

			FileStream BlobFile = await this.blobStorage.TryGetBlobFile(Digest);

			if (BlobFile == null)
				throw new NotFoundException(new DockerErrors(DockerErrorCode.BLOB_UNKNOWN, "Blob not found."), apiHeader);

			Request.Header.AcceptEncoding = null;

			using (BlobFile)
			{
				long Offset = Interval?.First ?? 0L;
				long Count;

				Count = BlobFile.Length;

				Response.StatusCode = 200;
				Response.SetHeader("Content-Length", BlobFile.Length.ToString());
				Response.SetHeader("Docker-Content-Digest", Digest.ToString());
				Response.SetHeader("Content-Range", Offset.ToString() + "-" +
					(Offset + Count - 1).ToString() + "/" + BlobFile.Length.ToString());

				await WriteToResponse(Response, BlobFile, Offset, Count);
			}

			await Response.SendResponse();
		}

		public async Task DELETE(HttpRequest Request, HttpResponse Response, DockerActor Actor, DockerRepository Repository, string Reference)
		{
			throw new BadRequestException(new DockerErrors(DockerErrorCode.UNAUTHORIZED, "Deleting blobs via API is not allowed"));
		}
	}
}
