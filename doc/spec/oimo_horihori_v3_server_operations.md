# OIMO HORIHORI v3.0.0 Server 構築・運用手順書

## 1. 概要

OIMO HORIHORI v3.0.0 のオンライン機能を、さくらのVPS上の ASP.NET Core Server で提供する。

本番構成は以下。

```text
Blazor WebAssembly Client
https://mkunori.github.io/OimoHorihori/

        ↓ HTTPS API / CORS

https://mkunori.com/oimo-api/

        ↓

Nginx
HTTPS終端 / Reverse Proxy
/oimo-api/ を除去

        ↓

ASP.NET Core / Kestrel
http://127.0.0.1:5000

        ↓

SQLite
/var/lib/oimohorihori/oimohorihori.db
```

Server 本体は以下に配置する。

```text
/opt/oimohorihori/
```

秘密設定は以下。

```text
/etc/oimohorihori/
```

永続データは以下。

```text
/var/lib/oimohorihori/
```

バックアップは以下。

```text
/var/backups/oimohorihori/
```

---

# 2. VPS環境

確認済み環境：

```text
OS        : Ubuntu 24.04 LTS
CPU       : x86_64
.NET      : ASP.NET Core Runtime 10
Nginx     : 1.24.0
Server user : oimohorihori
Deploy user : deploy
```

.NET SDK はVPSには入れない。

```text
Windows開発PC
    ↓ dotnet publish
Linux用成果物
    ↓ scp
VPS
    ↓ ASP.NET Core Runtimeで実行
```

VPSは実行環境としてのみ利用する。

---

# 3. ディレクトリ構成

```text
/opt/oimohorihori/
    ASP.NET Core Server本体

/var/lib/oimohorihori/
    oimohorihori.db
    oimohorihori.db-wal
    oimohorihori.db-shm

/etc/oimohorihori/
    oimohorihori.env

/var/backups/oimohorihori/
    oimohorihori-YYYYMMDD-HHMMSS.db

/usr/local/sbin/
    backup-oimohorihori.sh
    deploy-oimohorihori.sh
```

---

# 4. Server実行ユーザー

専用ユーザー `oimohorihori` を使用する。

```bash
sudo useradd \
  --system \
  --home-dir /var/lib/oimohorihori \
  --shell /usr/sbin/nologin \
  --no-create-home \
  oimohorihori
```

確認：

```bash
getent passwd oimohorihori
```

Serverプロセスはこのユーザーで実行し、SSHログインには使用しない。

---

# 5. .NET Runtime

Ubuntu 24.04 に ASP.NET Core Runtime 10 を導入する。

```bash
sudo apt update
sudo apt install -y aspnetcore-runtime-10.0
```

確認：

```bash
dotnet --info
dotnet --list-runtimes
```

期待する例：

```text
Microsoft.AspNetCore.App 10.x
Microsoft.NETCore.App 10.x
```

SDKは不要。

---

# 6. ServerのDBパス設定

ローカル開発では従来どおりプロジェクト配下のDBを使用し、本番では環境変数で永続領域を指定する。

`Program.cs`：

```csharp
string dbPath = builder.Configuration["Database:Path"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "oimohorihori.db");

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlite($"Data Source={dbPath}"));
```

本番では：

```text
Database__Path=/var/lib/oimohorihori/oimohorihori.db
```

をsystemdから渡す。

ASP.NET Coreでは環境変数の `__` は設定キーの `:` として扱われる。

```text
Database__Path
    ↓
Database:Path
```

---

# 7. EF Core Migration

起動時に未適用Migrationを自動適用する。

`Program.cs`：

```csharp
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
```

`EnsureCreated()` は使用しない。

---

# 8. Reverse Proxy対応

NginxでHTTPS終端するため、ASP.NET Core側ではForwarded Headersを有効化する。

```csharp
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
```

```csharp
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownProxies.Add(IPAddress.Loopback);
});
```

middleware順序：

```csharp
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseCors(ClientCorsPolicy);
```

Nginxは127.0.0.1からKestrelへ接続するため、Loopbackのみを信頼する。

---

# 9. CORS

本番ClientのOriginはパスを含めず：

```text
https://mkunori.github.io
```

を許可する。

Client URL：

```text
https://mkunori.github.io/OimoHorihori/
```

でも、CORS Originは：

```text
https://mkunori.github.io
```

となる。

