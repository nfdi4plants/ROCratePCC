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

## Python

```python
from rocratepcc import *

# List all types which are used in this profile
types = [
    UsedType(iri = "https://schema.org/CreativeWork", name = "CreativeWork"),
    UsedType(iri = "http://www.w3.org/ns/dx/prof/Profile", name = "Profile"),
]

# List all authors of this profile
authors = [
    Author(orcid = "0000-0002-5526-71389", name = "Florian Wetzels"),
    Author(orcid = "0000-0003-1945-6342", name = "Heinrich Lukas Weil"),
]

# Version of the profile (semver if possible)
version = "1.0.0-draft.2"

# Identifier of the root data entity of the profile, if url not applicable then use "./"
id = f"https://github.com/nfdi4plants/isa-ro-crate-profile/tree/{version}/profile"

# Name of the profile
name = "ISA RO-Crate Profile"

# Description of the profile
description = "An RO-Crate profile for representing ISA data in Research Object Crates (RO-Crates). This profile defines how to represent ISA Investigation, Study, and Assay data using RO-Crate metadata."

# License by which the profile is published
license = License(iri = "https://mit-license.org/", name = "MIT License")

# List of the textual resources we use as specifications for this profile
specifications = [
    TextualResource(
        name = "ISA RO-Crate Profile description",
        file_path = "isa_ro_crate.md",
        encoding_format = "text/markdown",
        root_data_entity_id = id
    )
]

# List of all the resource descriptors. Here, only the specification is given. Other possible types
resourceDescriptors = [
    Specification(specifications)
    # Example()
    # Guidance()
    # Constraint()
]

# The root data entity of the profile
rootEntity = RootDataEntity(
        id = id,
        name = name,
        description = description,
        license = license,
        used_types = types,
        resource_descriptors = resourceDescriptors,
        authors = authors
    )

# Profile entity
profile = Profile(
        rootEntity,
        license = license
    )

# Convert the profile to a JSON-LD string
string = profile.ToROCrateJsonString(spaces = 2)

# Write the string to a file "ro-crate-metadata.json"
with open("ro-crate-metadata.json", "w", encoding="utf-8") as f:
    f.write(string)
```

resulting in the following `ro-crate-metadata.json`:
```json
{
  "@context": "https://w3id.org/ro/crate/1.2-DRAFT/context",
  "@graph": [
    {
      "@id": "https://mit-license.org/",
      "@type": "CreativeWork",
      "name": "MIT License"
    },
    {
      "@id": "https://orcid.org/0000-0002-5526-71389",
      "@type": "Person",
      "name": "Florian Wetzels"
    },
    {
      "@id": "https://orcid.org/0000-0003-1945-6342",
      "@type": "Person",
      "name": "Heinrich Lukas Weil"
    },
    {
      "@id": "isa_ro_crate.md",
      "@type": "File",
      "name": "ISA RO-Crate Profile description",
      "encodingFormat": "text/markdown",
      "about": {
        "@id": "https://github.com/nfdi4plants/isa-ro-crate-profile/tree/1.0.0-draft.2/profile"
      }
    },
    {
      "@id": "https://schema.org/CreativeWork",
      "@type": "DefinedTerm",
      "name": "CreativeWork"
    },
    {
      "@id": "http://www.w3.org/ns/dx/prof/Profile",
      "@type": "DefinedTerm",
      "name": "Profile"
    },
    {
      "@id": "#hasSpecification",
      "@type": "ResourceDescriptor",
      "hasRole": {
        "@id": "http://www.w3.org/ns/dx/prof/role/specification"
      },
      "hasArtifact": {
        "@id": "isa_ro_crate.md"
      }
    },
    {
      "@id": "https://github.com/nfdi4plants/isa-ro-crate-profile/tree/1.0.0-draft.2/profile",
      "@type": [
        "Dataset",
        "Profile"
      ],
      "license": {
        "@id": "https://mit-license.org/"
      },
      "name": "ISA RO-Crate Profile",
      "description": "An RO-Crate profile for representing ISA data in Research Object Crates (RO-Crates). This profile defines how to represent ISA Investigation, Study, and Assay data using RO-Crate metadata.",
      "author": [
        {
          "@id": "https://orcid.org/0000-0002-5526-71389"
        },
        {
          "@id": "https://orcid.org/0000-0003-1945-6342"
        }
      ],
      "hasPart": [
        {
          "@id": "isa_ro_crate.md"
        },
        {
          "@id": "https://schema.org/CreativeWork"
        },
        {
          "@id": "http://www.w3.org/ns/dx/prof/Profile"
        }
      ],
      "hasResource": {
        "@id": "#hasSpecification"
      }
    },
    {
      "@id": "ro-crate-metadata.json",
      "@type": "CreativeWork",
      "about": {
        "@id": "https://github.com/nfdi4plants/isa-ro-crate-profile/tree/1.0.0-draft.2/profile"
      },
      "license": {
        "@id": "https://mit-license.org/"
      },
      "conformsTo": "https://w3id.org/ro/crate/1.2"
    }
  ]
}
```

