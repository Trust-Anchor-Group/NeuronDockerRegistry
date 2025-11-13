using System.IO;
using System.Threading.Tasks;
using TAG.Networking.DockerRegistry.Errors;
using TAG.Networking.DockerRegistry.Model;
using Waher.Networking.HTTP;
using Waher.Persistence;
using Waher.Persistence.Filters;


namespace TAG.Networking.DockerRegistry.Endpoints
{
	internal class BlobEndpoints : DockerEndpoints
	{
		private BlobStorage blobStorage;

		public BlobEndpoints(string DockerRegistryFolder, BlobStorage BlobStorage)
			: base(DockerRegistryFolder)
		{
			this.blobStorage = BlobStorage;
		}

		public async Task GET(HttpRequest Request, HttpResponse Response, ByteRangeInterval Interval,  IDockerActor Actor, DockerRepository Repository, string Reference)
		{

            AssertRepositoryPrivilages(Actor, Repository, DockerRepository.RepositoryAction.Pull, Request);

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

		public async Task DELETE(HttpRequest Request, HttpResponse Response, IDockerActor Actor, DockerRepository Repository, string Reference)
		{
            AssertRepositoryPrivilages(Actor, Repository, DockerRepository.RepositoryAction.Delete, Request);

            if (!HashDigest.TryParseDigest(Reference, out HashDigest Digest))
				throw new BadRequestException(new DockerErrors(DockerErrorCode.DIGEST_INVALID, "Invalid BLOB digest reference."), apiHeader);

			DockerBlob blob = await Database.FindFirstIgnoreRest<DockerBlob>(new FilterAnd(
				new FilterFieldEqualTo("Digest", Digest)));

			if (blob is null)
				throw new NotFoundException(new DockerErrors(DockerErrorCode.BLOB_UNKNOWN, "BLOB unknown to registry."), apiHeader);

			await Database.Delete(blob);

			Response.StatusCode = 202;
			Response.StatusMessage = "Accepted";
			Response.ContentLength = 0;
			Response.SetHeader("Docker-Content-Digest", Digest.ToString());
			await Response.SendResponse();
		}
	}
}
