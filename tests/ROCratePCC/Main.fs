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

let all = 
    testList "ArcROCrateConversion" [
        tests_PlaceHolder
        tests_ResourceDescriptorIds
    ]

#if !TESTS_ALL
[<EntryPoint>]
#endif
let main argv = Pyxpecto.runTests [||] all