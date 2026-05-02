# Hướng dẫn chạy `ChargePoint.CarManagement` bằng Docker

## 1) Chuẩn bị

- Cài `Docker` và `Docker Compose`.
- Mở terminal tại thư mục: `ChargePoint.CarManagement` (nơi có `docker-compose.yml`).
- Chọn file môi trường:
  - Local: `.env.local`
  - VPS: `.env.vps`

## 2) Build image

- Local:

```bash
docker compose --env-file .env.local build
```

- VPS:

```bash
docker compose --env-file .env.vps build
```

## 3) Chạy container

- Local:

```bash
docker compose --env-file .env.local up -d
```

- VPS:

```bash
docker compose --env-file .env.vps up -d
```

## 4) Kiểm tra trạng thái

```bash
docker compose ps
```

- Mặc định app chạy tại: `http://localhost:8080`

## 5) Xem log

```bash
docker compose logs -f app
```

## 6) Dừng và xoá container

```bash
docker compose down
```

Nếu muốn xoá luôn volume dữ liệu:

```bash
docker compose down -v
```

## 7) Lỗi thường gặp

### Lỗi `COPY ... .csproj not found` khi build

Nguyên nhân: `build.context` chưa đúng với cấu trúc solution nhiều project.

Sửa trong `docker-compose.yml`:

```yaml
services:
  app:
    build:
      context: ..
      dockerfile: ChargePoint.CarManagement/Dockerfile
```

Sau đó build lại:

```bash
docker compose --env-file .env.local build --no-cache
```

### Lỗi MySQL không kết nối được

- Nếu app chạy trong Docker và MySQL chạy trên host:
  - dùng `Server=host.docker.internal` trong `ConnectionStrings__DefaultConnection`
  - giữ `extra_hosts: "host.docker.internal:host-gateway"` trong `docker-compose.yml`

- Nếu MySQL chạy trong container khác cùng compose:
  - dùng `Server=<service_mysql>` (ví dụ `Server=mysql`), không dùng `localhost`.
