Title: Repositories
Copyright: /Copyright.md
Master: /Master.md
JavaScript: /Events.js
Script: /Controls/SimpleTable.script
CSS: Style.cssx
UserVariable: User
Privilege: DockerRegistry
Login: /Login.md

================================================================================================================================

Docker Repositories
===================

{{
	if (exists(Posted) and Posted matches { "RepositoryName": PRepositoryName, "OwnerGuid": POwnerGuid, "Visibility": PVisibility }) then
	(
		Authorize(User, "Administrator.Docker.Create");
		DockerCreateRepository(PRepositoryName, POwnerGuid, PVisibility = "private");
		]]+> Created repository at ((PRepositoryName)) [[
	);
}}

{{
	if User.HasPrivilege("Administrator.DockerRegistry.Create") then (
		]]
## Create repository

		<form action="" method="POST">
			<p>
				<label for="RepositoryName">Repository Name</label>  
				<input type="text" id="RepositoryName" name="RepositoryName" autofocus required/>
			</p>
			<p>
				<label for="OwnerGuid">Owner GUID</label>  
				<input type="text" id="OwnerGuid" name="OwnerGuid" autofocus required/>
			</p>

			<p>
				<label for="Visibility">Visibility</label>  
				<select name="Visibility" id="Visibility" required>
				<option value="public" selected>Public</option>
				<option value="private">Private</option>
				</select>
			</p>

			<button>Create repository</button>
		</form>
		[[;
	);
}}


## Repositories

{{
PrepareTable(()->
(
	Page.Order:="RepositoryName";
	select * from TAG.Networking.DockerRegistry.Model.DockerRepository order by RepositoryName
));
}}

| {{Header("Name","RepositoryName")}} | {{Header("Owner Guid","OwnerGuid")}} | Owner |
|:----------|:-------:|-----------|
{{foreach Repository in Page.Table do
(
	if (DockerDashboardHasPermisions(Repository, "DockerRegistry.Read")) then (
		Owner := select top 1 * from TAG.Networking.DockerRegistry.Model.DockerActor where Guid=Repository.OwnerGuid;

		]]| [((MarkdownEncode(UN:=Repository.RepositoryName);))](DockerRepository.md?guid=((Repository.Guid))) [[;
		]]| [((MarkdownEncode(EM:=Repository.OwnerGuid);))] [[;
		if Owner is TAG.Networking.DockerRegistry.Model.DockerUser then(
		]]| [((Owner.AccountName))](DockerUser.md?guid=((Owner.Guid))) [[;
		)
		else(
		]]| [((Owner.OrganizationName))](DockerOrganization.md?guid=((Owner.Guid))) [[;
		);
		]]|
[[
	);
)
}}

