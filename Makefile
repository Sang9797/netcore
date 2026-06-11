.PHONY: run migrate migrate-status test build docker-up docker-up-infra docker-down pentest load-test stress-test postman clean help

help:
	@echo ""
	@echo "CQRS Order Service .NET — Available commands"
	@echo "  make run              Start ASP.NET Core app locally"
	@echo "  make migrate          Run Liquibase database migrations"
	@echo "  make migrate-status   Show pending Liquibase migrations"
	@echo "  make test             Run .NET tests"
	@echo "  make build            Build the service"
	@echo "  make docker-up        Start full stack"
	@echo "  make docker-up-infra  Start postgres, pgbouncer, prometheus, grafana"
	@echo "  make docker-down      Stop containers"
	@echo "  make pentest          Run security checks"
	@echo "  make load-test        Run k6 load test"
	@echo "  make stress-test      Run k6 stress test"
	@echo "  make postman          Generate Postman collection"
	@echo "  make clean            Remove build artifacts"
	@echo ""

run: migrate
	docker compose up -d pgbouncer redis rabbitmq
	set -a && [ -f ./.env ] && . ./.env || true && set +a && \
	ConnectionStrings__Default="Host=$${LOCAL_DB_HOST:-localhost};Port=$${LOCAL_DB_PORT:-5433};Database=$${DB_NAME:-orders_db};Username=$${DB_USERNAME:-$${DB_USER:-orders_user}};Password=$${DB_PASSWORD:-orders_pass};Pooling=true;Maximum Pool Size=20" \
	ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Cqrs.OrderService/Cqrs.OrderService.csproj --urls http://localhost:8080

migrate:
	cp -n .env.example .env 2>/dev/null || true
	docker compose up -d postgres
	docker compose run --rm liquibase update

migrate-status:
	cp -n .env.example .env 2>/dev/null || true
	docker compose up -d postgres
	docker compose run --rm liquibase status --verbose

test:
	ASPNETCORE_ENVIRONMENT=Testing dotnet test CqrsOrderService.slnx

build:
	dotnet publish src/Cqrs.OrderService/Cqrs.OrderService.csproj -c Release -o build/publish

docker-up:
	cp -n .env.example .env 2>/dev/null || true
	docker compose up --build -d
	@echo ""
	@echo "Services started:"
	@echo "  App:        http://localhost:8080"
	@echo "  OpenAPI:    http://localhost:8080/openapi/v1.json"
	@echo "  Prometheus: http://localhost:9091"
	@echo "  Grafana:    http://localhost:3000"

docker-up-infra:
	cp -n .env.example .env 2>/dev/null || true
	docker compose up -d postgres pgbouncer redis rabbitmq prometheus grafana

docker-down:
	docker compose down

pentest:
	@chmod +x scripts/pentest.sh
	@BASE_URL=http://localhost:8080 ./scripts/pentest.sh

K6_PROMETHEUS_RW_SERVER_URL ?= http://localhost:9091/api/v1/write

load-test:
	@command -v k6 >/dev/null 2>&1 || { echo "k6 not found"; exit 1; }
	BASE_URL=$${BASE_URL:-http://localhost:8080} \
	K6_USERNAME=$${K6_USERNAME:-admin} \
	K6_PASSWORD=$${K6_PASSWORD:-admin123} \
	K6_PROMETHEUS_RW_SERVER_URL=$(K6_PROMETHEUS_RW_SERVER_URL) \
	K6_PROMETHEUS_RW_TREND_AS_NATIVE_HISTOGRAM=false \
	K6_PROMETHEUS_RW_TREND_STATS='p(50),p(90),p(95),p(99)' \
	k6 run --out experimental-prometheus-rw k6/load-test.js

stress-test:
	@command -v k6 >/dev/null 2>&1 || { echo "k6 not found"; exit 1; }
	BASE_URL=$${BASE_URL:-http://localhost:8080} \
	K6_USERNAME=$${K6_USERNAME:-admin} \
	K6_PASSWORD=$${K6_PASSWORD:-admin123} \
	K6_PROMETHEUS_RW_SERVER_URL=$(K6_PROMETHEUS_RW_SERVER_URL) \
	K6_PROMETHEUS_RW_TREND_AS_NATIVE_HISTOGRAM=false \
	K6_PROMETHEUS_RW_TREND_STATS='p(50),p(90),p(95),p(99)' \
	k6 run --out experimental-prometheus-rw k6/stress-test.js

postman:
	@chmod +x scripts/gen-postman.sh
	@./scripts/gen-postman.sh

clean:
	dotnet clean CqrsOrderService.slnx
	docker compose down -v 2>/dev/null || true
