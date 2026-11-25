module ROCratePCC.Tests

open ARCtrl.ROCrate
open ARCtrl
open ARCtrl.Helper
open ARCtrl.Conversion
open ARCtrl.Process
open TestingUtils
open ARCtrl.FileSystem
open Fable.Pyxpecto

let tests_PlaceHolder = 
    testList "PlaceHolder" [
        testCase "Positive" <| fun _ ->
            Expect.isTrue true "dawdaw"
        ptestCase "Negative" <| fun _ ->
            Expect.isFalse true "dawdaw"
    ]



module ResourceDescriptorHelper =

    let hasResource = "http://www.w3.org/ns/dx/prof/hasResource"
    let hasArtifact = "http://www.w3.org/ns/dx/prof/hasArtifact"

    let textualResources (fileNames : string list) =
        ResizeArray [
            for fileName in fileNames do
                TextualResource(name = fileName, filePath = fileName, encodingFormat = "text/markdown")
        ]

    let rootDataEntity (resourceDescriptors : ResourceDescriptor list) =
        RootDataEntity(
            id = "./",
            name = "Test Profile",
            description = "A profile for testing",
            license = ROCratePCC.License(iri = "https://mit-license.org/", name = "MIT License"),
            usedTypes = ResizeArray [],
            resourceDescriptors = ResizeArray resourceDescriptors,
            authors = ResizeArray [],
            dataPublished = System.DateTime(2025, 1, 1)
        )

    let ids (nodes : LDNode seq) = [for node in nodes do node.Id]

    let descriptorIds (root : RootDataEntity) =
        root.GetPropertyNodes(hasResource) |> ids

open ResourceDescriptorHelper

let tests_ResourceDescriptorIds =
    testList "ResourceDescriptorIds" [
        testCase "DefaultIds" <| fun _ ->
            let trs = textualResources ["a.md"]
            Expect.equal (Specification(trs).Id) "#hasSpecification" "Specification"
            Expect.equal (Constraint(trs).Id) "#hasConstraint" "Constraint"
            Expect.equal (Guidance(trs).Id) "#hasGuidance" "Guidance"
            Expect.equal (Example(trs).Id) "#hasExample" "Example"
        testCase "ExplicitIds" <| fun _ ->
            let trs = textualResources ["a.md"]
            Expect.equal (Specification(trs, id = "#spec").Id) "#spec" "Specification"
            Expect.equal (Constraint(trs, id = "#constraint").Id) "#constraint" "Constraint"
            Expect.equal (Guidance(trs, id = "#guidance").Id) "#guidance" "Guidance"
            Expect.equal (Example(trs, id = "#example").Id) "#example" "Example"
        testCase "OneDescriptorPerType_KeepsDefaultIds" <| fun _ ->
            let root =
                rootDataEntity [
                    Specification(textualResources ["spec.md"])
                    Example(textualResources ["example.md"])
                ]
            Expect.sequenceEqual (descriptorIds root) ["#hasSpecification"; "#hasExample"] "ids"
        testCase "SameType_ExplicitIdsAreKept" <| fun _ ->
            let root =
                rootDataEntity [
                    Example(textualResources ["genome.md"], id = "#example-genome")
                    Example(textualResources ["barcode.md"], id = "#example-barcode")
                ]
            Expect.sequenceEqual (descriptorIds root) ["#example-genome"; "#example-barcode"] "ids"
        testCase "SameType_DefaultIdsAreNumbered" <| fun _ ->
            let root =
                rootDataEntity [
                    Specification(textualResources ["spec.md"])
                    Example(textualResources ["genome_1.md"; "genome_2.md"])
                    Example(textualResources ["barcode.md"])
                ]
            let descriptors = root.GetPropertyNodes(hasResource)
            Expect.sequenceEqual (ids descriptors) ["#hasSpecification"; "#hasExample_1"; "#hasExample_2"] "ids"
            Expect.sequenceEqual (descriptors.[1].GetPropertyNodes(hasArtifact) |> ids) ["genome_1.md"; "genome_2.md"] "artifacts of first example"
            Expect.sequenceEqual (descriptors.[2].GetPropertyNodes(hasArtifact) |> ids) ["barcode.md"] "artifacts of second example"
            Expect.isTrue (descriptors.[1].HasType("http://www.w3.org/ns/dx/prof/ResourceDescriptor")) "type of first example"
            let role = descriptors.[2].TryGetPropertyAsSingleton("http://www.w3.org/ns/dx/prof/hasRole")
            Expect.equal role (Some (box (LDRef("http://www.w3.org/ns/dx/prof/role/example")))) "role of second example"
        testCase "SameType_NumberedIdSkipsTakenId" <| fun _ ->
            let root =
                rootDataEntity [
                    Example(textualResources ["a.md"], id = "#hasExample_1")
                    Example(textualResources ["b.md"])
                    Example(textualResources ["c.md"])
                ]
            Expect.sequenceEqual (descriptorIds root) ["#hasExample_1"; "#hasExample_2"; "#hasExample_3"] "ids"
        testCase "SameType_StaysSeparateInFlattenedCrate" <| fun _ ->
            let root =
                rootDataEntity [
                    Example(textualResources ["genome.md"])
                    Example(textualResources ["barcode.md"])
                ]
            let graph = root.Flatten()
            Expect.isNone (graph.TryGetNode "#hasExample") "merged descriptor should not exist"
            let artifactsOf (id : string) = graph.GetNode(id).GetPropertyNodes(hasArtifact, graph = graph) |> ids
            Expect.sequenceEqual (artifactsOf "#hasExample_1") ["genome.md"] "artifacts of first example"
            Expect.sequenceEqual (artifactsOf "#hasExample_2") ["barcode.md"] "artifacts of second example"
            Expect.sequenceEqual (graph.GetNode("./").GetPropertyNodes(hasResource, graph = graph) |> ids) ["#hasExample_1"; "#hasExample_2"] "references of the root data entity"
        testCase "SameType_StaysSeparateInJson" <| fun _ ->
            let root =
                rootDataEntity [
                    Example(textualResources ["genome.md"])
                    Example(textualResources ["barcode.md"])
                ]
            let json = Profile(root).ToROCrateJsonString()
            Expect.isTrue (json.Contains "\"#hasExample_1\"") "first example"
            Expect.isTrue (json.Contains "\"#hasExample_2\"") "second example"
            Expect.isFalse (json.Contains "\"#hasExample\"") "merged example"
    ]

