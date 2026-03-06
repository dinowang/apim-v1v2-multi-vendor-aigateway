在 terraform 專案中建置兩個 API Management 服務，一個是 v1 版本，另一個是 v2 版本

以下是 terraform 配置細節：

- 將兩個 API-M 部署在同一個 resource group 中，並且使用相同的 location。
- API-M 要整合一個 application insights 服務，以便後續可以觀察 API-M 的運作狀況。
- v1 版本的 API-M 使用 Developer SKU，而 v2 版本的 API-M 使用 Basic SKU。
- 建制一個 User Assigned Managed Identity，並且分別授權給兩個 API-M 使用。
- API-M 同時也要啟用 System Assigned Managed Identity。｀
- User Assigned Managed Identity 要授權到可以呼叫 OpenAI API 和 Content Safety 的資源上。
- 也建置一個 Microsoft Foundry，在 Foundry 中建立 Deployment (gpt-4o)
- 建置一個 content safety 服務
- 節點之間都走 internet ，不需要 virtual network