---

# 10. Client API URL

Development：

```json
{
  "ApiBaseUrl": "https://localhost:7233/"
}
```

Production：

```json
{
  "ApiBaseUrl": "https://mkunori.com/oimo-api/"
}
```

Clientでは：

```csharp
string? apiBaseUrl = builder.Configuration["ApiBaseUrl"];

if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    throw new InvalidOperationException("ApiBaseUrl が設定されていません。");
}

builder.Services.AddScoped(
    _ => new HttpClient
    {
        BaseAddress = new Uri(apiBaseUrl)
    });
```

この状態で：

```csharp
http.GetAsync("api/auth/me")
```

は本番では：

```text
https://mkunori.com/oimo-api/api/auth/me
```

へアクセスする。

---

# 11. Server publish

Windowsのリポジトリルートで実行。

```powershell
dotnet publish .\src\OimoHorihori.Server\OimoHorihori.Server.csproj `
  -c Release `
  -r linux-x64 `
  --self-contained false `
  -o .\publish\server
```

圧縮：

```powershell
Remove-Item .\oimohorihori-server.tar.gz -ErrorAction SilentlyContinue

tar -czf oimohorihori-server.tar.gz -C .\publish\server .
```

転送：

```powershell
scp -i $env:USERPROFILE\.ssh\sakura_vps `
  .\oimohorihori-server.tar.gz `
  deploy@<VPS_HOST>:~/
```

SSH：

```powershell
ssh -i $env:USERPROFILE\.ssh\sakura_vps deploy@<VPS_HOST>
```

---

# 12. Server配置先

アプリ本体：

```bash
sudo mkdir -p /opt/oimohorihori
sudo chown root:root /opt/oimohorihori
sudo chmod 755 /opt/oimohorihori
```

DB領域：

```bash
sudo mkdir -p /var/lib/oimohorihori
sudo chown oimohorihori:oimohorihori /var/lib/oimohorihori
sudo chmod 750 /var/lib/oimohorihori
```

アプリ本体はroot所有とし、Serverプロセスからは基本的に読み取りのみとする。

---

# 13. systemd

ファイル：

```text
/etc/systemd/system/oimohorihori.service
```

内容：

```ini
[Unit]
Description=OIMO HORIHORI ASP.NET Core Server
After=network.target

[Service]
Type=simple
User=oimohorihori
Group=oimohorihori
WorkingDirectory=/opt/oimohorihori

ExecStart=/usr/bin/dotnet /opt/oimohorihori/OimoHorihori.Server.dll

Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="ASPNETCORE_URLS=http://127.0.0.1:5000"
Environment="Database__Path=/var/lib/oimohorihori/oimohorihori.db"
EnvironmentFile=/etc/oimohorihori/oimohorihori.env

Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
```

反映：

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now oimohorihori
```

確認：

```bash
systemctl status oimohorihori --no-pager -l
sudo ss -lntp | grep ':5000'
```

期待値：

```text
127.0.0.1:5000
```

`0.0.0.0:5000` にはしない。

---

# 14. Admin Key

Admin APIは：

```text
X-Admin-Key
```

ヘッダーを使用する。

Server側：

```csharp
configuration["Admin:Key"]
```

で取得するため、本番環境変数は：

```text
Admin__Key
```

とする。

秘密設定ファイル：

```text
/etc/oimohorihori/oimohorihori.env
```

例：

```text
Admin__Key=<ランダムな秘密値>
```

生成例：

```bash
openssl rand -hex 32
```

権限：

```bash
sudo chown root:root /etc/oimohorihori/oimohorihori.env
sudo chmod 600 /etc/oimohorihori/oimohorihori.env
```

Admin KeyはGitHub、Client、appsettings.jsonへコミットしない。

---

# 15. Nginx

既存設定：

```text
/etc/nginx/sites-available/apps
```

`mkunori.com` は既にCertbotでHTTPS化済み。

追加する設定：

```nginx
location ^~ /oimo-api/ {
    proxy_pass http://127.0.0.1:5000/;

    proxy_http_version 1.1;

    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
}
```

重要：

```nginx
proxy_pass http://127.0.0.1:5000/;
```

末尾 `/` を付ける。

これにより：

```text
外部:
/oimo-api/api/auth/login

内部:
/api/auth/login
```

となる。

変更前にはバックアップ：

```bash
sudo cp /etc/nginx/sites-available/apps \
  /etc/nginx/sites-available/apps.bak-YYYYMMDD
