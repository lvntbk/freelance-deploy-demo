# Freelance Deploy Demo API

ASP.NET Core, PostgreSQL, Docker ve GitHub Actions kullanılarak geliştirilmiş modern backend ve deploy demo projesi.

## Özellikler

* ASP.NET Core Web API
* PostgreSQL entegrasyonu
* Entity Framework Core
* Docker & Docker Compose
* Swagger API dokümantasyonu
* GitHub Actions CI/CD
* Health Check endpointi
* RESTful CRUD yapısı

## Kullanılan Teknolojiler

* .NET 8
* ASP.NET Core
* PostgreSQL
* Entity Framework Core
* Docker
* Docker Compose
* GitHub Actions

## Projeyi Çalıştırma

```bash id="sh9j4d"
docker compose up --build
```

Swagger:

```text id="k8d1fj"
http://localhost:8080/swagger
```

## API Endpointleri

### Clients

* GET /api/clients
* POST /api/clients

### Projects

* GET /api/projects
* POST /api/projects
* PUT /api/projects/{id}/status

### Health

* GET /api/health

## Proje Amacı

Bu proje; backend geliştirme, containerization, PostgreSQL yönetimi ve deploy süreçleri üzerine modern bir demo ortamı oluşturmak amacıyla geliştirilmiştir.
