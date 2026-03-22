# Deployment Guide — Al-Badour Hospital Document System

## Server

| Item | Value |
|------|-------|
| Host | `192.168.41.10` |
| User | `cytech` |
| OS   | Ubuntu 24.04 LTS |
| Path | `/opt/QR-BDOOR-master/` |

---

## Architecture

```
Internet / LAN
      │
   nginx (host)
      ├── budoor-hospital.com (443)         → public-web  :8081  (/verify/* only)
      └── qrcode.budoor-hospital.local (443) → internal-web :8082 (full app)

Docker Compose (docker-compose.prod.yml)
  ├── public-web   — React (QR verification page only, nginx.public.conf)
  ├── internal-web — React (full admin app, nginx.internal.conf)
  ├── api          — .NET 9 WebApi :8080 (internal Docker network only)
  ├── postgres     — PostgreSQL 16
  └── minio        — MinIO object storage (PDFs, QR images)
```

All ports except 8081/8082 are bound to 127.0.0.1 or internal Docker network only.

---

## Deploying a New Version

### 1. Sync source code from dev machine

```bash
sshpass -p 'PASSWORD' rsync -az \
  --exclude='**/bin/' --exclude='**/obj/' \
  --exclude='**/node_modules/' --exclude='**/dist/' \
  /home/kira/Documents/freelance/bdor-hospital/src/ \
  cytech@192.168.41.10:/tmp/bdor-src/

sshpass -p 'PASSWORD' ssh cytech@192.168.41.10 \
  'echo PASSWORD | sudo -S cp -r /tmp/bdor-src/. /opt/QR-BDOOR-master/src/'
```

### 2. Rebuild images and restart containers

```bash
ssh cytech@192.168.41.10
cd /opt/QR-BDOOR-master
echo PASSWORD | sudo -S docker compose -f docker-compose.prod.yml \
  build --no-cache api internal-web public-web
echo PASSWORD | sudo -S docker compose -f docker-compose.prod.yml \
  up -d api internal-web public-web
```

### 3. Verify deployment

```bash
sudo docker ps
sudo docker logs qr-bdoor-master-api-1 --tail 20
curl -o /dev/null -w "%{http_code}" http://127.0.0.1:8082/         # 200 (internal app)
curl -o /dev/null -w "%{http_code}" http://127.0.0.1:8081/verify/x # 200 (public QR page)
curl -o /dev/null -w "%{http_code}" http://127.0.0.1:8081/login    # 404 (blocked - correct)
```

---

## Database Migrations

Migrations run automatically on API startup. To run manually:

```bash
sudo docker exec -it qr-bdoor-master-api-1 \
  dotnet ef database update --no-build
```

Check applied migrations:

```bash
sudo docker exec qr-bdoor-master-postgres-1 \
  psql -U albadour -d albadour \
  -c "SELECT migration_id FROM __ef_migrations_history ORDER BY migration_id;"
```

---

## Environment Variables

File: `/opt/QR-BDOOR-master/.env`

| Variable | Purpose |
|----------|---------|
| `POSTGRES_DB/USER/PASSWORD` | Database credentials |
| `MINIO_ROOT_USER/PASSWORD` | MinIO credentials |
| `JWT_KEY` | JWT signing secret (keep private) |
| `PUBLIC_BASE_URL` | External URL embedded in QR codes |
| `INTERNAL_BASE_URL` | Internal URL for CORS |
| `PUBLIC_APP_PORT` | public-web port (default 8081) |

---

## nginx

Config: `/etc/nginx/sites-available/qrcode`

SSL certs:
- Internal: `/etc/ssl/internal/qrcode.crt` + `qrcode.key`
- External (Cloudflare): `/etc/ssl/bdor/cloudflare-origin.pem` + `.key`

```bash
echo PASSWORD | sudo -S nginx -t && echo PASSWORD | sudo -S systemctl reload nginx
```

---

## MinIO Console

Only accessible from localhost. SSH tunnel to reach it:

```bash
ssh -L 9001:127.0.0.1:9001 cytech@192.168.41.10
# open http://localhost:9001
```

---

## Logs

```bash
sudo docker logs -f qr-bdoor-master-api-1      # API (live)
sudo tail -f /var/log/nginx/error.log           # nginx errors
```

---

## Restart / Stop

```bash
cd /opt/QR-BDOOR-master
echo PASSWORD | sudo -S docker compose -f docker-compose.prod.yml restart
echo PASSWORD | sudo -S docker compose -f docker-compose.prod.yml down
echo PASSWORD | sudo -S docker compose -f docker-compose.prod.yml up -d
```