```

構文確認：

```bash
sudo nginx -t
```

成功後のみ：

```bash
sudo systemctl reload nginx
```

---

# 16. HTTPS / CORS確認

Kestrel直接：

```bash
curl -i http://127.0.0.1:5000/api/auth/me
```

未ログインなら：

```text
401 Unauthorized
```

Nginx経由：

```bash
curl -i https://mkunori.com/oimo-api/api/auth/me
```

こちらも未ログインなら：

```text
401 Unauthorized
```

CORS：

```bash
curl -i -X OPTIONS \
  -H "Origin: https://mkunori.github.io" \
  -H "Access-Control-Request-Method: GET" \
  https://mkunori.com/oimo-api/api/auth/me
```

期待値：

```text
204 No Content
Access-Control-Allow-Origin: https://mkunori.github.io
```

---

# 17. 管理API

## ユーザー一覧

```bash
sudo bash -c '
source /etc/oimohorihori/oimohorihori.env
curl -s \
  -H "X-Admin-Key: ${Admin__Key}" \
  https://mkunori.com/oimo-api/api/admin/users
'
```

キーなしでは：

```text
401 Unauthorized
```

キーありでは：

```text
200 OK
```

## PINリセット

```text
PUT /api/admin/users/{userId}/pin
```

Body：

```json
{
  "newPin": "1234"
}
```

PINリセット時には該当ユーザーのAuthSessionを削除するため、全端末ログアウトとなる。

## アカウント無効化

```text
PUT /api/admin/users/{userId}/disabled
```

無効化：

```json
{
  "isDisabled": true
}
```

再有効化：

```json
{
  "isDisabled": false
}
```

---

# 18. アカウント削除

API：

```text
DELETE /api/account
```

削除時：

```text
最新Server Saveを取得
↓
GameSave削除
↓
AuthSession全削除
↓
UserAccount.IsDeleted = true
↓
EquippedTitle解除
↓
最新SaveをClientへ返却
↓
Client localStorageへ保存
```

確認済み仕様：

- 削除後もローカルゲームとして継続可能
- 削除済みユーザー名は再利用不可
- ランキング記録は残る

---

# 19. SQLite

本番DB：

```text
/var/lib/oimohorihori/oimohorihori.db
```

稼働中は：

```text
oimohorihori.db
oimohorihori.db-wal
oimohorihori.db-shm
```

が存在する。

WAL運用中に `.db` のみを単純コピーしてバックアップしない。

---

# 20. SQLite CLI

導入：

```bash
sudo apt update
sudo apt install -y sqlite3
```

確認：

```bash
sqlite3 --version
```

---

# 21. バックアップスクリプト

ファイル：

```text
/usr/local/sbin/backup-oimohorihori.sh
```

内容：

```bash
#!/usr/bin/env bash

set -euo pipefail
umask 077

DB="/var/lib/oimohorihori/oimohorihori.db"
BACKUP_DIR="/var/backups/oimohorihori"
TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
BACKUP="${BACKUP_DIR}/oimohorihori-${TIMESTAMP}.db"

mkdir -p "$BACKUP_DIR"

echo "Creating backup: $BACKUP"

sqlite3 "$DB" ".backup '$BACKUP'"

RESULT="$(sqlite3 "$BACKUP" "PRAGMA integrity_check;")"

if [ "$RESULT" != "ok" ]; then
    echo "Integrity check failed: $RESULT" >&2
    rm -f "$BACKUP"
    exit 1
fi

echo "Integrity check: ok"

find "$BACKUP_DIR" \
    -type f \
    -name 'oimohorihori-*.db' \
    -mtime +14 \
    -delete

echo "Backup completed."
```

権限：

```bash
sudo chown root:root /usr/local/sbin/backup-oimohorihori.sh
sudo chmod 700 /usr/local/sbin/backup-oimohorihori.sh
```

手動実行：

```bash
sudo /usr/local/sbin/backup-oimohorihori.sh
```

---

# 22. 毎日自動バックアップ

Service：

```text
/etc/systemd/system/oimohorihori-backup.service
```

```ini
[Unit]
Description=Backup OIMO HORIHORI SQLite database

