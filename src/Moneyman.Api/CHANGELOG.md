# Change Log

All notable changes to this project will be documented in this file. See [versionize](https://github.com/versionize/versionize) for commit guidelines.

<a name="1.2.0"></a>
## [1.2.0](https://www.github.com/halemiles/moneyman-api/releases/tag/v1.2.0) (2023-2-11)

### Features

* Updated DTP service to ingnore non-anticipated for generate monthly and weekly. Updated unit tests ([4fa6426](https://www.github.com/halemiles/moneyman-api/commit/4fa6426aeb825b2c8d0d0120c84f13316ce68740))
* Updated transaction to include IsAnticipated field ([6e02010](https://www.github.com/halemiles/moneyman-api/commit/6e020100e462dccb1593b436c69c757a83e63ffa))

### Bug Fixes

* DTO correction ([0586f44](https://www.github.com/halemiles/moneyman-api/commit/0586f4496782e52b85013e7588238dbfa059d9c9))
* updated snapshots ([fd19a5f](https://www.github.com/halemiles/moneyman-api/commit/fd19a5f1a39905efc3b7ec9619976f0f596103f1))

<a name="1.1.0"></a>
## [1.1.0](https://www.github.com/halemiles/moneyman-api/releases/tag/v1.1.0) (2023-2-9)

### Features

* Added anticipated field and added migration ([09234eb](https://www.github.com/halemiles/moneyman-api/commit/09234eb52d9d526960a75442708c36ac3e8e6cbf))
* Added endpoint to create multuple transactions ([3348a91](https://www.github.com/halemiles/moneyman-api/commit/3348a912ce3ade41481282215ce4d2c113f8f0a8))
* Added initial logging with some basic logging ([187b454](https://www.github.com/halemiles/moneyman-api/commit/187b454e34de528ecef2965b0c33668abaff63fa))
* added logging to transaction service ([6eb46d4](https://www.github.com/halemiles/moneyman-api/commit/6eb46d4b307032514bd828a8d00bc05e26fe942e))
* Added plandate endpoint (#55) ([a00baee](https://www.github.com/halemiles/moneyman-api/commit/a00baeefd341f7186f5643d5d091b4a7d66ded4c))
* Added summary fields to dtp endpoint ([89bb470](https://www.github.com/halemiles/moneyman-api/commit/89bb470ab01a333753539bcf98b83bf4b0a4e88e))
* Upgrade to net 7.0 (#52) ([244ca89](https://www.github.com/halemiles/moneyman-api/commit/244ca89b46b9a922bc70e816f8a1819fa711b8d2))

### Bug Fixes

* Updated bank holidays ([2914ce0](https://www.github.com/halemiles/moneyman-api/commit/2914ce008670d7e85fc8a5686b636b503a2a91ff))