module RelatedProfileHelper =

    let hasPart = "http://schema.org/hasPart"
    let hasRole = "http://www.w3.org/ns/dx/prof/hasRole"

    let processRunCrate () =
        RelatedProfile(iri = "https://w3id.org/ro/wfrun/process/0.4", name = "Process Run Crate profile", version = "0.4.0")

    let workflowROCrate () =
        RelatedProfile(iri = "https://w3id.org/workflowhub/workflow-ro-crate/1.0", name = "Workflow RO-Crate profile", version = "1.0")

open RelatedProfileHelper

let tests_RelatedProfiles =
    testList "RelatedProfiles" [
        testCase "RelatedProfile_Entity" <| fun _ ->
            let profile = processRunCrate ()
            Expect.equal profile.Id "https://w3id.org/ro/wfrun/process/0.4" "id"
            Expect.sequenceEqual profile.SchemaType ["http://schema.org/CreativeWork"; "http://www.w3.org/ns/dx/prof/Profile"] "types"
            Expect.equal (profile.TryGetPropertyAsSingleton("http://schema.org/name")) (Some (box "Process Run Crate profile")) "name"
            Expect.equal (profile.TryGetPropertyAsSingleton("http://schema.org/version")) (Some (box "0.4.0")) "version"
        testCase "RelatedProfile_VersionIsOptional" <| fun _ ->
            let profile = RelatedProfile(iri = "https://w3id.org/ro/wfrun/process/0.4", name = "Process Run Crate profile")
            Expect.isNone (profile.TryGetPropertyAsSingleton("http://schema.org/version")) "version"
        testCase "ConformsTo_Descriptor" <| fun _ ->
            let descriptor = ConformsTo(processRunCrate ())
            Expect.isTrue (descriptor.HasType("http://www.w3.org/ns/dx/prof/ResourceDescriptor")) "type"
            let role = descriptor.TryGetPropertyAsSingleton(hasRole)
            Expect.equal role (Some (box (LDRef("http://purl.org/dc/terms/conformsTo")))) "role"
            Expect.sequenceEqual (descriptor.GetPropertyNodes(hasArtifact) |> ids) ["https://w3id.org/ro/wfrun/process/0.4"] "artifact"
            Expect.sequenceEqual (descriptor.GetPropertyNames() |> Seq.sort) [hasArtifact; hasRole] "no further properties"
        testCase "ConformsTo_DefaultIdFromProfileId" <| fun _ ->
            Expect.equal (ConformsTo(processRunCrate ()).Id) "#shouldConformTo_https://w3id.org/ro/wfrun/process/0.4" "process run crate"
            Expect.equal (ConformsTo(workflowROCrate ()).Id) "#shouldConformTo_https://w3id.org/workflowhub/workflow-ro-crate/1.0" "workflow ro-crate"
        testCase "ConformsTo_ExplicitId" <| fun _ ->
            Expect.equal (ConformsTo(processRunCrate (), id = "#shouldConformToProcess").Id) "#shouldConformToProcess" "id"
        testCase "RootDataEntity_ListsProfilesAsParts" <| fun _ ->
            let root =
                rootDataEntity [
                    Specification(textualResources ["spec.md"])
                    ConformsTo(workflowROCrate ())
                    ConformsTo(processRunCrate ())
                ]
            Expect.sequenceEqual
                (descriptorIds root)
                ["#hasSpecification"; "#shouldConformTo_https://w3id.org/workflowhub/workflow-ro-crate/1.0"; "#shouldConformTo_https://w3id.org/ro/wfrun/process/0.4"]
                "descriptors"
            Expect.sequenceEqual
                (root.GetPropertyNodes(hasPart) |> ids)
                ["spec.md"; "https://w3id.org/workflowhub/workflow-ro-crate/1.0"; "https://w3id.org/ro/wfrun/process/0.4"]
                "parts"
        testCase "RootDataEntity_SameProfileNameStaysSeparate" <| fun _ ->
            let root =
                rootDataEntity [
                    ConformsTo(RelatedProfile(iri = "https://example.org/profile/1.0", name = "Example profile", version = "1.0"))
                    ConformsTo(RelatedProfile(iri = "https://example.org/profile/2.0", name = "Example profile", version = "2.0"))
                ]
            let descriptors = root.GetPropertyNodes(hasResource)
            Expect.sequenceEqual (ids descriptors) ["#shouldConformTo_https://example.org/profile/1.0"; "#shouldConformTo_https://example.org/profile/2.0"] "ids"
            Expect.sequenceEqual (descriptors.[0].GetPropertyNodes(hasArtifact) |> ids) ["https://example.org/profile/1.0"] "artifact of first descriptor"
            Expect.sequenceEqual (descriptors.[1].GetPropertyNodes(hasArtifact) |> ids) ["https://example.org/profile/2.0"] "artifact of second descriptor"
        testCase "FlattenedCrate_ReferencesProfiles" <| fun _ ->
            let root =
                rootDataEntity [
                    Specification(textualResources ["spec.md"])
                    ConformsTo(processRunCrate ())
                ]
            let graph = root.Flatten()
            let profile = graph.GetNode("https://w3id.org/ro/wfrun/process/0.4")
            Expect.isTrue (profile.HasType("http://schema.org/CreativeWork")) "profile is a creative work"
            Expect.isTrue (profile.HasType("http://www.w3.org/ns/dx/prof/Profile")) "profile is a profile"
            let descriptor = graph.GetNode("#shouldConformTo_https://w3id.org/ro/wfrun/process/0.4")
            Expect.sequenceEqual (descriptor.GetPropertyNodes(hasArtifact, graph = graph) |> ids) ["https://w3id.org/ro/wfrun/process/0.4"] "artifact of the descriptor"
            let rootNode = graph.GetNode("./")
            Expect.sequenceEqual (rootNode.GetPropertyNodes(hasPart, graph = graph) |> ids) ["spec.md"; "https://w3id.org/ro/wfrun/process/0.4"] "parts of the root data entity"
            Expect.sequenceEqual (rootNode.GetPropertyNodes(hasResource, graph = graph) |> ids) ["#hasSpecification"; "#shouldConformTo_https://w3id.org/ro/wfrun/process/0.4"] "resources of the root data entity"
        testCase "Json_ContainsProfileAndDescriptor" <| fun _ ->
            let root =
                rootDataEntity [
                    Specification(textualResources ["spec.md"])
                    ConformsTo(processRunCrate ())
                ]
            let json = Profile(root).ToROCrateJsonString()
            Expect.isTrue (json.Contains "\"#shouldConformTo_https://w3id.org/ro/wfrun/process/0.4\"") "descriptor"
            Expect.isTrue (json.Contains "\"http://purl.org/dc/terms/conformsTo\"") "role"
            Expect.isTrue (json.Contains "\"Process Run Crate profile\"") "name of the profile"
            Expect.isTrue (json.Contains "\"0.4.0\"") "version of the profile"
    ]

let all =
    testList "ArcROCrateConversion" [
        tests_PlaceHolder
        tests_ResourceDescriptorIds
        tests_RelatedProfiles
    ]

#if !TESTS_ALL
[<EntryPoint>]
#endif
let main argv = Pyxpecto.runTests [||] all