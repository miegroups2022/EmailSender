# EMS Meetby API 接口文档 v1.0

**Base URL**: `http://ems.meetby.net/api/v1`  
**协议**: HTTP/HTTPS  
**数据格式**: JSON  
**认证方式**: Bearer Token（登录后获取，放在 Header）

---

## 通用规范

### 请求 Header
```
Content-Type: application/json
Authorization: Bearer {token}   ← 除登录接口外，所有接口必须携带
```

### 统一响应格式
```json
{
  "code":    0,          // 0=成功, 非0=失败
  "message": "ok",       // 提示信息
  "data":    { ... }     // 业务数据，失败时为 null
}
```

### 错误码
| code | 含义 |
|------|------|
| 0    | 成功 |
| 401  | 未登录 / Token 过期 |
| 403  | 无权限 |
| 404  | 资源不存在 |
| 422  | 参数校验失败 |
| 500  | 服务器内部错误 |

---

## 1. 认证模块

### 1.1 登录
```
POST /api/v1/auth/login
```
**请求体**
```json
{
  "username": "admin",
  "password": "admin"
}
```
**响应**
```json
{
  "code": 0,
  "message": "登录成功",
  "data": {
    "token":      "eyJhbGci...",
    "expires_at": "2026-05-12T12:00:00Z",
    "user": {
      "id":       1,
      "username": "admin",
      "nickname": "管理员",
      "email":    "admin@meetby.net"
    }
  }
}
```

### 1.2 退出登录
```
POST /api/v1/auth/logout
```
**响应**
```json
{ "code": 0, "message": "已退出", "data": null }
```

---

## 2. 邮件模板模块

### 2.1 获取模板列表
```
GET /api/v1/templates?page=1&page_size=20&keyword=
```
**响应**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "total": 3,
    "page":  1,
    "list": [
      {
        "id":         1,
        "name":       "外贸开发信-英文版",
        "subject":    "Cooperation Opportunity - {CompanyName}",
        "html_body":  "<p>Dear {ContactName}...</p>",
        "variables":  ["ContactName", "CompanyName", "SenderName"],
        "created_at": "2026-04-25T10:00:00Z",
        "updated_at": "2026-04-25T10:00:00Z"
      }
    ]
  }
}
```

### 2.2 获取单个模板
```
GET /api/v1/templates/{id}
```

### 2.3 创建模板
```
POST /api/v1/templates
```
**请求体**
```json
{
  "name":      "模板名称",
  "subject":   "邮件主题，支持 {变量}",
  "html_body": "<p>HTML正文，支持 {变量}</p>"
}
```

### 2.4 更新模板
```
PUT /api/v1/templates/{id}
```
请求体同创建。

### 2.5 删除模板
```
DELETE /api/v1/templates/{id}
```

---

## 3. 收件人列表模块

### 3.1 获取列表分组
```
GET /api/v1/recipient-groups
```
**响应**
```json
{
  "code": 0,
  "message": "ok",
  "data": [
    { "id": 1, "name": "欧美客户", "count": 120 },
    { "id": 2, "name": "东南亚客户", "count": 85 }
  ]
}
```

### 3.2 获取收件人列表
```
GET /api/v1/recipients?group_id=1&page=1&page_size=50
```
**响应**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "total": 120,
    "page":  1,
    "list": [
      {
        "id":      1,
        "email":   "john.smith@example.com",
        "name":    "John Smith",
        "company": "ABC Trading Co.",
        "country": "USA",
        "status":  "active"
      }
    ]
  }
}
```

### 3.3 导入收件人（CSV）
```
POST /api/v1/recipients/import
Content-Type: multipart/form-data

file:     [CSV文件]
group_id: 1
```

---

## 4. 发送任务模块

### 4.1 提交发送任务
```
POST /api/v1/send-tasks
```
**请求体**
```json
{
  "template_id":   1,
  "group_id":      1,
  "sender_account": "sales@company.com",
  "schedule_at":   null
}
```

### 4.2 获取任务列表
```
GET /api/v1/send-tasks?page=1&page_size=20
```

### 4.3 获取任务详情/进度
```
GET /api/v1/send-tasks/{id}
```
**响应**
```json
{
  "code": 0,
  "data": {
    "id":        101,
    "status":    "running",
    "total":     120,
    "sent":      45,
    "success":   43,
    "failed":    2,
    "started_at":"2026-05-05T10:00:00Z"
  }
}
```

---

## 5. 统计模块

### 5.1 发送统计概览
```
GET /api/v1/stats/overview
```
**响应**
```json
{
  "code": 0,
  "data": {
    "total_sent":    1280,
    "total_success": 1245,
    "total_failed":  35,
    "today_sent":    86,
    "last_send_time":"2026-05-05T08:30:00Z"
  }
}
```