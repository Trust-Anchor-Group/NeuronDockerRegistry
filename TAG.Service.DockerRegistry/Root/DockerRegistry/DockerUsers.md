Title: Users
Copyright: /Copyright.md
Master: /Master.md
JavaScript: /Events.js
Script: /Controls/SimpleTable.script
UserVariable: User
Privilege: Admin.Docker
Login: /Login.md

================================================================================================================================

Docker Users
===================


## Create user

{{
	if (exists(Posted) and Posted matches { "BrokerAccountName": PName, "MaxStorage": PMaxStorage }) then
	(
		max:=Number(PMaxStorage);
		DockerCreateUser(PName, max);
		]]+> User "((PName))" created with max storage ((ToMetricBytes(max);)). [[
	);
}}

<form action="" method="POST">
	<p>
		<label for="BrokerAccountName">Broker Account Name</label>  
		<input type="text" id="BrokerAccountName" name="BrokerAccountName" autofocus required/>
	</p>
	<p>
		<label for="MaxStorage">MaxStorage (In bytes)</label>  
		<input type="text" id="MaxStorage" name="MaxStorage" placeholder="eg: 3.5e9 for 3.5 GB" autofocus required/>
	</p>

	<button>Create user</button>
</form>


## Users

{{
PrepareTable(()->
(
	Page.Order:="AccountName";
	select * from TAG.Networking.DockerRegistry.Model.DockerUser order by AccountName
));
}}

| {{Header("Name","AccountName")}} | {{Header("Guid","Guid")}} | {{Header("Storage","Storage")}} |
|:----------:|:-------:|:-------:|
{{foreach DockerUser in Page.Table do
(
    Storage := DockerUser.GetStorage();
    Used := ToMetricBytes(Storage.UsedStorage);
    Max := ToMetricBytes(Storage.MaxStorage);

	]]| [((MarkdownEncode(UN:=DockerUser.AccountName);))](DockerUser.md?guid=((DockerUser.Guid))) [[;
	]]| [((DockerUser.Guid.ToString();))] [[;
	]]| [((Used)) of ((Max)) used] [[;
	]]|
[[
)
}}