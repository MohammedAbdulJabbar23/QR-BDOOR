# Production Deployment

This deployment layout exposes only the public verification flow on the internet:

- Public host: `/verify/:documentId`
- Public API: `/api/verify/:documentId` and `/api/verify/:documentId/pdf`
- Internal staff app: full SPA and authenticated API, bound to `127.0.0.1` by default

## Architecture

- `public-web`: public Nginx gateway for the verification page only
- `internal-web`: internal Nginx gateway for the full staff app
- `api`: ASP.NET Core API
- `postgres`: application database
- `minio`: object storage for QR images and PDFs

## Files

- `docker-compose.prod.yml`: production stack
- `deploy/.env.production.example`: production environment template
- `src/client/nginx.public.conf`: public route restrictions
- `src/client/nginx.internal.conf`: internal full app gateway

## Deploy

1. Copy `deploy/.env.production.example` to `.env` in the repository root.
2. Replace every `CHANGE_ME` value.
3. Set `PUBLIC_BASE_URL` to the public verification domain that should appear in QR codes.
4. Set `INTERNAL_BASE_URL` to the staff-only domain or URL.
5. Start the stack:

```bash
docker compose -f docker-compose.prod.yml up -d --build
```

## Exposure model

- Public internet should point only to `PUBLIC_APP_PORT` or to a reverse proxy in front of `public-web`.
- `internal-web` is intentionally bound to `127.0.0.1`, so it is not internet-accessible by default.
- If staff need remote access, publish `internal-web` through a VPN, SSH tunnel, Tailscale, or a private reverse proxy.
- `api`, `postgres`, and the MinIO S3 API are not published to the public internet.

## First boot behavior

- The API now applies pending EF Core migrations on startup.
- The API also creates the configured MinIO bucket automatically if it does not exist.

## Recommended DNS / TLS

- Public verification: `verify.your-domain.com`
- Internal staff app: `staff.your-domain.local` or a private hostname behind VPN

Terminate TLS in your outer reverse proxy or load balancer. Keep the internal app on a private network.
