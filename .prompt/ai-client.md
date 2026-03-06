使用 C# .NET 10 寫一個 Console App

- 專案路徑 ./src/console-app
- C# Namespace 為 `ApimAi.ConsoleApp` 
- 透過預先組態好(appsettings.json)的 API-M  AI Gateway 呼叫 OpenAI API，並且將回應的結果印出來
- OpenAI API 端點會有兩個（都是 API-M 的 AI Gateway），可按熱鍵切換 (Ctrl+1 / Ctrl+2) 接下來要使用的 API 端點
- 使用者可選擇要使用的後端服務端點  (F3 切換)
  - 可選 OpenAI, Claude
  - 預設為 OpenAI
  - 後端服務端點對應
    OpenAI:
      Model/Deployment ID: gpt-4o
      HTTP Header: 
        X-UseBackend: openai-ai-endpoint
      Query String 
        api-version: 2025-03-01-preview
    Claude:
      Model/Deployment ID: claude-opus-4-6 
      HTTP Header: 
        X-UseBackend: claude-ai-endpoint
    Gemini:
      Model/Deployment ID: gemini-2.5-flash-lite
      HTTP Header: 
        X-UseBackend: gemini-ai-endpoint
- 使用者可選擇 Content Safety 的 Threshold (F4 切換)
  - 等級分為 0, 1, 2, 4, 5, 6, 7 級
  - 預設為 7 級
  - Content Safety 的等級會透過 HTTP Header `X-CS-Threshold` 傳遞給 API-M 的 AI Gateway
- 使用者介面由使用者提供 prompt，並且可以選擇要呼叫的 API 端點（v1 或 v2）
  - 若可行，實作按上下鍵尋找之前使用過的 prompt 歷史紀錄，並且可以編輯後再次送出
- 呼叫 ChatCompletions API，使用 stream mode，並且將回應的內容印出來
  - 統計 stream 回應的 token 數量，並且在回應結束後印出總 token 數
  - 統計收到的 trunk 數量，並且在回應結束後印出總 trunk 數以及平均的 trunk 大小
- 使用者介面採用 TUI 的方式設計，相關設定顯示於固定在畫面最底部的一列上，該列使用合適的色彩作為底色
  - 使用熱鍵切換時不會造成畫面換行