# Fiap.Payments

API de pagamentos da plataforma. Ela **consome os eventos de pedido realizado** publicados no RabbitMQ (fila `queue-order-placed`) e processa os pagamentos. O resultado do pagamento é consumido pelo serviço de **Notifications**, que envia o e-mail de confirmação quando o pagamento é aprovado.

O serviço **não possui banco de dados**: toda a comunicação com os demais serviços é feita pelo RabbitMQ.

## Estrutura do projeto

```
.
├── Fiap.Payments.Api/           # API ASP.NET Core 8 (consumidores, Swagger e configuração)
│   └── Dockerfile
├── Fiap.Payments.Domain/        # Eventos e regras de domínio
├── docker-compose.yml           # Ambiente local (RabbitMQ + API)
└── k8s/                         # Manifestos Kubernetes
    ├── 01-deployment-payments.yaml
    └── 02-svc-payments-api.yaml
```

## Pré-requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) (execução local sem container)
- [Docker](https://www.docker.com/) e Docker Compose
- [kubectl](https://kubernetes.io/docs/tasks/tools/) e um cluster local (kind, minikube ou Docker Desktop) para o deploy em Kubernetes

## Configuração

Em variáveis de ambiente, o `:` das configurações do `appsettings` vira `__`.

| Variável | Descrição | Exemplo |
|---|---|---|
| `ASPNETCORE_URLS` | Endereço de escuta da API | `http://+:8084` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente da aplicação | `Development` |
| `RabbitMQ__HostName` | Host do RabbitMQ | `rabbitmq` (Compose) / `rabbitmq-service` (Kubernetes) |
| `RabbitMQ__Port` | Porta AMQP | `5672` |
| `RabbitMQ__UserName` | Usuário do RabbitMQ | `admin` |
| `RabbitMQ__Password` | Senha do RabbitMQ | `admin1234` |
| `RabbitMQ__VirtualHost` | Virtual host | `/` |

## Executando com Docker Compose

O `docker-compose.yml` sobe o RabbitMQ (com volume e healthcheck) e a API, que só inicia depois que o RabbitMQ estiver saudável.

```bash
docker compose up --build -d
docker compose logs -f api-payments
docker compose down            # para os containers (mantém o volume do RabbitMQ)
docker compose down -v         # para e apaga o volume do RabbitMQ
```

| Serviço | URL |
|---|---|
| Swagger da API | http://localhost:8084/swagger/index.html |
| RabbitMQ (painel) | http://localhost:15672 (usuário `admin`, senha `admin1234`) |
| RabbitMQ (AMQP) | `localhost:5672` |

## Docker

O `Dockerfile` usa **multi-stage build** (`base` → `build` → `publish` → `final`): o SDK 8.0 compila e publica, e a imagem final usa apenas o runtime ASP.NET 8.0. O contexto de build é a **raiz da solução**, porque o Dockerfile copia os projetos `Api` e `Domain`.

```bash
docker build -f Fiap.Payments.Api/Dockerfile -t <usuario>/src-api-payments:latest .
docker push <usuario>/src-api-payments:latest
```

A aplicação roda com o usuário não-root `app` e expõe a porta `8084`.

## Executando no Kubernetes

### Pré-requisito: RabbitMQ no cluster

Os manifestos deste projeto **não criam o RabbitMQ**. O Deployment depende do ConfigMap `configmap-rabbitmq` e do Secret `rabbitmq-credentials`, que vêm do projeto **Fiap.Notifications** (`rabbitmq-service`). Aplique-os antes, senão o pod fica em `CreateContainerConfigError`.

### Recursos

| Arquivo | Recurso | Descrição |
|---|---|---|
| `01-deployment-payments.yaml` | Deployment `payments-api-deployment` | API na porta 8084, com readiness probe TCP |
| `02-svc-payments-api.yaml` | Service `payments-api` | `ClusterIP`, porta **8094** → container 8084 |

### Aplicar

```bash
kubectl apply -f k8s/
kubectl get pod,svc,endpoints
```

Para remover tudo: `kubectl delete -f k8s/`.

### Comunicação entre serviços

Dentro do cluster, os serviços se encontram pelo **nome do Service**:

| Serviço | Endereço no cluster | Porta do container |
|---|---|---|
| users-api | `http://users-api:8091` | 8081 |
| catalog-api | `http://catalog-api:8092` | 8082 |
| notifications-api | `http://notifications-api:9093` | 8083 |
| payments-api | `http://payments-api:8094` | 8084 |
| rabbitmq-service | `rabbitmq-service:5672` | 5672 |

### Acessando da sua máquina

Nomes de Service só existem dentro do cluster. Para abrir no navegador:

1. Adicione o nome ao arquivo `hosts` (edite como administrador):

   ```
   # Windows: C:\Windows\System32\drivers\etc\hosts
   127.0.0.1 payments-api
   ```

2. Mantenha um port-forward ativo, em um terminal separado:

   ```bash
   kubectl port-forward svc/payments-api 8094:8094
   ```

3. Acesse http://payments-api:8094/swagger/index.html.

Alternativa sem editar o `hosts`: http://127.0.0.1:8094/swagger/index.html.

### Verificação

```bash
kubectl get pod,svc,endpoints
kubectl logs deploy/payments-api-deployment -f
```

Quando o consumidor estiver ativo, os logs mostram `Aguardando mensagens da fila 'queue-order-placed'...`. As mensagens `Tentativa N/10 falhou` no início indicam que o RabbitMQ ainda estava subindo, e a aplicação tenta reconectar.

## Problemas comuns

| Sintoma | Causa provável | O que fazer |
|---|---|---|
| Pod em `CreateContainerConfigError` | `configmap-rabbitmq` ou `rabbitmq-credentials` não existem | Aplicar o Secret, o ConfigMap e o RabbitMQ do Notifications antes do Deployment |
| `Tentativa N/10 falhou: None of the specified endpoints were reachable` | RabbitMQ ainda subindo ou host incorreto | Aguardar. Se persistir, conferir `RabbitMQ__HostName` |
| `DNS_PROBE_FINISHED_NXDOMAIN` ou `ping` não encontra `payments-api` | O nome não está no `hosts` | Adicionar a entrada (veja acima) e rodar `ipconfig /flushdns` |
| `ERR_CONNECTION_REFUSED` | Port-forward parado ou nome apontando para outro IP | Subir o `port-forward` e conferir com `ping payments-api` |
| `Unable to listen on port ...: bind` | Porta já em uso (port-forward antigo) | Encerrar o processo `kubectl` anterior |
| `Endpoints` do Service como `<none>` | Selector não bate ou pod não está `Ready` | Conferir `app: svc-payments-api` e `kubectl describe pod` |

## Pontos de atenção

- **Sem espera pelo RabbitMQ no Kubernetes:** diferente do Notifications, o Deployment não tem `initContainer`. Se o pod subir antes do RabbitMQ ficar pronto, a aplicação precisa reconectar (como mostram as tentativas nos logs).
- **Senha do RabbitMQ no repositório:** o `docker-compose.yml` e o Secret do Notifications trazem `admin1234`. Serve para estudo. Em produção, crie os Secrets por `kubectl create secret` e não os versione.
- **Ambiente `Development`:** o Dockerfile e o Deployment fixam `ASPNETCORE_ENVIRONMENT=Development`. Em produção, use `Production`.
- **Imagem `:latest`:** com `imagePullPolicy: Always`, dificulta o rollback. Prefira uma tag de versão.
- **Sem `resources` nem `livenessProbe`:** o Deployment só tem a readiness probe. Defina `requests/limits` e uma liveness probe para um uso além de estudo.
- **Portas diferentes:** no Compose a API responde em `8084`, e no Kubernetes o Service responde em `8094`. O container escuta `8084` nos dois casos.
- **Versão do RabbitMQ:** o Compose usa `3.11-management` e o Kubernetes (projeto Notifications) usa `3.13-management`.
- **APIS:** para simular o processo completo é preciso baixar as apis: catalog, users e notifications.
