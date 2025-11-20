Title: Organizations
Copyright: /Copyright.md
Master: /Master.md
JavaScript: /Events.js
Script: /Controls/SimpleTable.script
UserVariable: User
Privilege: Admin.Docker
CSS: Style.cssx
Login: /Login.md

===============================================================

# Docker Organizations

===============================================================

{{
	if (exists(Posted) and Posted matches { "BrokerAccountName": PName, "MaxStorage": PMaxStorage }) then
	(
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

| {{Header("Name","OrganizationName")}} | {{Header("Actor Guid","ActorGuid")}} | {{Header("Storage","Storage")}} |
|:----------:|:-------:|:-------:|
{{foreach DockerOranization in Page.Table do
(
	Storage := DockerOranization.GetStorage();
	Used := ToMetricBytes(Storage.UsedStorage);
	Max := ToMetricBytes(Storage.MaxStorage);

	]]| [((MarkdownEncode(UN:=DockerOranization.OrganizationName);))](DockerOrganization.md?guid=((DockerOranization.ObjectId))) [[;
	]]| [((DockerOranization.ActorGuid.ToString();))] [[;
	]]| [((Used)) of ((Max)) used] [[;
	]]|
[[
)
}}
	</div>
</div>