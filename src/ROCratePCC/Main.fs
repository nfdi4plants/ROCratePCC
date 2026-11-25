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
    | ConformsTo
    | Other of CustomResourceDescriptor

    with

    member this.ID =
        match this with
        | Specification -> "#hasSpecification"
        | Constraint -> "#hasConstraint"
        | Guidance -> "#hasGuidance"
        | Example -> "#hasExample"
        | ConformsTo -> "#shouldConformTo"
        | Other(crd) -> crd.Id

    member this.Role =
        match this with
        | Specification -> "http://www.w3.org/ns/dx/prof/role/specification"
        | Constraint -> "http://www.w3.org/ns/dx/prof/role/constraints"
        | Guidance -> "http://www.w3.org/ns/dx/prof/role/guidance"
        | Example -> "http://www.w3.org/ns/dx/prof/role/example"
        | ConformsTo -> "http://purl.org/dc/terms/conformsTo"
        | Other(crd) -> crd.Role

[<AttachMembers>]
type Organization(name : string, ?url : string, ?address : string, ?department : Organization, ?orcid) as n =
    
    inherit LDNode(id = (if url.IsSome then url.Value else $"#{name}"), schemaType = ResizeArray [LDOrganization.schemaType])
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
type UsedType(iri : string, name : string, ?termCode) as n =

    inherit LDNode(id = iri, schemaType = ResizeArray [LDDefinedTerm.schemaType])

    do
        LDDataset.setNameAsString(n, name)
        if termCode.IsSome then n.SetProperty("https://schema.org/termCode", termCode.Value)

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

/// The `id` is optional and defaults to the id of the resource descriptor type (e.g. `#hasExample`).
///
/// Set it to tell several resource descriptors of the same type apart.
[<AttachMembers>]
type ResourceDescriptor(textualResources : ResizeArray<TextualResource>, resourceDescriptorType : ResourceDescriptorType, ?id : string) as n =
    inherit LDNode(id = defaultArg id resourceDescriptorType.ID, schemaType = ResizeArray ["http://www.w3.org/ns/dx/prof/ResourceDescriptor"])
    do
        //let artifacts = textualResources |> Seq.map (fun tr -> LDRef(tr.Id)) |> ResizeArray
        //n.SetProperty("http://www.w3.org/ns/dx/prof/hasArtifact", artifacts)
        n.SetProperty("http://www.w3.org/ns/dx/prof/hasRole", LDRef(resourceDescriptorType.Role))
        n.SetProperty("http://www.w3.org/ns/dx/prof/hasArtifact", textualResources)

[<AttachMembers>]
type Specification(textualResources : ResizeArray<TextualResource>, ?id : string) =
    inherit ResourceDescriptor(textualResources = textualResources, resourceDescriptorType = ResourceDescriptorType.Specification, ?id = id)

[<AttachMembers>]
type Constraint(textualResources : ResizeArray<TextualResource>, ?id : string) =
    inherit ResourceDescriptor(textualResources = textualResources, resourceDescriptorType = ResourceDescriptorType.Constraint, ?id = id)

[<AttachMembers>]
type Guidance(textualResources : ResizeArray<TextualResource>, ?id : string) =
    inherit ResourceDescriptor(textualResources = textualResources, resourceDescriptorType = ResourceDescriptorType.Guidance, ?id = id)

[<AttachMembers>]
type Example(textualResources : ResizeArray<TextualResource>, ?id : string) =
    inherit ResourceDescriptor(textualResources = textualResources, resourceDescriptorType = ResourceDescriptorType.Example, ?id = id)

/// Another profile that this profile relates to, e.g. a profile it builds on.
///
/// Only describes the other profile by `iri`, `name` and `version`. Further details belong to the Profile Crate of that profile.
[<AttachMembers>]
type RelatedProfile(iri : string, name : string, ?version : string) as n =
    inherit LDNode(id = iri, schemaType = ResizeArray [LDCreativeWork.schemaType; "http://www.w3.org/ns/dx/prof/Profile"])
    do
        LDDataset.setNameAsString(n, name)
        if version.IsSome then LDLabProtocol.setVersionAsString(n, version.Value)

