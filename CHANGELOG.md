# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [v3.2.3] - 2026-04-08
### :bug: Bug Fixes
- [`b8147b7`](https://github.com/itachi1706/SPCCSHelpers/commit/b8147b7608db77daca2921a0280e651d6d9c036b) - If metrics disabled, clear queue *(commit by [@itachi1706](https://github.com/itachi1706))*


## [v3.2.2] - 2026-03-20
### :bug: Bug Fixes
- [`acbf031`](https://github.com/itachi1706/SPCCSHelpers/commit/acbf031f2792741c863999c47182c7b4357e4f54) - Update DPoP implementation to pass the string value instead *(commit by [@itachi1706](https://github.com/itachi1706))*


## [v3.2.1] - 2026-03-20
### :sparkles: New Features
- [`162ffb0`](https://github.com/itachi1706/SPCCSHelpers/commit/162ffb0a3d406c75d6856d818de312ec40057972) - Add support for DPoP *(commit by [@itachi1706](https://github.com/itachi1706))*


## [v3.2.0] - 2026-03-17
### :boom: BREAKING CHANGES
- due to [`60bca70`](https://github.com/itachi1706/SPCCSHelpers/commit/60bca70410fb61aca60c54f36886a3b1ea2b519a) - Retired support for .NET 6 *(commit by [@itachi1706](https://github.com/itachi1706))*:

  Retired support for .NET 6


### :sparkles: New Features
- [`0ef8979`](https://github.com/itachi1706/SPCCSHelpers/commit/0ef89798bb9d75ae87686c74e9f392aa147642c8) - Implemented helper functions for calling Cloudwatch Metrics if enabled *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`6341b0d`](https://github.com/itachi1706/SPCCSHelpers/commit/6341b0dc8217c3a1d47a02b458a7ff36582e3036) - Added services to handle metrics *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`60bca70`](https://github.com/itachi1706/SPCCSHelpers/commit/60bca70410fb61aca60c54f36886a3b1ea2b519a) - Retired support for .NET 6 *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`c5e02bc`](https://github.com/itachi1706/SPCCSHelpers/commit/c5e02bcbc52f13977704bfa505f9db5a35cd196a) - Added option to always add instance id to dimensions *(commit by [@itachi1706](https://github.com/itachi1706))*

### :recycle: Refactors
- [`aede24a`](https://github.com/itachi1706/SPCCSHelpers/commit/aede24af463308027092c949122962f6d0ef8009) - Rename package *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`a0f025d`](https://github.com/itachi1706/SPCCSHelpers/commit/a0f025dc89e867ebfb06f2d1dad50a1abd9e217a) - Rename references *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`d8f6dc0`](https://github.com/itachi1706/SPCCSHelpers/commit/d8f6dc0bc3d1be912e378621ba2ee5bbb5422093) - Formatting *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`6d0057a`](https://github.com/itachi1706/SPCCSHelpers/commit/6d0057a1725384f52ded14334652b1cf7b416b2b) - Formatting *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`a2c4a87`](https://github.com/itachi1706/SPCCSHelpers/commit/a2c4a87a772cddd552716a21ada7931584a66ff6) - Formatting *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`cdb17dc`](https://github.com/itachi1706/SPCCSHelpers/commit/cdb17dce01fed89ec87dff09e82c2776232c302d) - Move to file scoped namespace *(commit by [@itachi1706](https://github.com/itachi1706))*

### :white_check_mark: Tests
- [`c3a453d`](https://github.com/itachi1706/SPCCSHelpers/commit/c3a453d79fd9873d6510ad1faffb145c649c2916) - Bump tests to .NET 10 *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`a9d31ac`](https://github.com/itachi1706/SPCCSHelpers/commit/a9d31acfee9cf35b64115f9e283a0452d5a11545) - Add back test cert *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`b7f6e9b`](https://github.com/itachi1706/SPCCSHelpers/commit/b7f6e9b09b871e6a8188deedf17ab1835f0d0e4c) - Added test cases for new metrics handling *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`3ea0d2b`](https://github.com/itachi1706/SPCCSHelpers/commit/3ea0d2be14bcfe78b0ea004d004ce3cc39ba9119) - Added AWSHelper test cases and reduce flake *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`fbb15e0`](https://github.com/itachi1706/SPCCSHelpers/commit/fbb15e06e3994bdad1fd5430128f89a2072cb71f) - Add more test cases *(commit by [@itachi1706](https://github.com/itachi1706))*

### :wrench: Chores
- [`19b589f`](https://github.com/itachi1706/SPCCSHelpers/commit/19b589fcb22f55b87618c305d4417d4751fd42d7) - **deps**: Bump NuGet/setup-nuget from 2.0.1 to 2.0.2 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`38f6cb0`](https://github.com/itachi1706/SPCCSHelpers/commit/38f6cb02180d4168fc73db93dccc40b5339722d3) - Use DI injected AWSHelperV3 *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`5b7ea42`](https://github.com/itachi1706/SPCCSHelpers/commit/5b7ea42c1e51d54f51faa8f9df96d071ef57a4ad) - Spelling *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`5ed0e4d`](https://github.com/itachi1706/SPCCSHelpers/commit/5ed0e4da7886101e8de6c06a2e83a96aac0f5de4) - Allow client reuse for cloudwatch client *(commit by [@itachi1706](https://github.com/itachi1706))*


## [v3.1.1] - 2026-03-12
### :sparkles: New Features
- [`cc62577`](https://github.com/itachi1706/SPCCSHelpers/commit/cc625771c686a6449814d206123b642f91932438) - Added status code logging and bump version *(commit by [@itachi1706](https://github.com/itachi1706))*

### :wrench: Chores
- [`41ccef8`](https://github.com/itachi1706/SPCCSHelpers/commit/41ccef81fe3377af8b172d2c383a95a43c787baf) - **deps**: bump actions/setup-dotnet from 4 to 5 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`050a29c`](https://github.com/itachi1706/SPCCSHelpers/commit/050a29ce61d8bd27f8b67748adf6f1d319e6c2ea) - **deps**: bump actions/cache from 4 to 5 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`1b0c1f3`](https://github.com/itachi1706/SPCCSHelpers/commit/1b0c1f38063db178d78e963287830d7d376825c1) - **deps**: bump actions/checkout from 5 to 6 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*


## [v3.1.0] - 2026-02-25
### :sparkles: New Features
- [`00e87b0`](https://github.com/itachi1706/SPCCSHelpers/commit/00e87b0471bf329b47c216cb3cf32f0a497b8112) - Migrated Web call function to common *(commit by [@itachi1706](https://github.com/itachi1706))*

### :white_check_mark: Tests
- [`317db77`](https://github.com/itachi1706/SPCCSHelpers/commit/317db77cf9667036609566f429dcffa202df4ccb) - Added test cases *(commit by [@itachi1706](https://github.com/itachi1706))*

### :wrench: Chores
- [`2d7b716`](https://github.com/itachi1706/SPCCSHelpers/commit/2d7b716ea1167380b2dd8cce5532cb0d14df4fbf) - **deps**: bump actions/setup-java from 4 to 5 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`1cc9262`](https://github.com/itachi1706/SPCCSHelpers/commit/1cc9262012c8771b9baba929043b9748706a7980) - **deps**: bump github/codeql-action from 3 to 4 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`37e81e5`](https://github.com/itachi1706/SPCCSHelpers/commit/37e81e54a15a8614b9c6ede1f35561f57f7a5bb8) - **deps**: bump actions/checkout from 4 to 5 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`5484c3e`](https://github.com/itachi1706/SPCCSHelpers/commit/5484c3eba5a994b63e0baedd4e99328a3578fb4a) - **deps**: bump stefanzweifel/git-auto-commit-action from 5 to 7 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`49b9e62`](https://github.com/itachi1706/SPCCSHelpers/commit/49b9e62fb0c757243c55544f19b4982c5a2bfa26) - Bump version *(commit by [@itachi1706](https://github.com/itachi1706))*


## [v3.0.2] - 2026-01-27
### :wrench: Chores
- [`2cf0432`](https://github.com/itachi1706/SPCCSHelpers/commit/2cf0432a96f16b39b257ef55cbc36526cd9866a7) - Added README *(commit by [@xiaopang254](https://github.com/xiaopang254))*
- [`fb06362`](https://github.com/itachi1706/SPCCSHelpers/commit/fb0636201efa0d3a76c52663cd54f5f4526f77d9) - upgrade libraries *(commit by [@xiaopang254](https://github.com/xiaopang254))*
- [`53166d6`](https://github.com/itachi1706/SPCCSHelpers/commit/53166d69cc701e22eb604ca65d9c07699624669e) - update readme, upgrade version number *(commit by [@xiaopang254](https://github.com/xiaopang254))*


## [v3.0.1] - 2025-10-08
### :sparkles: New Features
- [`a4f0084`](https://github.com/itachi1706/SPCCSHelpers/commit/a4f0084ad01e64865a661abe809a2072f94e520e) - Dependency updates and reintroduce .net 6 *(commit by [@itachi1706](https://github.com/itachi1706))*

### :wrench: Chores
- [`5df6fc9`](https://github.com/itachi1706/SPCCSHelpers/commit/5df6fc949f2a9804aa5d59ecf1a4a6a18339d431) - Disable debug *(commit by [@itachi1706](https://github.com/itachi1706))*


## [v3.0.0] - 2025-07-01
### :sparkles: New Features
- [`729565f`](https://github.com/itachi1706/SPCCSHelpers/commit/729565f9f183eab1ccea559987136d8c121a20a6) - upgrade for dotnet 8 *(commit by [@xiaopang254](https://github.com/xiaopang254))*

### :wrench: Chores
- [`eb1a40b`](https://github.com/itachi1706/SPCCSHelpers/commit/eb1a40b4d253fd4851420e0656e0b38905bcc3fe) - **deps**: bump NuGet/setup-nuget from 2.0.0 to 2.0.1 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`e569a81`](https://github.com/itachi1706/SPCCSHelpers/commit/e569a81cef8cc8fe0c9a44aaab054634b3bb7b0a) - upgrade github action dotnet *(commit by [@xiaopang254](https://github.com/xiaopang254))*


## [v2.2.0] - 2024-08-21
### :recycle: Refactors
- [`7ca0648`](https://github.com/itachi1706/SPCCSHelpers/commit/7ca06482bb20673fd2b43af1804c1683e19120f2) - Sonar fixes *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`4541293`](https://github.com/itachi1706/SPCCSHelpers/commit/45412935cb2520442f9afa5906b598a832c13954) - Sonar fixes *(commit by [@itachi1706](https://github.com/itachi1706))*

### :white_check_mark: Tests
- [`7bb358c`](https://github.com/itachi1706/SPCCSHelpers/commit/7bb358cbc58caf449c2dc79878702a68bfb3384a) - Added test solution to project *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`c0d94ee`](https://github.com/itachi1706/SPCCSHelpers/commit/c0d94ee777648eb0720a4463ec71f135c5750287) - Added test cases *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`1d2a080`](https://github.com/itachi1706/SPCCSHelpers/commit/1d2a0807cdad2e1615e4335a392e19ab459fae27) - Add more test cases *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`1a4cade`](https://github.com/itachi1706/SPCCSHelpers/commit/1a4cadee49a366954171434c5863a6591ff92931) - Finish adding S3 tests *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`ce61f49`](https://github.com/itachi1706/SPCCSHelpers/commit/ce61f49b6a0f67fc3ea0e6f823735b650b5c6ce3) - AP-SOUTHEAST-1 added *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`b47c5ac`](https://github.com/itachi1706/SPCCSHelpers/commit/b47c5ac4ef37abd28c07ed0492ca0daf2e13c244) - Added test environments *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`884559c`](https://github.com/itachi1706/SPCCSHelpers/commit/884559cd84b32e5e7809993f03f322ed23796f4c) - Added more test cases *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`17692f9`](https://github.com/itachi1706/SPCCSHelpers/commit/17692f932aaee1ca22182d057f3fb2040aba51be) - Added test cases for v2 *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`c0f84c5`](https://github.com/itachi1706/SPCCSHelpers/commit/c0f84c51a19b35cfe153c559d0c5ca948ebbfdcc) - Update AWS test cases *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`f4acf25`](https://github.com/itachi1706/SPCCSHelpers/commit/f4acf25faa68f7ddee93876cb64c14196f612800) - Add more test cases *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`3af70cb`](https://github.com/itachi1706/SPCCSHelpers/commit/3af70cbee2cf571185de71033c5e09f2f5b364f6) - Finish up env helper tests *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`01c3d14`](https://github.com/itachi1706/SPCCSHelpers/commit/01c3d14f20586f8ef126bfb22cbea86558173216) - More test cases *(commit by [@itachi1706](https://github.com/itachi1706))*

### :wrench: Chores
- [`8846427`](https://github.com/itachi1706/SPCCSHelpers/commit/8846427c312a1615b75d3fedc273de331ced12ff) - **deps**: bump stefanzweifel/git-auto-commit-action from 4 to 5 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`b0bda7c`](https://github.com/itachi1706/SPCCSHelpers/commit/b0bda7c76962cfa2efbb4d6759e84364a1d33def) - **deps**: bump actions/checkout from 3 to 4 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`7ede461`](https://github.com/itachi1706/SPCCSHelpers/commit/7ede4614f4762000fdb1428b438433666d3c482a) - **deps**: bump actions/setup-java from 3 to 4 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`0977e7a`](https://github.com/itachi1706/SPCCSHelpers/commit/0977e7a398f8f4b9298935a85cb2b0f67d0ce0f2) - **deps**: bump actions/setup-dotnet from 3 to 4 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`ba45b95`](https://github.com/itachi1706/SPCCSHelpers/commit/ba45b951bc8fafe2ccf5b6e0f0ee96132ce67157) - **deps**: bump github/codeql-action from 2 to 3 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`aff93e4`](https://github.com/itachi1706/SPCCSHelpers/commit/aff93e4ec565ee4933502a3eb4ad1dee354e6119) - **deps**: bump actions/cache from 3 to 4 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`47ae0ad`](https://github.com/itachi1706/SPCCSHelpers/commit/47ae0adfeca08da387e1d7d52c506df1e9b5a779) - **deps**: bump NuGet/setup-nuget from 1.2.0 to 2.0.0 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`65238ed`](https://github.com/itachi1706/SPCCSHelpers/commit/65238edc0c5be36d69be4eb6901054a4fdfbe1be) - **release**: 2.2.0 Dependency Update *(commit by [@itachi1706](https://github.com/itachi1706))*


## [v2.1.2] - 2023-06-07
### :wrench: Chores
- [`117b9cf`](https://github.com/itachi1706/SPCCSHelpers/commit/117b9cf64f187b3049fe5f352445cec595c1512e) - Update dependabot *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`b0a2997`](https://github.com/itachi1706/SPCCSHelpers/commit/b0a299711614276f7bf34973e2d532ad3a983f44) - **deps**: bump actions/checkout from 2 to 3 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`c8142b0`](https://github.com/itachi1706/SPCCSHelpers/commit/c8142b08c61dbcd97b67d4af29590ec1083fbbe1) - **deps**: bump NuGet/setup-nuget from 1.1.1 to 1.2.0 *(commit by [@dependabot[bot]](https://github.com/apps/dependabot))*
- [`712e59d`](https://github.com/itachi1706/SPCCSHelpers/commit/712e59dce2f5f46f0cf0306d55ba16a637346602) - **release**: v2.1.2 *(commit by [@itachi1706](https://github.com/itachi1706))*


## [v2.1.1] - 2023-03-10
### :wrench: Chores
- [`b67d573`](https://github.com/itachi1706/SPCCSHelpers/commit/b67d573d0ce4737f9871b085a4afe0d9cf82ccc0) - **deps**: Update AWS Dependencies *(commit by [@itachi1706](https://github.com/itachi1706))*
- [`a27a91c`](https://github.com/itachi1706/SPCCSHelpers/commit/a27a91c5c8eeb9a0d4f11dcdcda71e74d3aa4c73) - **release**: v2.1.1 *(commit by [@itachi1706](https://github.com/itachi1706))*


[v2.1.1]: https://github.com/itachi1706/SPCCSHelpers/compare/v2.1.0...v2.1.1
[v2.1.2]: https://github.com/itachi1706/SPCCSHelpers/compare/v2.1.1...v2.1.2
[v2.2.0]: https://github.com/itachi1706/SPCCSHelpers/compare/v2.1.2...v2.2.0
[v3.0.0]: https://github.com/itachi1706/SPCCSHelpers/compare/v2.2.0...v3.0.0
[v3.0.1]: https://github.com/itachi1706/SPCCSHelpers/compare/v3.0.0...v3.0.1
[v3.0.2]: https://github.com/itachi1706/SPCCSHelpers/compare/v3.0.1...v3.0.2
[v3.1.0]: https://github.com/itachi1706/SPCCSHelpers/compare/v3.0.2...v3.1.0
[v3.1.1]: https://github.com/itachi1706/SPCCSHelpers/compare/v3.1.0...v3.1.1
[v3.2.0]: https://github.com/itachi1706/SPCCSHelpers/compare/v3.1.1...v3.2.0
[v3.2.1]: https://github.com/itachi1706/SPCCSHelpers/compare/v3.2.0...v3.2.1
[v3.2.2]: https://github.com/itachi1706/SPCCSHelpers/compare/v3.2.1...v3.2.2
[v3.2.3]: https://github.com/itachi1706/SPCCSHelpers/compare/v3.2.2...v3.2.3