### Multiple resource descriptors of the same type

A profile can contain several resource descriptors of the same type, for example two separate examples. Give each of them its own `id` to tell them apart:

```python
example_genome = [
    TextualResource(
        name = "Genome example",
        file_path = "examples/genome.json",
        encoding_format = "application/ld+json",
        root_data_entity_id = id
    )
]

example_barcode = [
    TextualResource(
        name = "Barcode example",
        file_path = "examples/barcode.json",
        encoding_format = "application/ld+json",
        root_data_entity_id = id
    )
]

resourceDescriptors = [
    Specification(specifications),
    Example(example_genome, id = "#example-genome"),
    Example(example_barcode, id = "#example-barcode")
]
```

Each example then gets its own entity in the `ro-crate-metadata.json`:

```json
{
  "@id": "#example-genome",
  "@type": "ResourceDescriptor",
  "hasRole": {
    "@id": "http://www.w3.org/ns/dx/prof/role/example"
  },
  "hasArtifact": {
    "@id": "examples/genome.json"
  }
},
{
  "@id": "#example-barcode",
  "@type": "ResourceDescriptor",
  "hasRole": {
    "@id": "http://www.w3.org/ns/dx/prof/role/example"
  },
  "hasArtifact": {
    "@id": "examples/barcode.json"
  }
}
```

The `id` is optional. Without it, a resource descriptor uses the id of its type (e.g. `#hasExample`). Resource descriptors that would end up with the same id are numbered instead (`#hasExample_1`, `#hasExample_2`).

### Additional properties

Every entity (`RootDataEntity`, `TextualResource`, `Author`, ...) can be given properties that have no parameter of their own. Set them with `SetProperty`, using the full IRI of the property:

```python
from rocratepcc.py.fable_modules.arctrl_rocrate.ldobject import LDRef

example = TextualResource(
    name = "Example crate",
    file_path = "example/ro-crate-metadata.json",
    encoding_format = "application/ld+json",
    root_data_entity_id = id
)

# A single value
example.SetProperty("http://schema.org/description", "A minimal crate that follows this profile")

# A list of values
example.SetProperty("http://schema.org/keywords", ["example", "minimal"])

# A reference to another entity or to a web resource
example.SetProperty("http://purl.org/dc/terms/conformsTo", LDRef(id))

rootEntity.SetProperty("http://schema.org/identifier", "https://doi.org/10.5281/zenodo.1234567")
```

Properties that are part of the RO-Crate context are written with their short name:

```json
{
  "@id": "example/ro-crate-metadata.json",
  "@type": "File",
  "name": "Example crate",
  "encodingFormat": "application/ld+json",
  "about": {
    "@id": "https://github.com/nfdi4plants/isa-ro-crate-profile/tree/1.0.0-draft.2/profile"
  },
  "description": "A minimal crate that follows this profile",
  "keywords": [
    "example",
    "minimal"
  ],
  "conformsTo": {
    "@id": "https://github.com/nfdi4plants/isa-ro-crate-profile/tree/1.0.0-draft.2/profile"
  }
}
```

### Additional entities

An entity the library has no class for can be created as an `LDNode` with an id and a list of types. Attach it to another entity with `SetProperty`:

```python
from rocratepcc.py.fable_modules.arctrl_rocrate.ldobject import LDNode

grant = LDNode(id = "https://example.org/grants/1234", schema_type = ["http://schema.org/Grant"])
grant.SetProperty("http://schema.org/name", "Example grant")

rootEntity.SetProperty("http://schema.org/funding", grant)
```

The new entity is written to the `ro-crate-metadata.json` and referenced by its id:

```json
{
  "@id": "https://example.org/grants/1234",
  "@type": "Grant",
  "name": "Example grant"
},
{
  "@id": "https://github.com/nfdi4plants/isa-ro-crate-profile/tree/1.0.0-draft.2/profile",
  ...
  "funding": {
    "@id": "https://example.org/grants/1234"
  }
}
```

An entity only ends up in the crate if it is attached to another entity.

### More from ARCtrl

`LDNode` and `LDRef` are part of [ARCtrl](https://github.com/nfdi4plants/ARCtrl), the library `rocratepcc` is built on. The package ships its own copy of ARCtrl in `rocratepcc.py.fable_modules`, and there is more in it worth a look. For example, `arctrl_rocrate.LDTypes` has helpers for common types:

```python
from rocratepcc.py.fable_modules.arctrl_rocrate.LDTypes.organization import LDOrganization

funder = LDOrganization.create(name = "Example funder", id = "https://example.org/funder")
grant.SetProperty("http://schema.org/funder", funder)
```

Always import from this copy. A separately installed `arctrl` package has classes of the same name, but `rocratepcc` does not recognise them.

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