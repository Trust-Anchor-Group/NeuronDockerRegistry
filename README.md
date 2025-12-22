# Neuron Docker Registry
 
This service is a Docker registry implementation that runs as a TAG Neuron service. It allows users to upload, download and manage Docker and OCI images on the Neuron.

For details on the HTTP API, refer to the official [Docker Registry HTTP API v2 specification](https://distribution.github.io/distribution/spec/api/#:~:text=The%20Docker%20Registry%20HTTP%20API,images%20and%20enable%20their%20distribution.).

---

## Quick Start

### Prerequisites

- A running Neuron with the Docker Registry service deployed.
- A Broker Account with appropriate Docker-related privileges.
- Docker CLI installed on your machine.

### Basic workflow



Replace `<neuron-host>` and placeholders with your actual values.

1. Go to `https://<neuron-host>/DockerRegistry/DockerUsers.md` and create a docker user

2. Open the user and create a repository to upload images to.

3. Log in to the registry:

   ```bash
   docker login <neuron-host>
   ```

4. Tag an image to push to a repository under an organization or user:

   ```bash
   docker tag myapp:latest <neuron-host>/<REPOSITORY_NAME>:latest
   ```

5. Push the image:

   ```bash
   docker push <neuron-host>/<REPOSITORY_NAME>:latest
   ```

6. Pull the image later:

   ```bash
   docker pull <neuron-host>/<REPOSITORY_NAME>:latest
   ```

---

## Core Concepts

- **Docker Actor**  
  An actor is the logical owner of registries and repositories. All registries are created by an actor. Actors:
  - Can own repositories or be whitelisted on repositories.
  - Have a maximum unique BLOB storage limit.
  - Represent either an organization or an individual user.

  When performing a request on the Neuron, an actor is chosen based on information from the Broker Account authenticating via HTTP Basic Auth, following the [Docker v2 specification](https://distribution.github.io/distribution/spec/api/#:~:text=The%20Docker%20Registry%20HTTP%20API,images%20and%20enable%20their%20distribution.).

- **Docker Organization**  
  A Docker organization is an actor where every Broker Account or administrative user whose Legal Identity `ORGNAME` property matches the organization’s `Organization Name` (as shown on the `/DockerOrganization.md` page) has access to that actor.

- **Docker User**  
  A Docker user is an actor that is accessible by the Broker Account whose user name matches the `Broker Account` property on the `/DockerUser.md` page.

- **Used Storage**  
  Used storage is calculated as the total size of all unique BLOBs that your images reference. If images A, B and C reference the same BLOBs, the used storage is the same as if you only had image A.

---

## Registry Administrative Management

> The built-in dashboard lives under:
> `https://<neuron>/DockerRegistry/`
> Use the **Settings**, **Users**, **Organizations** and **Repositories** pages to manage actors, storage and repositories.

### General

- Users with global administrative privileges `Administrator.Docker.*` can manage all users and organizations.
- Users with specific administrative privileges `DockerRegistry.*` can manage only the resources owned by the actors available to them.
- Only `Administrator.Docker.*` can edit max storage and create new organizations and users.

### Users (`/DockerUsers.md`)

- Any administrator with `Administrator.Docker.Create` can:
  - Create Docker users.
  - See all existing users.
- If you have `DockerRegistry.Read`, you can see any user that you own.

### Organizations (`/DockerOrganizations.md`)

- Any administrator with `Administrator.Docker.Create` can:
  - Create organizations.
  - See all existing organizations.
- If you have `DockerRegistry.Read`, you can see any organization you belong to.

### Auto-create Repositories (`/DockerOrganization.md`)

- Auto-create is **disabled by default**.
- If enabled in the dashboard, then when an API action is performed on a repository that does not exist, the registry **may** auto-create it.
- If you are **not** an administrator:
  - You can only set `Auto Create Root` to a value that begins with `<ORGNAME>/`.
- Only users with `Administrator.Docker.Update` can set `Auto Create Root` to something that does **not** start with `<ORGNAME>/`.
- `Auto Create Root` is the required prefix for the repository name in order for auto-create to work.

Example:

- `Auto Create Root = <ORGNAME>/`
- `docker push <neuron>/<ORGNAME>/myservice:latest` → repository can be auto-created.
- `docker push <neuron>/otherorg/myservice:latest` → auto-create will not apply.

### Repositories (`/DockerRepositories.md`)

- If you have global administrative privileges:
  - You can list **all** repositories.
  - You can create new repositories.
- If you only have specific administrative privileges:
  - You can list all repositories you have access to.
- When creating a repository:
  - `Owner GUID` must match the GUID of an existing actor.
  - You can find the GUID of an actor at the bottom of their dashboard page.

### Other Maintenance

- Use the **Force Clean** button on `/Settings.md` to:
  - Remove repositories whose owners no longer exist.
  - Delete all BLOBs that are not referenced by any image on the registry.
- This clean-up is scheduled to run every 24 hours.

---

## Access Control Overview

- **Repository visibility**
  - **Public repositories** are readable by anyone.
  - **Private repositories** are readable only by:
    - Whitelisted users.
    - The repository owner (either a user or an organization).

- **Privileges**
  - Dashboards use the `DockerRegistry.*` and `Administrator.DockerRegistry.*` verbs:
    - `Read`
    - `Update`
    - `Create`
    - `Delete`

  In general:
  - `DockerRegistry.*` → manage/read resources for actors you are associated with.
  - `Administrator.Docker.*` → global admin operations.

---

## How the Neuron Chooses the Effective Actor

The registry maps an authenticated request to one or two possible `DockerActor` identities (a user and/or an organization) and selects a single **effective actor** to perform the operation.

The logic implemented in `RegistryServerV2.cs` can be summarized as follows.

### 1. Actor Sources

From the Broker Account performing the request, using its Legal Identity:

- It gets the **organization actor** from the `OrgName` property.
- It gets the **user actor** from the `UserName` property.

Either, both, or neither may resolve to actual actors.

### 2. When the repository already exists (common read/write/delete)

- If **no** candidate actors are found:
  - The request is rejected (access denied).
- If **exactly one** candidate actor is found:
  - That actor is chosen as the effective actor.
- If **multiple** candidate actors are found (both user and organization):
  - The owner of the repository is preferred:
    - The actor whose `Guid` equals `Repository.OwnerGuid` is selected.
  - If none of the candidate actors is the owner:
    - The first actor in the list is used.

### 3. When the repository does not exist yet (create / auto-create)

- Start from the candidate actors (user and/or organization).
- Filter to only those actors that have `CanAutoCreateRepository` set.
- For each remaining actor (in order):
  - Use a **repository-level semaphore** to avoid races between concurrent creates.
  - Re-check whether the repository already exists (to handle concurrent creation).
  - If the repository now exists:
    - Return that repository paired with the actor.
  - Otherwise:
    - Attempt to auto-create the repository **if** the requested repository name starts with the actor’s `AutoCreateRepositoryRoot`.
    - If auto-create succeeds:
      - Return the newly created repository paired with the actor.
- If no candidate actor can create the repository, or none is authorized to auto-create:
  - No effective actor is chosen, and the caller will treat this as not found / forbidden.

---

## Key Consequences and Behaviour Notes

- You can toggle BLOB backups in `/Settings/Backup.md`
- When both a user and an organization are available, operations are performed as the **repository owner** where possible (via `Repository.OwnerGuid`).
- Auto-creation is gated both by:
  - The actor’s `CanAutoCreateRepository` option.
  - The repository name starting with the actor’s `AutoCreateRepositoryRoot`.
- Actors without `CanAutoCreateRepository` are **never** considered for repository auto-creation.
- The `DELETE /v2/<name>/blobs/<digest>` endpoint is **disabled** in this implementation:
  - Manual deletion of individual BLOBs via the Docker API is not allowed.
  - BLOB cleanup is handled via reference tracking and the scheduled/forced clean-up mechanisms described above.
