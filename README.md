# Freelance Deploy Demo API

![CI](https://github.com/lvntbk/freelance-deploy-demo/actions/workflows/docker.yml/badge.svg)

ASP.NET Core ve PostgreSQL ile müşteri/proje yönetimi API'si.
Docker ile çalıştırılır; API davranışları gerçek PostgreSQL kullanan
entegrasyon testleriyle kontrol edilir.

## Özellikler

- Müşteri ve proje oluşturma, listeleme ve detay görüntüleme
- İstek/yanıt DTO'ları ve alan doğrulaması
- Proje durumunu JSON isteğiyle güncelleme
- Olmayan kayıtlar için 404, geçersiz girdiler için 400 yanıtları
- Oluşturulan kayıt için 201 Created ve Location başlığı
- Müşteri başına proje sayısı
- EF Core migration'ları
- Testcontainers ile 8 entegrasyon testi
- GitHub Actions üzerinde test ve Docker derleme kontrolü

## Teknolojiler

.NET 8, ASP.NET Core, EF Core 8, PostgreSQL 16,
Docker Compose, xUnit, Testcontainers ve GitHub Actions.

## Yerel kurulum — Ubuntu

Gereksinimler: Docker/Compose, .NET 8 veya üzeri SDK,
.NET 8 çalışma zamanı, Python 3 ve dotnet-ef.

dotnet-ef kurulu değilse proje ile aynı EF sürümünü kur:

```bash
dotnet tool install --global dotnet-ef --version 8.0.11
```

### 1. Ortam dosyası ve PostgreSQL

```bash
cp -n .env.example .env
# PostgreSQL'i ilk kez başlatmadan önce örnek parolayı düzenle.
nano .env
docker compose up -d postgres
docker compose exec -T postgres sh -c 'pg_isready -U "$POSTGRES_USER" -d "$POSTGRES_DB"'
```

Son komut `accepting connections` yazana kadar PostgreSQL'in açılmasını bekle.

Örnek dosya host portu olarak 5434 kullanır. Port doluysa
`.env` içindeki `POSTGRES_HOST_PORT` değerini değiştir.
API, Compose ağı içinde veritabanına `postgres:5432` üzerinden bağlanır.

### 2. Migration'ı uygula

Bu adım yeni veritabanında tabloları oluşturur.
Idempotent script, migration geçmişini kontrol eder.

```bash
dotnet ef migrations script --idempotent --output /tmp/freelance-deploy-demo-migration.sql

python3 - <<'PY'
from pathlib import Path
path = Path("/tmp/freelance-deploy-demo-migration.sql")
path.write_text(path.read_text(encoding="utf-8-sig"), encoding="utf-8")
PY

docker compose exec -T postgres sh -c 'psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -d "$POSTGRES_DB"' < /tmp/freelance-deploy-demo-migration.sql
```

Script oluşturma veya uygulama adımı hata verirse önce hatayı çöz.

### 3. API'yi başlat

```bash
docker compose up -d --build api
```

API hazır olduğunda:

- Swagger: http://localhost:8080/swagger
- Müşteriler: http://localhost:8080/api/clients
- Projeler: http://localhost:8080/api/projects

Örnek istekler `FreelanceDeployDemo.API.http` dosyasındadır.
POST yanıtlarından aldığın ID'lerle dosyadaki değişkenleri güncelle.

## API uç noktaları

| Metot | Yol | İşlev |
| --- | --- | --- |
| GET | `/api/clients` | Müşterileri listele |
| GET | `/api/clients/{id}` | Müşteri detayı |
| POST | `/api/clients` | Müşteri oluştur |
| GET | `/api/projects` | Projeleri listele |
| GET | `/api/projects/{id}` | Proje detayı |
| POST | `/api/projects` | Proje oluştur |
| PUT | `/api/projects/{id}/status` | Proje durumunu güncelle |
| GET | `/api/health` | Mevcut sağlık kontrolü |

Durum güncelleme gövdesi:

```json
{
  "status": "InProgress"
}
```

Geçerli değerler: `Pending`, `InProgress`, `Completed`, `Cancelled`.
Durumlar arasındaki geçiş kuralları henüz uygulanmıyor.

## Entegrasyon testleri

Docker çalışır durumdayken:

```bash
dotnet test tests/FreelanceDeployDemo.IntegrationTests/FreelanceDeployDemo.IntegrationTests.csproj
```

Testler ayrı PostgreSQL container'ları ve otomatik atanmış portlar kullanır.
Migration'lar test başlangıcında uygulanır; container'lar test sonunda temizlenir.
Test veritabanı bağlantısı test altyapısı tarafından oluşturulur.

Kapsam: müşteri/proje akışı, SQL sorguları, ilişkiler, alan doğrulaması,
olmayan kayıtlar ve geçersiz durumun mevcut kaydı değiştirmemesi.

## CI

`main` hedefli PR'larda ve `main` push'larında:
bağımlılıkları yükle → entegrasyon testlerini çalıştır → Docker imajını derle.

Workflow sunucuya dağıtım yapmaz.

## Mevcut sınırlar

- Kimlik doğrulama ve sayfalama henüz yok.
- Health endpoint'i veritabanı bağlantısı başarısızken de 200 dönebilir.
- Yerel kurulumda migration ayrı bir adımla uygulanır.

Render demo bağlantısı:
https://freelance-deploy-demo.onrender.com/swagger/index.html