/// States that crates of this profile should also conform to the given related profile.
///
/// The `id` is optional and defaults to `#shouldConformTo_` followed by the id of the related profile (e.g. `#shouldConformTo_https://w3id.org/ro/wfrun/process/0.4`).
[<AttachMembers>]
type ConformsTo(profile : RelatedProfile, ?id : string) as n =
    inherit ResourceDescriptor(textualResources = ResizeArray(), resourceDescriptorType = ResourceDescriptorType.ConformsTo, id = defaultArg id $"{ResourceDescriptorType.ConformsTo.ID}_{profile.Id}")
    do
        // The artifact of this descriptor is a profile instead of a file, so it replaces the empty list of textual resources.
        n.SetProperty("http://www.w3.org/ns/dx/prof/hasArtifact", ResizeArray [profile :> LDNode])

module internal ResourceDescriptors =

    /// The id of a node is read-only, so a new id requires a copy of the node.
    let private copyWithId (newId : string) (resourceDescriptor : ResourceDescriptor) : LDNode =
        let copy = LDNode(id = newId, schemaType = ResizeArray resourceDescriptor.SchemaType)
        for propertyName in resourceDescriptor.GetPropertyNames() do
            match resourceDescriptor.TryGetProperty(propertyName) with
            | Some value -> copy.SetProperty(propertyName, value)
            | None -> ()
        copy

    /// Nodes sharing an id are merged into a single node when the crate is flattened.
    ///
    /// To keep them apart, resource descriptors with the same id are replaced by copies whose id carries a running number, e.g. `#hasExample_1` and `#hasExample_2`.
    let withUniqueIds (resourceDescriptors : ResizeArray<ResourceDescriptor>) : ResizeArray<LDNode> =
        let takenIds = resourceDescriptors |> Seq.map (fun rd -> rd.Id) |> ResizeArray
        let isShared (id : string) = takenIds |> Seq.filter (fun takenId -> takenId = id) |> Seq.length > 1
        let rec nextFreeId (id : string) (number : int) =
            let candidate = $"{id}_{number}"
            if takenIds.Contains candidate then
                nextFreeId id (number + 1)
            else
                takenIds.Add candidate
                candidate
        let uniqueDescriptors = ResizeArray<LDNode>()
        for rd in resourceDescriptors do
            if isShared rd.Id then
                uniqueDescriptors.Add(copyWithId (nextFreeId rd.Id 1) rd)
            else
                uniqueDescriptors.Add rd
        uniqueDescriptors

[<AttachMembers>]
type RootDataEntity(id : string, name : string, description : string, license: License, authors : ResizeArray<Author>, ?version : string, ?keywords : ResizeArray<string>, ?usedTypes : ResizeArray<UsedType>, ?resourceDescriptors : ResizeArray<ResourceDescriptor>, ?dataPublished : System.DateTime, ?publisher : Organization) as n =
    inherit LDNode(id = id, schemaType = ResizeArray [LDDataset.schemaType; "http://www.w3.org/ns/dx/prof/Profile"])
    do
        let resourceDescriptors = resourceDescriptors |> Option.map ResourceDescriptors.withUniqueIds
        let textualResources : ResizeArray<LDNode> =
            ResizeArray [
                for rd in resourceDescriptors |> Option.defaultValue (ResizeArray()) do
                    yield! rd.GetPropertyNodes("http://www.w3.org/ns/dx/prof/hasArtifact")
            ]
        let hasParts : List<LDNode> =
            [for tr in textualResources do tr; for ut in usedTypes |> Option.defaultValue (ResizeArray()) do ut]
            |> List.distinct
        LDDataset.setLicenseAsCreativeWork(n, license)
        LDDataset.setNameAsString(n, name)
        LDDataset.setDescriptionAsString(n, description)
        n.SetProperty("http://schema.org/author", authors)
        if keywords.IsSome then n.SetProperty("http://schema.org/keywords", keywords.Value)
        if version.IsSome then LDLabProtocol.setVersionAsString(n, version.Value)
        if hasParts.Length > 0 then LDDataset.setHasParts(n, ResizeArray hasParts)
        if resourceDescriptors.IsSome then n.SetProperty("http://www.w3.org/ns/dx/prof/hasResource", resourceDescriptors.Value)
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
        let roCrateSpec =
            Option.defaultValue "https://w3id.org/ro/crate/1.2" roCrateSpec
            |> LDRef
        rootDataEntity.SetProperty("http://www.w3.org/ns/dx/prof/isProfileOf", roCrateSpec)
        n.SetProperty("http://purl.org/dc/terms/conformsTo", roCrateSpec)

    member this.ToROCrateJsonString(?spaces : int) =
        let context = Context.initV1_2()
        this.Compact_InPlace(context, false)
        let graph = this.Flatten()
        graph.SetContext(context)
        graph.ToROCrateJsonString(?spaces = spaces)





