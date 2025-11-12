# RO-Crate Profile Crate Creator

Polyglot library for creation of RO-Crate Profile Crates according to [the official documentation](https://www.researchobject.org/ro-crate/specification/1.2/profiles.html#profile-crate), and based on the examples of [profile run crate](https://www.researchobject.org/workflow-run-crate/profiles/workflow_run_crate/ro-crate-metadata.json) and [workflow crate](https://about.workflowhub.eu/Workflow-RO-Crate/ro-crate-metadata.json).

| Version | Downloads |
| :--------|-----------:|
|<a href="https://www.nuget.org/packages/ROCratePCC/"><img alt="Nuget" src="https://img.shields.io/nuget/v/ROCratePCC?logo=nuget&color=%234fb3d9"></a>|<a href="https://www.nuget.org/packages/ROCratePCC/"><img alt="Nuget" src="https://img.shields.io/nuget/dt/ROCratePCC?color=%234FB3D9"></a>|
|<a href="https://www.npmjs.com/package/@nfdi4plants/rocratepcc"><img alt="NPM" src="https://img.shields.io/npm/v/%40nfdi4plants/rocratepcc?logo=npm&color=%234fb3d9"></a>|<a href="https://www.npmjs.com/package/@nfdi4plants/rocratepcc"><img alt="NPM" src="https://img.shields.io/npm/dt/%40nfdi4plants%2Frocratepcc?color=%234fb3d9"></a>|
|<a href="https://pypi.org/project/ROCratePCC/"><img alt="PyPI" src="https://img.shields.io/pypi/v/rocratepcc?logo=pypi&color=%234fb3d9"></a>|<a href="https://pypi.org/project/ROCratePCC/"><img alt="PyPI" src="https://img.shields.io/pepy/dt/rocratepcc?color=%234fb3d9"></a>|


## Install

#### .NET

```fsharp
#r "nuget: ROCratePCC"
``` 

```bash
<PackageReference Include="ROCratePCC" Version="1.1.0" />
```

#### JavaScript

```bash
npm i @nfdi4plants/rocratepcc
```

#### Python

```bash
pip install rocratepcc
```


# Documentation

## FSharp

```fsharp
open ROCratePCC

let types : ResizeArray<UsedType> = ResizeArray [
    UsedType(iri = "https://schema.org/CreativeWork", name = "CreativeWork");
    UsedType(iri = "http://www.w3.org/ns/dx/prof/Profile", name = "Profile");
]

let authors : ResizeArray<Author> = ResizeArray [
    Author(orcid = "0000-0002-5526-71389", name = "Florian Wetzels");
    Author(orcid = "0000-0003-1945-6342", name = "Heinrich Lukas Weil");
]

let version = "1.0.0-draft.2"

let id = $"https://github.com/nfdi4plants/isa-ro-crate-profile/tree/{version}/profile"

let name = "ISA RO-Crate Profile"

let description = "An RO-Crate profile for representing ISA data in Research Object Crates (RO-Crates). This profile defines how to represent ISA Investigation, Study, and Assay data using RO-Crate metadata."

let license = License(iri = "https://mit-license.org/", name = "MIT License")

let specifications = ResizeArray[
    TextualResource(
        name = "ISA RO-Crate Profile description",
        filePath = "isa_ro_crate.md",
        encodingFormat = "text/markdown",
        rootDataEntityId = id
    )
]


let resourceDescriptors = ResizeArray [
    Specification(specifications) :> ResourceDescriptor
]

let rootEntity = 
    RootDataEntity(
        id = id,
        name = name,
        description = description,
        license = license,
        usedTypes = types,
        resourceDescriptors = resourceDescriptors,
        authors = ResizeArray authors
    )

let profile = 
    Profile(
        rootEntity,
        license = license
    )

let string = profile.ToROCrateJsonString(spaces = 2)

System.IO.File.WriteAllText("profile/ro-crate-metadata.json", string)
```
## Development

#### Requirements

- [nodejs and npm](https://nodejs.org/en/download)
    - verify with `node --version` (Tested with v18.16.1)
    - verify with `npm --version` (Tested with v9.2.0)
- [.NET SDK](https://dotnet.microsoft.com/en-us/download)
    - verify with `dotnet --version` (Tested with 7.0.306)
- [Python](https://www.python.org/downloads/)
    - verify with `py --version` (Tested with 3.12.2, known to work only for >=3.11)

#### Local Setup

On windows you can use the `setup.cmd` to run the following steps automatically!

1. Setup dotnet tools

   `dotnet tool restore`


2. Install NPM dependencies
   
    `npm install`

3. Setup python environment
    
    `py -m venv .venv`

4. Install [uv](https://docs.astral.sh/uv/) and dependencies

   1. `.\.venv\Scripts\python.exe -m pip install -U pip setuptools`
   2. `.\.venv\Scripts\python.exe -m pip install uv`
   3. `.\.venv\Scripts\python.exe -m uv pip install -r pyproject.toml --group dev`

Verify correct setup with `./build.cmd runtests` ✨