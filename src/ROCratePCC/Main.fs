namespace ROCratePCC


open ARCtrl.ROCrate
open ARCtrl.Json
open Fable.Core

/// object id * url to role
[<AttachMembers>]
type CustomResourceDescriptor(id : string, role : string) =

    member this.Id = id
    member this.Role = role

[<Erase>]
type ResourceDescriptorType =
    | Specification
    | Constraint
    | Guidance
    | Example 
    | Other of CustomResourceDescriptor

    with

    member this.ID =
        match this with
        | Specification -> "#hasSpecification"
        | Constraint -> "#hasConstraint"
        | Guidance -> "#hasGuidance"
        | Example -> "#hasExample"
        | Other(crd) -> crd.Id

    member this.Role =
        match this with
        | Specification -> "http://www.w3.org/ns/dx/prof/role/specification"
        | Constraint -> "http://www.w3.org/ns/dx/prof/role/constraints"
        | Guidance -> "http://www.w3.org/ns/dx/prof/role/guidance"
        | Example -> "http://www.w3.org/ns/dx/prof/role/example"
        | Other(crd) -> crd.Role

[<AttachMembers>]
type Organization(name : string, ?url : string, ?address : string, ?department : Organization, ?orcid) as n =

    inherit LDNode(id = $"https://orcid.org/{orcid}", schemaType = ResizeArray [LDOrganization.schemaType])
    do
        LDDataset.setNameAsString(n, name)
        if url.IsSome then LDDataset.setUrlAsString(n, url.Value)
        if address.IsSome then n.SetProperty("http://schema.org/address", address.Value)
        if department.IsSome then n.SetProperty("http://schema.org/department", department.Value)

[<AttachMembers>]
type Author(orcid : string, ?name : string, ?givenName : string, ?familyName : string, ?email : string, ?affiliation : Organization) as n =

    inherit LDNode(id = $"https://orcid.org/{orcid}", schemaType = ResizeArray [LDPerson.schemaType])

    do
        if name.IsSome then LDDataset.setNameAsString(n, name.Value)
        if affiliation.IsSome then LDPerson.setAffiliation(n, affiliation.Value)
        if givenName.IsSome then LDPerson.setGivenNameAsString(n, givenName.Value)
        if familyName.IsSome then LDPerson.setFamilyNameAsString(n, familyName.Value)
        if email.IsSome then LDPerson.setEmailAsString(n, email.Value)
    

[<AttachMembers>]
type UsedType(iri : string, name : string) as n =

    inherit LDNode(id = iri, schemaType = ResizeArray [LDDefinedTerm.schemaType])

    do LDDataset.setNameAsString(n, name)

[<AttachMembers>]
type License(iri : string, name : string) as n =
    inherit LDNode(id = iri, schemaType = ResizeArray [LDCreativeWork.schemaType])
    do LDDataset.setNameAsString(n, name)

[<AttachMembers>]
type TextualResource(name : string, filePath : string, encodingFormat : string, ?rootDataEntityId) as n =
    inherit LDNode(id = filePath, schemaType = ResizeArray [LDFile.schemaType])

    do 
        LDDataset.setNameAsString(n, name)
        LDFile.setEncodingFormatAsString(n, encodingFormat)
        match rootDataEntityId with
        | Some id -> n.SetProperty(LDFile.about, LDRef(id = id))
        | None -> ()

[<AttachMembers>]
type ResourceDescriptor(textualResources : ResizeArray<TextualResource>, resourceDescriptorType : ResourceDescriptorType) as n =
    inherit LDNode(id = resourceDescriptorType.ID, schemaType = ResizeArray ["http://www.w3.org/ns/dx/prof/ResourceDescriptor"])
    do
        //let artifacts = textualResources |> Seq.map (fun tr -> LDRef(tr.Id)) |> ResizeArray
        //n.SetProperty("http://www.w3.org/ns/dx/prof/hasArtifact", artifacts)
        n.SetProperty("http://www.w3.org/ns/dx/prof/hasRole", LDRef(resourceDescriptorType.Role))
        n.SetProperty("http://www.w3.org/ns/dx/prof/hasArtifact", textualResources)

[<AttachMembers>]
type Specification(textualResources : ResizeArray<TextualResource>) =
    inherit ResourceDescriptor(textualResources = textualResources, resourceDescriptorType = ResourceDescriptorType.Specification)

[<AttachMembers>]
type Constraint(textualResources : ResizeArray<TextualResource>) =
    inherit ResourceDescriptor(textualResources = textualResources, resourceDescriptorType = ResourceDescriptorType.Constraint)

[<AttachMembers>]
type Guidance(textualResources : ResizeArray<TextualResource>) =
    inherit ResourceDescriptor(textualResources = textualResources, resourceDescriptorType = ResourceDescriptorType.Guidance)

[<AttachMembers>]
type Example(textualResources : ResizeArray<TextualResource>) =
    inherit ResourceDescriptor(textualResources = textualResources, resourceDescriptorType = ResourceDescriptorType.Example)

[<AttachMembers>]
type RootDataEntity(id : string, name : string, description : string, license: License, usedTypes : ResizeArray<UsedType>, resourceDescriptors : ResizeArray<ResourceDescriptor>, authors : ResizeArray<Author>, ?dataPublished : System.DateTime, ?publisher : Organization) as n =
    inherit LDNode(id = id, schemaType = ResizeArray [LDDataset.schemaType; "http://www.w3.org/ns/dx/prof/Profile"])
    do
        let textualResources : ResizeArray<LDNode> =
            ResizeArray [
                for rd in resourceDescriptors do
                    yield! rd.GetPropertyNodes("http://www.w3.org/ns/dx/prof/hasArtifact")
            ]
        let hasParts : List<LDNode> = [for tr in textualResources do tr; for ut in usedTypes do ut]
        LDDataset.setLicenseAsCreativeWork(n, license)
        LDDataset.setNameAsString(n, name)
        LDDataset.setDescriptionAsString(n, description)
        n.SetProperty("http://schema.org/author", authors)
        LDDataset.setHasParts(n, ResizeArray hasParts)
        n.SetProperty("http://www.w3.org/ns/dx/prof/hasResource", resourceDescriptors)
        if dataPublished.IsSome then
            LDDataset.setDatePublishedAsDateTime(n, dataPublished.Value)
        else
            LDDataset.setDatePublishedAsDateTime(n, System.DateTime.UtcNow)
        if publisher.IsSome then
            n.SetProperty("http://schema.org/publisher", publisher.Value)

[<AttachMembers>]
type Profile(rootDataEntity : RootDataEntity, ?license : License, ?roCrateSpec : string) as n =
    inherit LDNode(id = "ro-crate-metadata.json", schemaType = ResizeArray [LDCreativeWork.schemaType])
    do
        LDDataset.setAbouts(n, ResizeArray [rootDataEntity :> LDNode])
        if license.IsSome then LDDataset.setLicenseAsCreativeWork(n, license.Value)
        let roCrateSpec = Option.defaultValue "https://w3id.org/ro/crate/1.2" roCrateSpec
        n.SetProperty("http://purl.org/dc/terms/conformsTo", roCrateSpec)

    member this.ToROCrateJsonString(?spaces : int) =
        let context = Context.initV1_2DRAFT()
        this.Compact_InPlace(context, false)
        let graph = this.Flatten()
        graph.SetContext(context)
        graph.ToROCrateJsonString(?spaces = spaces)





