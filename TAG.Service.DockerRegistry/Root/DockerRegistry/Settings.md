Title: Docker Registry
Author: Peter Waher
Copyright: /Copyright.md
Master: /Master.md
JavaScript: /Events.js
Script: /Controls/SimpleTable.script
JavaScript: /Sniffers/Sniffer.js
UserVariable: User
Privilege: Admin.Docker
Login: /Login.md
Parameter: Purge

{{
    if (exists(Posted) and exists(Posted.Purge)) then (

        delete from DockerImage;
        delete from DockerBlob;
        delete from DockerRepository;
        delete from DockerUser;
        delete from DockerOrganization;
        delete from DockerStorage;

        "Purged";
    );
}}

================================================================================================================================

Docker Registry Settings
===================

<a class="button" href="DockerUsers.md">Users</a>
<a class="button" href="Repositories.md">Repositories</a>

<form method="post">
    <input type="hidden" name="Purge" value="True"/>
    <button>Purge</button>
</form>

<button type="button" class="posButton"{{
if User.HasPrivilege("Admin.Communication.DockerRegistry") and User.HasPrivilege("Admin.Communication.Sniffer") then
	" onclick=\"OpenSniffer('Sniffer.md')\""
else
	" disabled"
}}>Sniffer</button>