[Service]
Type=oneshot
ExecStart=/usr/local/sbin/backup-oimohorihori.sh
```

Timer：

```text
/etc/systemd/system/oimohorihori-backup.timer
```

```ini
[Unit]
Description=Daily OIMO HORIHORI database backup

[Timer]
OnCalendar=*-*-* 03:30:00
Persistent=true

[Install]
WantedBy=timers.target
```

反映：

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now oimohorihori-backup.timer
```

確認：

```bash
systemctl status oimohorihori-backup.timer --no-pager -l
systemctl list-timers --all | grep oimohorihori
```

バックアップログ：

```bash
sudo journalctl -u oimohorihori-backup.service -n 50 --no-pager
```

保持期間：

```text
14日
```

---

# 23. バックアップ整合性確認

最新バックアップ：

```bash
sudo bash -c '
BACKUP=$(ls -1t /var/backups/oimohorihori/*.db | head -1)
sqlite3 "$BACKUP" "PRAGMA integrity_check;"
'
```

期待値：

```text
ok
```

---

# 24. 復元テスト

本番DBを壊さず `/tmp` へ復元する。

```bash
sudo rm -f /tmp/oimohorihori-restore-test.db
```

```bash
sudo bash -c '
BACKUP=$(ls -1t /var/backups/oimohorihori/oimohorihori-*.db | head -1)
sqlite3 /tmp/oimohorihori-restore-test.db ".restore '\''$BACKUP'\''"
'
```

整合性：

```bash
sudo sqlite3 /tmp/oimohorihori-restore-test.db \
  "PRAGMA integrity_check;"
```

テーブル：

```bash
sudo sqlite3 /tmp/oimohorihori-restore-test.db ".tables"
```

Migration履歴：

```bash
sudo sqlite3 /tmp/oimohorihori-restore-test.db \
  'SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;'
```

終了後：

```bash
sudo rm -f /tmp/oimohorihori-restore-test.db
```

---

# 25. 本番DB復旧手順

## 注意

障害時のみ実施する。
通常運用では実行しない。

### 1. バックアップ選択

```bash
sudo ls -lht /var/backups/oimohorihori/
```

整合性確認：

```bash
sudo sqlite3 <backup-file> "PRAGMA integrity_check;"
```

### 2. Server停止

```bash
sudo systemctl stop oimohorihori
```

### 3. 現DB緊急退避

```bash
sudo mkdir -p /var/backups/oimohorihori/pre-restore
```

DB / WAL / SHM を退避してから復旧する。

### 4. 現DB削除

```bash
sudo rm -f \
  /var/lib/oimohorihori/oimohorihori.db \
  /var/lib/oimohorihori/oimohorihori.db-wal \
  /var/lib/oimohorihori/oimohorihori.db-shm
```

### 5. バックアップ復元

```bash
sudo cp <backup-file> \
  /var/lib/oimohorihori/oimohorihori.db
```

権限：

```bash
sudo chown oimohorihori:oimohorihori \
  /var/lib/oimohorihori/oimohorihori.db

sudo chmod 640 \
  /var/lib/oimohorihori/oimohorihori.db
```

### 6. 整合性確認

```bash
sudo sqlite3 \
  /var/lib/oimohorihori/oimohorihori.db \
  "PRAGMA integrity_check;"
```

### 7. Server起動

```bash
sudo systemctl start oimohorihori
```

確認：

```bash
curl -i http://127.0.0.1:5000/api/auth/me
curl -i https://mkunori.com/oimo-api/api/auth/me
```

未ログインなら401で正常。

---

# 26. Server更新用スクリプト

ファイル：

```text
/usr/local/sbin/deploy-oimohorihori.sh
```

想定処理：

```text
DBバックアップ
↓
Server停止
↓
/opt/oimohorihori 更新
↓
権限設定
↓
Server起動
↓
systemd確認
↓
/api/auth/me が401になることを確認
```

今後の更新：

Windows：

```powershell
dotnet publish .\src\OimoHorihori.Server\OimoHorihori.Server.csproj `
  -c Release `
  -r linux-x64 `
  --self-contained false `
  -o .\publish\server

Remove-Item .\oimohorihori-server.tar.gz -ErrorAction SilentlyContinue

tar -czf oimohorihori-server.tar.gz -C .\publish\server .

scp -i $env:USERPROFILE\.ssh\sakura_vps `
  .\oimohorihori-server.tar.gz `
  deploy@<VPS_HOST>:~/
```

VPS：

