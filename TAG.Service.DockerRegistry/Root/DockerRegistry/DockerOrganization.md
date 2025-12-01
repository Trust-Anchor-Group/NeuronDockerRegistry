Title: Docker Organization
Copyright: /Copyright.md
Master: /Master.md
JavaScript: /Events.js
JavaScript: Docker.js
Script: /Controls/SimpleTable.script
JavaScript: /TargetBlank.js
UserVariable: User
Privilege: Admin.Docker
Login: /Login.md
CSS: Style.cssx
Parameter: Guid

{{
DockerOrganization := select top 1 * from TAG.Networking.DockerRegistry.Model.DockerOrganization where Guid=Guid;

if DockerOrganization = null then
	NotFound("Organization with Guid " + Guid + " does not exist.");

Storage := DockerOrganization.GetStorageNonBlocking();
if exists(Posted) then
(
	// delete 
	if Posted matches { "delete": Bool(PDelete) } and PDelete = true then (
		DeleteObject(DockerOrganization);
		TemporaryRedirect("DockerOrganizations.md");
	);

	// max storage update
	if Posted matches { "maxStorage": Number(PMaxStorage) } and PMaxStorage > 0 then (
		Storage.MaxStorage:= PMaxStorage;
		UpdateObject(Storage);
	);

	// organization name update
	if Posted matches { "organizationName": String(POrganizationName) } then (
		DockerOrganization.OrganizationName:=POrganizationName;
		UpdateObject(DockerOrganization);
	);

	// auto create update
	if Posted matches { "saveOptions": Bool(PSaveOptions), "autoCreateRepository": Bool(PAutoCreate), "autoCreateRoot": String(PAutoCreateRoot) } and PSaveOptions = true then (
		LowerCaseName := LowerCase(PAutoCreateRoot);

		if not TAG.Networking.DockerRegistry.DockerRepository.IsValidRootName(LowerCaseName) then
			BadRequest("invalid auto create repository root name");

		DockerOrganization.Options.SetOption(ActorOptions.CanAutoCreateRepository, PAutoCreate);
		DockerOrganization.Options.SetOption(ActorOptions.AutoCreateRepositoryRoot, LowerCaseName);
        
        UpdateObject(DockerOrganization);
    );

	// create repository
	if Posted matches { "createRepository": Bool(PCreate), "repositoryName": PRepositoryName, "visibility": PVisibility} then
	(
		DockerCreateRepository(PRepositoryName, DockerOrganization.Guid.ToString(), PVisibility = "private");
		]]+> created repository at ((PRepositoryName)) [[
	)
);
"";
}}

================================================================================================================================

# Docker Organization: {{DockerOrganization.OrganizationName}}

============================================================================

<div class="docker-double">
	<form method="POST" onsubmit="DockerAreYouSure(event, 'Are you sure you want to update the storage limit?')">
		<h2>Update Storage</h2>
		<input name="maxStorage" value="{{Storage.MaxStorage}}">
		<button>Update</button>
	</form>
	<form method="POST" onsubmit="DockerAreYouSure(event, 'Are you sure you want to update the organization name?')">
		<h2>Broker Account</h2>
		<input name="organizationName" value="{{DockerOrganization.OrganizationName}}">
		<button>Update</button>
	</form>
<div>



============================================================================

<h2>
StorageUsed: {{
	Used := ToMetricBytes(Storage.UsedStorage);
    Max := ToMetricBytes(Storage.MaxStorage);
	]] ((Used)) / ((Max)) [[;
}}
</h2>

<br>
{{
PrepareTable(()->
(
	Page.Order:="RepositoryName";
	DockerOrganization.FindOwnedImages();
));

}}

| {{Header("Repository","RepositoryName")}}  | {{Header("Tag", "Tag")}} | {{Header("Size", "Size")}} | {{Header("Digest", "Digest")}} | 
|:----------|:--------|:----------|:----------|
{{foreach Image in Page.Table do
(
	Size:=ToMetricBytes(Image.GetSize());
	]]| ((Image.RepositoryName)) [[;
	]]| ((Image.Tag)) [[;
	]]| ((Size)) [[;
	]]| ((Image.Digest)) [[;
	]]|
[[
)
}}

## Owned repositories

<div class="docker-double">
<form action="" method="POST">
	<input name="createRepository" value=true hidden>
	<p>
		<label for="repositoryName">Repository Name</label>  
		<input type="text" id="RepositoryName" name="repositoryName" autofocus required/>
	</p>
	<p>
		<label for="visibility">Visibility</label>  
		<select name="visibility" id="visibility" required>
		<option value="public" selected>Public</option>
		<option value="private">Private</option>
		</select>
	</p>

	<button>Create repository</button>
</form>

{{
PrepareTable(()->
(
	Page.Order:="Repositories";
	select * from DockerRepository where OwnerGuid = DockerOrganization.Guid
));

}}

| {{Header("Repository","RepositoryName")}} | 
|:----------|
{{foreach Repository in Page.Table do
(
	]]| [((Repository.RepositoryName))](DockerRepository.md?objectId=((Repository.ObjectId.ToString();))) [[;
	]]|
[[
)
}}

</div>



============================================================================

## Options

{{
	ActorOptions := TAG.Networking.DockerRegistry.ActorOptions;
	"";
}}

<form method="POST" class="docker-row">
    <div>
        <label for="autoCreateRepository">Auto Create Repositories</label>
        <select name="autoCreateRepository" id="autoCreateRepository">
            <option value="true" {{if DockerOrganization.Options.IsOptionTrue(ActorOptions.CanAutoCreateRepository) then "selected" else ""}}>Enabled</option>
            <option value="false" {{if not DockerOrganization.Options.IsOptionTrue(ActorOptions.CanAutoCreateRepository) then "selected" else ""}}>Disabled</option>
        </select>
    </div>
    <div>
        <label for="autoCreateRoot">Auto Create Root</label>
        <input type="text" name="autoCreateRoot" id="autoCreateRoot" value="{{DockerOrganization.Options.TryGetOptionWithDefault(ActorOptions.AutoCreateRepositoryRoot, DockerOrganization.OrganizationName +"/")}}">
    </div>
    <input name="saveOptions" value="true" hidden>
    <button>Save</button>
</form>

============================================================================

<div class="docker-row">
	<form method="POST" onsubmit="DockerAreYouSure(event, 'Are you sure you want to delete this organization and all its repositories?')">
		<input name="delete" value="true" hidden>
		<button class="negButton">Delete Docker Organization</button>
	</form>
	<button onclick="OpenPage('DockerStorage.md?Guid={{Storage.Guid}}')">Storage</button>
	<div>
		<p><small>Actor GUID: {{DockerOrganization.Guid}}</small></p>
		<p><small>Sstorage GUID: {{Storage.Guid}}</small></p>
	</div>
</div>
