using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TAG.Networking.DockerRegistry.Model;
using Waher.Networking.HTTP;
using Waher.Script;


namespace TAG.Networking.DockerRegistry.Endpoints
{
    internal class TagsEndpoints : DockerEndpoints
    {
        public TagsEndpoints(string DockerRegistryFolder)
             : base(DockerRegistryFolder)
        {

        }


        public async Task GET(HttpRequest Request, HttpResponse Response, IDockerActor Actor, DockerRepository Repository, string Reference)
        {
            AssertRepositoryPrivilages(Actor, Repository, DockerRepository.RepositoryAction.Pull, Request);

            object Result;

            if (RegistryServerV2.IsPaginated(Request, out bool HasLast, out Variables Pagination, new Variable("Name", Repository.RepositoryName)))
            {
                if (HasLast)
                    Result = await Expression.EvalAsync("select top N distinct Tag from 'DockerImages' where Image=Name and Tag>Last", Pagination);
                else
                    Result = await Expression.EvalAsync("select top N distinct Tag from 'DockerImages' where Image=Name", Pagination);

                RegistryServerV2.SetLastHeader(Response, "/v2/" + Repository.RepositoryName + "/tags/list?", Result, Pagination);
            }
            else
            {
                if (HasLast)
                    Result = await Expression.EvalAsync("select distinct Tag from 'DockerImages' where Image=Name and Tag>Last", Pagination);
                else
                    Result = await Expression.EvalAsync("select distinct Tag from 'DockerImages' where Image=Name", Pagination);
            }

            Response.StatusCode = 200;
            await Response.Return(new Dictionary<string, object>()
            {
                { "name", Repository.RepositoryName },
                { "tags", Result }
            });
        }

        public Task PUT(DockerRepository Repository, string Reference)
        {
            return Task.CompletedTask;
        }

        public Task DELETE(DockerRepository Repository, string Reference)
        {
            return Task.CompletedTask;
        }
    }
}