```bash
sudo /usr/local/sbin/deploy-oimohorihori.sh \
  /home/deploy/oimohorihori-server.tar.gz
```

---

# 27. ログ確認

## ASP.NET Core

```bash
systemctl status oimohorihori --no-pager -l
```

```bash
sudo journalctl -u oimohorihori -n 100 --no-pager
```

直近10分：

```bash
sudo journalctl -u oimohorihori \
  --since "10 minutes ago" \
  --no-pager
```

リアルタイム：

```bash
sudo journalctl -u oimohorihori -f
```

---

# 28. Nginxログ

状態：

```bash
systemctl status nginx --no-pager -l
```

構文：

```bash
sudo nginx -t
```

Error log：

```bash
sudo tail -n 100 /var/log/nginx/error.log
```

Access log：

```bash
sudo tail -n 100 /var/log/nginx/access.log
```

---

# 29. 障害時の最短チェック

まず以下の順番で確認する。

```bash
systemctl status oimohorihori --no-pager -l
```

```bash
sudo ss -lntp | grep ':5000'
```

```bash
curl -i http://127.0.0.1:5000/api/auth/me
```

```bash
sudo nginx -t
```

```bash
curl -i https://mkunori.com/oimo-api/api/auth/me
```

未ログイン時は401が正常。

---

# 30. HTTPステータスの目安

| Status | 意味 |
|---|---|
| 200 / 204 | 正常 |
| 400 | リクエスト不正 |
| 401 | 未ログイン / Token失効 / Admin Key不正 |
| 404 | URL / Endpoint / Nginxパス変換確認 |
| 409 | Revision競合 |
| 500 | ASP.NET Core内部例外 |
| 502 | NginxからKestrelへ到達できない可能性 |

---

# 31. 本番確認済み項目

以下は本番環境で動作確認済み。

- ユーザー登録
- ユーザー名 + 4桁PINログイン
- PasswordHasherによるPINハッシュ
- Bearer Session
- 複数端末ログイン
- Server Save
- Save再取得
- Revision競合防止
- TOTAL POTATOランキング
- BEST POTATO / SECランキング
- REPLANETランキング
- 公開プロフィール
- REPLANET履歴
- 管理者ユーザー一覧
- 管理者PINリセット
- PINリセット時の全Session失効
- 管理者による無効化 / 再有効化
- アカウント削除
- 削除時の最新Server Saveローカル復元
- 削除済みユーザー名の再利用禁止
- 削除後のランキング保持
- Nginx reverse proxy
- HTTPS
- CORS
- SQLite永続化
- SQLiteオンラインバックアップ
- 自動バックアップ
- integrity_check
- 復元テスト

---

# 32. セキュリティ方針

維持すること：

- Kestrelは `127.0.0.1:5000` のみで待ち受ける
- 5000番をインターネットへ直接公開しない
- 外部公開はNginx HTTPSのみ
- Admin KeyをGitHubへコミットしない
- Admin KeyをClientへ含めない
- PINをログへ出さない
- Session Tokenをログへ出さない
- SQLite DBをGitへ含めない
- publish時に本番DBを上書きしない
- Serverは `oimohorihori` 一般サービスユーザーで実行する
- `/opt/oimohorihori` はroot所有
- `/var/lib/oimohorihori` はServerユーザー所有
- `/etc/oimohorihori` はrootのみ読み取り可能
- Nginx設定変更前に必ず `nginx -t`
- DB復元前に必ずServer停止
- DB復元前に必ず現DBを退避
- バックアップ後は `PRAGMA integrity_check;`

---

# 33. 現在の本番構成まとめ

```text
GitHub Pages
https://mkunori.github.io/OimoHorihori/
        │
        │ HTTPS API
        ▼
Nginx
https://mkunori.com/oimo-api/
        │
        │ reverse proxy
        ▼
Kestrel
http://127.0.0.1:5000
        │
        ▼
OimoHorihori.Server
/opt/oimohorihori/
        │
        ▼
SQLite
/var/lib/oimohorihori/oimohorihori.db
```

運用：

```text
systemd
├─ oimohorihori.service
└─ oimohorihori-backup.timer

秘密設定
└─ /etc/oimohorihori/oimohorihori.env

バックアップ
└─ /var/backups/oimohorihori/
```

OIMO HORIHORI v3.0.0 のServer本番構築は、この構成を基準とする。
