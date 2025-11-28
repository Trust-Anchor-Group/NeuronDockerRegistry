Title: Docker Repository
Master: /Master.md
JavaScript: /Events.js
JavaScript: /TargetBlank.js
UserVariable: User
Script: /Controls/SimpleTable.script
Privilege: Admin.Communication.Sniffer
Privilege: Admin.Communication.DockerRegistry
Login: /Login.md
CSS: Style.cssx
Parameter: ObjectId

{{  
Repo := select top 1 * from DockerRepository where ObjectId=ObjectId;

if Repo = null then
    NotFound("Repository with object id " + ObjectId + " does not exist.");

if exists(Posted) then
(
    if Posted matches { "delete": Bool(PDelete) } and PDelete = true then
    (
        DeleteObject(Repo);
		TemporaryRedirect("Repositories.md");
    );
);

"";
}}

================================================================================================================================
	
# Docker Repository: {{Repo.RepositoryName}}



==================================================

## Owner:
{{
    Owner := select top 1 * from DockerActor where Guid=Repo.OwnerGuid;
    if Owner = null then (
        ]] > No owner [[;
    ) else (
        if Owner is TAG.Networking.DockerRegistry.Model.DockerUser then
            ]] User: [((Owner.AccountName))](DockerUser.md?guid=((Owner.Guid))) [[
        else 
            ]] Organization: [((Owner.OrganizationName))](DockerOrganization.md?guid=((Owner.Guid))) [[
    );
}}


==================================================

## Images

{{
PrepareTable(()->(
    Page.Order:="Tag";
    select * from DockerImage where RepositoryName=Repo.RepositoryName order by Tag;
));
}}

| {{Header("Name", "Name")}} | {{Header("Tag", "Tag")}} | {{Header("Digest", "Digest")}} | {{Header("Size", "Size")}} |
|---|---|---|---|
{{
foreach Image in Page.Table do
(
    Size:=Image.GetSize();
    ]]| ((Image.RepositoryName)):((Image.Tag)) [[;
    ]]| ((Image.Tag)) [[;
    ]]| ((Image.Digest)) [[;
    ]]| ((ToMetricBytes(Size);)) [[;
    ]]| [[;
);
}}

============================================================================

<div class="docker-row">
	<form method="POST" onsubmit="DockerAreYouSure(event, 'Are you sure you want to delete this repository and all its images?')">
		<input name="delete" value="true" hidden>
		<button class="negButton">Delete Repository</button>
	</form>
	<button onclick="OpenPage('DockerStorage.md?Guid={{Storage.Guid}}')">Storage</button>
	<div>
		<p><small>Repository GUID: {{Repo.Guid}}</small></p>
	</div>
</div>