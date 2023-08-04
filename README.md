# NeuronDockerRegistry

Contains a service that can run on a TAG Neuron, that hosts a Docker Image Registry using the Docker Registry API v2.

## Projects

The solution contains the following C# projects:

| Project                         | Framework         | Description |
|:--------------------------------|:------------------|:------------|
| `TAG.Networking.DockerRegistry` | .NET Standard 2.0 | Class library that provides HTTP resources defined by the [Docker Registry API v2](https://docs.docker.com/registry/spec/api/). |
| `TAG.Service.DockerRegistry`    | .NET Standard 2.0 | Service module for the [TAG Neuron](https://lab.tagroot.io/Documentation/Index.md), hosting a Docker Registry on the Neuron where it is installed. |

## Nugets

The following nugets external are used. They faciliate common programming tasks, and
enables the libraries to be hosted on an [IoT Gateway](https://github.com/PeterWaher/IoTGateway).
This includes hosting the bridge on the [TAG Neuron](https://lab.tagroot.io/Documentation/Index.md).
They can also be used standalone.

| Nuget                                                                                              | Description |
|:---------------------------------------------------------------------------------------------------|:------------|
| [Waher.Content](https://www.nuget.org/packages/Waher.Content/)                                     | Pluggable architecture for accessing, encoding and decoding Internet Content. |
| [Waher.Events](https://www.nuget.org/packages/Waher.Events/)                                       | An extensible architecture for event logging in the application. |
| [Waher.IoTGateway](https://www.nuget.org/packages/Waher.IoTGateway/)                               | Contains the [IoT Gateway](https://github.com/PeterWaher/IoTGateway) hosting environment. |
| [Waher.Networking](https://www.nuget.org/packages/Waher.Networking/)                               | Tools for working with communication, including troubleshooting. |
| [Waher.Networking.HTTP](https://www.nuget.org/packages/Waher.Networking.HTTP/)                     | Library for publishing information and services via HTTP. |
| [Waher.Runtime.Cache](https://www.nuget.org/packages/Waher.Runtime.Cache/)                         | Helps the service maintain in-memory caches. |
| [Waher.Runtime.Inventory](https://www.nuget.org/packages/Waher.Runtime.Inventory/)                 | Maintains an inventory of type definitions in the runtime environment, and permits easy instantiation of suitable classes, and inversion of control (IoC). |
| [Waher.Security](https://www.nuget.org/packages/Waher.Security/)                                   | Contains basic cryptography functions. |
| [Waher.Security.JWS](https://www.nuget.org/packages/Waher.Security.JWS/)                           | Mananges JWS signatures of JSON objects. |

## Installable Package

The `TAG.Service.DockerRegistry` project has been made into a package that can be downloaded and installed on any 
[TAG Neuron](https://lab.tagroot.io/Documentation/Index.md).
To create a package, that can be distributed or installed, you begin by creating a *manifest file*. The
`TAG.Service.DockerRegistry` project has a manifest file called `TAG.Service.DockerRegistry.manifest`. It defines the
assemblies and content files included in the package. You then use the `Waher.Utility.Install` and `Waher.Utility.Sign` command-line
tools in the [IoT Gateway](https://github.com/PeterWaher/IoTGateway) repository, to create a package file and cryptographically
sign it for secure distribution across the Neuron network.

The Docker Registry service is published as a package on TAG Neurons. If your Neuron is connected to this network, you can install 
the package using the following information:

| Package information                                                                                                              ||
|:-----------------|:---------------------------------------------------------------------------------------------------------------|
| Package          | `TAG.DockerRegistry.package`                                                                                   |
| Installation key | TBD                                                                                                            |
| More Information | TBD                                                                                                            |

## Building, Compiling & Debugging

The repository assumes you have the [IoT Gateway](https://github.com/PeterWaher/IoTGateway) repository cloned in a folder called
`C:\My Projects\IoT Gateway`, and that this repository is placed in `C:\My Projects\NeuronDockerRegistry`. You can place the
repositories in different folders, but you need to update the build events accordingly. To run the application, you select the
`TAG.Service.DockerRegistry` project as your startup project. It will execute the console version of the
[IoT Gateway](https://github.com/PeterWaher/IoTGateway), and make sure the compiled files of the `NeuronDockerRegistry` solution
is run with it.

## Configuring Docker

Docker runs virtual machines from container images. Docker itself (for instance, if you run 
[Docker Desktop](https://www.docker.com/products/docker-desktop/)), is a command-line tool that runs within the context of a
container. This means that even though it looks like a command-line tool running on your local development machine, it is
actually running on a separate (albeit virtual) machine. This means, that `localhost` refers to itself, not your development machine.
To Docker, your local development machine is called `host.docker.internal`. Furthermore, for Docker to connect to your local
development machine and run against your local Docker Registry service, you need to configure Docker to accept *unsecure*
(i.e. unencrypted) communication with `host.docker.internal`, as you will not have a valid certificate for this domain. You perform
this configuration by editing the *Docker daemon configuration file*. If runnint Docker Desktop, this configuration file is
available under *Settings/Docker Engine*. There you can edit the configuration file, which is a simple JSON file. You need to
add an entry to `insecure-registries` (or add the entry if one is missing), as shown in the example below.

```json
{
  "builder": {
    "features": {
      "buildkit": true
    },
    "gc": {
      "defaultKeepStorage": "20GB",
      "enabled": true
    }
  },
  "experimental": false,
  "insecure-registries": [
    "host.docker.internal:8080"
  ]
}
```

## Configuring Neuron User Credentials

Once the DockerRegistry is installed on a Neuron, you need to configure user access privileges for Docker. A Docker client typically
access a Docker Registry using some form of credentials that give access to upload and download features. On the Neuron, the 
privileges defined are:

* `Docker.Upload` gives a user rights to upload images to the registry.
* `Docker.Download` allows access to existing uploaded images.

Make sure you create a user with the above privileges when testing the registry with a Docker client.

## Example Docker commands

To login to your local registry on your development machine, you issue the following Docker command, where you replace the
`USERNAME` and `PASSWORD` with the corresponding credentials configured in the previous step: (Here, it is assumed your local
developmen Neuron accepts requests on port 8080.)

```
docker login host.docker.internal:8080 -u USERNAME -p PASSWORD
```

To upload an image (in the following example, named `hello-world`) to the registry, issue the following command, once logged in:

```
docker push host.docker.internal:8080/hello-world
```

If you have downloaded an image from another registry, and wish to upload it to your local registry, you will have to tag it for
your local registry first. You do this with the following command:

```
docker tag docker.io/hello-world host.docker.internal:8080/hello-world
```

To pull an image (in the following example, named `hello-world`) from the registry, issue the following command, once logged in:

```
docker pull host.docker.internal:8080/hello-world
```

## Using curl

Many of the API resources cannot be accessed directly by the Docker command-line tool. To access these, you can use an alternative
tool, such as the `curl` command, as is shown in the following examples. Note that the `curl` tool does not run in a separate VM
on your machine, and so you can access your development Neuron directly, using `localhost`.

To check API version: (Empty response=OK, error=Not OK)

```
curl -X GET http://localhost:8080/v2/ -u USERNAME:PASSWORD
```

Fetch the list of repositories in the registry:

```
curl -X GET http://localhost:8080/v2/_catalog -u USERNAME:PASSWORD
```

If you want to use pagination in the request, you can use the `n` and `last` query parameters. If you combine this with `curl`, you
will need to put the URL within quotes, to avoid problems with the command-line parsing:

```
curl -X GET "http://localhost:8080/v2/_catalog?n=MAXCOUNT&last=LASTRESULT" -u USERNAME:PASSWORD
```

For each repository returned by the above command, you can list its tags (which typically represent different versions of an 
image in the repository): (Replace `<repository-name>` with the name of the repository whose tags you want to list.)

```
curl -X GET http://localhost:8080/v2/hello-world/tags/list -u USERNAME:PASSWORD
```

Likewise, `n` and `last` can be used for pagination purposes.

```
curl -X GET "http://localhost:8080/v2/hello-world/tags/list?n=MAXCOUNT&last=LASTRESULT" -u USERNAME:PASSWORD
```
