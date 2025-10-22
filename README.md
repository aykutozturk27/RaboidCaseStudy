# RaboidCaseStudy (Clean Architecture, .NET 9, MongoDB, JWT)

Bu repo; MongoDB Generic Repository, Register/Login (JWT), Job leasing, EAN‑13 barkod üretimi,
Scheduler (Windows Service uyumlu Worker) ve Mock RPA Client içerir. Case study gereksinimleri temel alınmıştır.

## Çalıştırma

1) MongoDB'yi başlatın:
```bash
docker compose up -d mongo
```

2) API'yi lokal çalıştırın (VS/CLI) veya docker ile:
```bash
# CLI
cd src/API
dotnet run --urls=http://localhost:5080

# veya compose (Dockerfile eklemeden örnek)
docker compose up -d api
```

3) Swagger: `http://localhost:5080/swagger`

4) Mock RPA Client:
```bash
cd src/Client.MockRPA
dotnet run
```

5) Scheduler Worker:
```bash
cd src/Worker.Scheduler
dotnet run
```

## Kimlik Doğrulama
- Register: `POST /api/auth/register` (PBKDF2 hashing, JWT üretir)
- Login: `POST /api/auth/login`

## İş Akışı
- Admin: `POST /api/jobs/enqueue`
- Client: `POST /api/jobs/lease` -> iş kiralar
- Client: `POST /api/jobs/complete/{id}` veya `fail/{id}`
- Barkod: `POST /api/barcodes/next` (atomik, EAN‑13)

## Notlar
- Seed: Admin kullanıcı `admin@raboid.local / Admin123!`
- Varsayılan role: `Client`
- Ayarlar `appsettings.json` üzerinden düzenlenebilir.
