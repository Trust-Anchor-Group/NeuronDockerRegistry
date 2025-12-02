Title: Organizations
Copyright: /Copyright.md
Master: /Master.md
JavaScript: /Events.js
Script: /Controls/SimpleTable.script
UserVariable: User
Privilege: DockerRegistry.Read
CSS: Style.cssx
Login: /Login.md

===============================================================

# Docker Organizations

===============================================================

{{
	if (exists(Posted) and Posted matches { "BrokerAccountName": PName, "MaxStorage": PMaxStorage }) then
	(
		Authorize(User, "Administrator.Docker.Create");
		max:=Number(PMaxStorage);
		DockerCreateOrganization(PName, max);
		]]+> Organization "((PName))" created with max storage ((ToMetricBytes(max);)). [[
	);
}}

<div class="docker-double">
	<form id="create-actor" action="" method="POST">

## Create Organization
	
		<p>
			<label for="BrokerAccountName">Organization Name (match with organization name on ids)</label>  
			<input type="text" id="BrokerAccountName" name="BrokerAccountName" autofocus required/>
		</p>
		<p>
			<label for="MaxStorage">MaxStorage (In bytes)</label>  
			<input type="text" id="MaxStorage" name="MaxStorage" placeholder="eg: 3.5e9 for 3.5 GB" autofocus required/>
		</p>
		<button>Create</button>
	</form>
	<div>

## Docker Organizations

{{
PrepareTable(()->
(
	Page.Order:="OrganizationName";
	select * from TAG.Networking.DockerRegistry.Model.DockerOrganization order by OrganizationName
));
}}

| {{Header("Name","OrganizationName")}} | {{Header("Guid","Guid")}} | {{Header("Storage","Storage")}} |
|:----------:|:-------:|:-------:|
{{foreach Org in Page.Table do
(
	if (DockerDashboardHasPermisions(Org, "DockerRegistry.Read")) then (
		Storage := Org.GetStorageNonBlocking();
		Used := ToMetricBytes(Storage.UsedStorage);
		Max := ToMetricBytes(Storage.MaxStorage);

		]]| [((MarkdownEncode(UN:=Org.OrganizationName);))](DockerOrganization.md?guid=((Org.Guid))) [[;
		]]| [((Org.ToString();))] [[;
		]]| [((Used)) of ((Max)) used] [[;
		]]|
[[
	);
)
}}
	</div>
</div>