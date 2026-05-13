# NetMasterAPI — Backend AI Chat Tutor

Backend ASP.NET Core .NET 8 cho app học C#, tích hợp Google Gemini API (miễn phí) cho tính năng AI Chat Tutor.

## Cấu trúc project

```
NetMasterAPI/
├── Controllers/
│   └── ChatController.cs      # POST /api/chat — nhận message, gọi Gemini
├── Services/
│   └── GeminiService.cs      # Wrap Gemini API call, handle errors
├── Models/
│   ├── ChatRequest.cs       # Request model
│   ├── ChatResponse.cs     # Response model
│   ├── GeminiModels.cs      # Gemini API request/response DTOs
│   └── GeminiConfig.cs     # Configuration model
├── Program.cs               # Minimal hosting, DI, CORS
├── appsettings.json        # Cấu hình (API key)
├── appsettings.Development.json
├── Dockerfile              # Multi-stage build cho Render
└── NetMasterAPI.csproj
```

## Yêu cầu

- .NET 8 SDK
- Google Gemini API Key (miễn phí)

## Lấy Gemini API Key

1. Truy cập: https://aistudio.google.com/app/apikey
2. Đăng nhập Google account
3. Click "Create API Key"
4. Copy key

## Chạy local

### 1. Clone và cài đặt

```bash
cd NetMasterAPI
dotnet restore
```

### 2. Đặt API Key

**Cách 1: Environment Variable (recommend)**
```bash
export GEMINI_API_KEY=your_api_key_here
dotnet run
```

**Cách 2: appsettings.json**
```json
{
  "Gemini": {
    "ApiKey": "your_api_key_here"
  }
}
```

### 3. Chạy

```bash
dotnet run
```

Server chạy tại: `http://localhost:10000`

### Test API

```bash
curl -X POST http://localhost:10000/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "messages": [
      {"role": "user", "content": "OOP trong C# là gì?"}
    ],
    "currentTopic": "csharp-oop"
  }'
```

Health check:
```bash
curl http://localhost:10000/health
```

## Deploy miễn phí lên Render.com

### Bước 1: Push code lên GitHub

```bash
cd NetMasterAPI
git init
git add .
git commit -m "Initial commit"
gh repo create netmaster-api --public --push
```

### Bước 2: Deploy trên Render

1. Truy cập https://render.com và đăng nhập
2. Click **New +** → **Web Service**
3. Connect GitHub repo vừa tạo
4. Cấu hình:

| Field | Value |
|-------|-------|
| Name | `netmaster-api` |
| Region | Singapore (hoặc gần nhất) |
| Branch | `main` |
| Root Directory | (để trống) |
| Runtime | `Docker` |
| Instance Type | `Free` |

5. Click **Create Web Service**

### Bước 3: Set Environment Variable

Trong Render dashboard → Environment tab:

| Key | Value |
|-----|-------|
| `GEMINI_API_KEY` | `your_gemini_api_key_here` |
| `PORT` | `10000` |

6. Click **Save Changes**

### Bước 4: Chờ deploy

- Deploy tự động sau khi push lên GitHub
- Build mất ~3-5 phút
- URL sẽ có dạng: `https://netmaster-api.onrender.com`

### Kiểm tra deploy

```bash
curl https://netmaster-api.onrender.com/health
```

## Kết nối Frontend

Trong React/Vite frontend, tạo file `.env`:

```bash
VITE_API_URL=https://netmaster-api.onrender.com
```

Gọi API:

```javascript
const response = await fetch(`${import.meta.env.VITE_API_URL}/api/chat`, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    messages: [
      { role: 'user', content: 'Giải thích Dependency Injection' }
    ],
    currentTopic: 'dependency-injection-lifetimes'
  })
});

const data = await response.json();
console.log(data.reply);
```

## API Reference

### POST /api/chat

**Request:**
```json
{
  "messages": [
    { "role": "user", "content": "..." },
    { "role": "model", "content": "..." }
  ],
  "currentTopic": "csharp-oop",
  "language": "vi"
}
```

**Response:**
```json
{
  "success": true,
  "reply": "OOP trong C# có 4 trụ cột...",
  "tokensUsed": 512,
  "generatedAt": "2026-05-13T10:00:00Z"
}
```

### GET /health

Trả về `{ "status": "healthy", "service": "NetMasterAPI" }`

## Giới hạn

| Limit | Value |
|-------|-------|
| Gemini free tier | 15 requests/phút |
| Gemini free tier | 1 triệu tokens/ngày |
| Render free tier | 750 giờ/tháng |
| Render free tier | Auto-sleep sau 15 phút không có traffic |

> **Lưu ý:** Render free tier sẽ sleep sau 15 phút không có request. Lần request đầu tiên sau sleep sẽ mất ~30-60 giây để wake up (冷启动).

## CORS

Backend cho phép requests từ:
- `http://localhost:3000` (development)
- `https://*.vercel.app` (production Vercel)

Để thêm domain khác, sửa `appsettings.json`:

```json
{
  "AllowedOrigins": [
    "http://localhost:3000",
    "https://your-frontend.vercel.app"
  ]
}
```

## Cấu trúc CI/CD

```
GitHub Push → Render Auto-Deploy → Dockerfile build → Running container
                    ↓
            ENV: GEMINI_API_KEY
```

## Troubleshooting

**Lỗi 500 "API key chưa được cấu hình"**
→ Kiểm tra `GEMINI_API_KEY` environment variable trên Render

**Timeout sau deploy đầu tiên**
→ Bình thường trên free tier, cold start ~30-60s

**CORS lỗi ở production**
→ Kiểm tra `AllowedOrigins` trong `appsettings.json`
