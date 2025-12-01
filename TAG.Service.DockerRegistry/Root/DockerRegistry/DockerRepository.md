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
Parameter: guid

{{  
Repo := select top 1 * from DockerRepository where Guid=guid;

if Repo = null then
    NotFound("Repository with object guid " + guid + " does not exist.");

if exists(Posted) then
(
    // delete repository
    if Posted matches { "delete": Bool(PDelete) } and PDelete then
    (
        DeleteObject(Repo);
		TemporaryRedirect("Repositories.md");
    );

    // add whitelist
    if Posted matches { "addToWhitelist": Bool(PAddToWhitelist), "accountName": PAccountName } and PAddToWhitelist then (
        DockerUser := select top 1 * from TAG.Networking.DockerRegistry.Model.DockerUser where AccountName=PAccountName;
        if DockerUser = null then (
            ]] > No user with account name ((PAccountName)) exists [[;
        ) else (
            Repo.CreatePrivileges(DockerUser, true, true);
            ]] +> Whitelisted user ((PAccountName)) [[;
        )
    );

    // remove whitelist
    if Posted matches { "removeFromWhitelist": Bool(PRemoveFromWhitelist), "accountGuid": PAccountGuid } and PRemoveFromWhitelist then (
        DockerUser := select top 1 * from TAG.Networking.DockerRegistry.Model.DockerUser where Guid=PAccountGuid;
        Pri := select top 1 * from TAG.Networking.DockerRegistry.Model.DockerRepositoryPrivilege where ActorGuid=PAccountGuid;
        if DockerUser = null or Pri = null then (
            ]] > No user with guid ((PAccountGuid)) is whitelisted [[;
        ) else (
            DeleteObject(Pri);
            ]] +> Removed user ((DockerUser.AccountName)) whitelist privileges [[;
        )
    );
);
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

============================================================================

## User Whitelist

{{
	Privileges := select * from DockerRepositoryPrivilege where RepositoryGuid=Repo.Guid;

    foreach Pri in Privileges do (
        Account := select top 1 * from TAG.Networking.DockerRegistry.Model.DockerUser where Guid=Pri.ActorGuid;
        if not (Account = null) then (
            ]]
            <form method="POST" class="docker-row">
                <div>
                    <label for="accountGuid">Account name: ((Account.AccountName))</label>
                    <input type="text" name="accountGuid" id="accountGuid" value="((Account.Guid))") hidden>
                </div>
                <input name="removeFromWhitelist" value="true" hidden>
                <button class="negButton">Remove whitelist</button>
            </form>
            [[;
        )
    );
}}

<form method="POST" class="docker-row">
    <div>
        <label for="accountName">Account name</label>
        <input type="text" name="accountName" id="accountName" value="" required>
    </div>
    <input name="addToWhitelist" value="true" hidden>
    <button>Add to whitelist</button>
</form>

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
    ]]| 
[[;
)
}}

============================================================================

<div class="docker-row">
	<form method="POST" onsubmit="DockerAreYouSure(event, 'Are you sure you want to delete this repository and all its images?')">
		<input name="delete" value="true" hidden>
		<button class="negButton">Delete Repository</button>
	</form>
	<div>
		<p><small>Repository GUID: {{Repo.Guid}}</small></p>
	</div>
</div>